using AutoMapper;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using PB.DatabaseFramework;
using PB.EntityFramework;
using PB.Model;
using PB.Shared.Enum;
using PB.Shared.Enum.Inventory;
using PB.Shared.Models;
using PB.Shared.Models.Inventory.Item;
using PB.Shared.Models.Inventory.Items;
using PB.Shared.Tables;
using PB.Shared.Tables.Inventory.Items;
using QRCoder;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace PB.Server.Repository
{
    public interface IItemRepository
    {

        #region Item

        Task<int> SaveItem(ItemSingleModel itemMainModel, IDbTransaction? tran = null);
        Task<ItemSingleModel> GetItemById(int itemID, IDbTransaction? tran = null);
        Task<ItemSingleModel?> GetItemByCode(string itemCode, int clientID, IDbTransaction? tran = null);
        Task<int> GetNonExistingQrCodeCount(int clientID);
        Task<IdnValuePair> CreateItemQrCode(int itemID, IDbTransaction? tran = null);
        Task InitiateClientItemsQrCodePDFGeneration(int clientID, int currentEntityID, ItemQrCodeGenerationPostModel postModel);
        Task InitiateClientItemGroupsQrCodePDFGeneration(int clientID, int currentEntityID, ItemQrCodeGenerationPostModel postModel);
        Task ImportItems(ItemImportPostModel fileUploadModel, int clientID, int userEntityID);

        #endregion

        #region Item Model

        Task SaveItemModels(List<ItemVariantModel> itemModels, int itemID, int clientID, IDbTransaction? tran = null);
        Task<List<ItemVariantModel>> GetItemVariants(int itemID, IDbTransaction? tran = null);
        Task<ItemVariantModel> GetItemModel(int itemModelID, IDbTransaction? tran = null);
        Task<ItemVariantImageModel> GetItemModelImage(int itemModelID, IDbTransaction? tran = null);
        Task<List<ItemVariantImageModel>> GetItemModelImages(int itemModelID, IDbTransaction? tran = null);
        Task<ItemVariantDetail> GetItemModelDetails(int itemModelID, int currentBranchID, int placeOfSupplyID);

        #endregion

    }

    public class ItemRepository : IItemRepository
    {
        private readonly IDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IPDFRepository _pdf;
        private readonly INotificationRepository _notification;
        private readonly IConfiguration _config;
        public ItemRepository(IDbContext dbContext, IMapper mapper, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, IPDFRepository pdf, INotificationRepository notification, IConfiguration config)
        {
            this._dbContext = dbContext;
            this._mapper = mapper;
            this._configuration = configuration;
            this._hostingEnvironment = webHostEnvironment;
            this._pdf = pdf;
            this._notification = notification;
            this._config = config;
        }

        #region Item

        public async Task<int> SaveItem(ItemSingleModel itemMainModel, IDbTransaction? tran = null)
        {
            //if qr code generation is activated then, checking item code change and changing the qrCode image respectively
            bool isItemCodeChanged = false;
            bool? clientCodeSettings = await _dbContext.GetFieldsAsync<ClientSetting, bool?>("ItemQrCodeState", "ClientID=@ClientID", new { ClientID = itemMainModel.ClientID }, tran);
            if (itemMainModel.ItemID > 0)
            {
                var existingItemCode = await _dbContext.GetFieldsAsync<Item, string>("ItemCode", $"ItemID={itemMainModel.ItemID}", null, tran);
                if (existingItemCode != itemMainModel.ItemCode)
                    isItemCodeChanged = true;
            }
            var item = _mapper.Map<Item>(itemMainModel);
            item.ItemID = await _dbContext.SaveAsync(item, tran);

            //Item images
            if (itemMainModel.ItemImages.Count > 0)
            {
                var itemImage = _mapper.Map<List<ItemImage>>(itemMainModel.ItemImages);
                await _dbContext.SaveSubItemListAsync(itemImage, "ItemID", item.ItemID, tran);
            }

            //Generating qrCode for new item or item code changed items
            if ((isItemCodeChanged || itemMainModel.ItemID == 0) && (clientCodeSettings is not null && clientCodeSettings.Value))
            {
                if (item.QrCodeMediaID is not null)
                {
                    await _dbContext.ExecuteAsync($"Update Media Set IsDeleted=1 Where MediaID={item.QrCodeMediaID.Value}", null, tran);
                    string filePath = _config["ServerURL"] + itemMainModel.QrCodeFileName;
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }
                IdnValuePair qrCodeResult = await CreateItemQrCode(item.ItemID, tran);
                await _dbContext.ExecuteAsync("Update Item Set QrCodeMediaID=@QrCodeMediaID Where ItemID=@ItemID", new { QrCodeMediaID = qrCodeResult.ID, ItemID = item.ItemID }, tran);
            }

            //Item variants section
            List<ItemVariantModel> itemModels = new();
            if (!itemMainModel.HasMultipleModels)
            {
                ItemVariantModel defaultItemModel = new()
                {
                    ItemVariantID = itemMainModel.ItemVariantID,
                    ItemID = itemMainModel.ItemID,
                    UrlCode = itemMainModel.UrlCode,
                    PackingTypeID = itemMainModel.PackingTypeID,
                    UMUnit = itemMainModel.UMUnit,
                    Price = itemMainModel.Price,
                    Cost = itemMainModel.Cost,
                    SizeID = itemMainModel.SizeID,
                    Size = itemMainModel.Size,
                    PackingTypeName = itemMainModel.PackingTypeName,
                    ColorID = itemMainModel.ColorID,
                    ColorName = itemMainModel.ColorName,
                    ItemVariantImages = itemMainModel.DefaultItemVariantImages
                };
                itemModels.Add(defaultItemModel);
            }
            else if (itemMainModel.ItemVariants is not null)
            {
                itemModels.AddRange(itemMainModel.ItemVariants);
            }
            await SaveItemModels(itemModels, item.ItemID, itemMainModel.ClientID.Value, tran);
            return item.ItemID;
        }
        public async Task<ItemSingleModel> GetItemById(int itemID, IDbTransaction? tran = null)
        {
            var item = await _dbContext.GetByQueryAsync<ItemSingleModel>($@"
                                                                Select I.*,TP.TaxPreferenceName,INTRA.TaxCategoryName AS IntraTaxCategoryName,INTER.TaxCategoryName AS InterTaxCategoryName,E.Name AS AddedByUserName,H.Code As HsnCode,S.Code As SacCode,M.FileName As QrCodeFileName,
                                                                IG.GroupName,IC.CategoryID,IC.CategoryName,IB.BrandID,IB.BrandName
                                                                From Item I
                                                                Left Join TaxPreference TP ON TP.TaxPreferenceTypeID=I.TaxPreferenceTypeID AND I.IsDeleted=0
                                                                Left Join viTaxCategory INTRA ON INTRA.TaxCategoryID=I.IntraTaxCategoryID
                                                                Left Join viTaxCategory INTER ON INTER.TaxCategoryID=I.InterTaxCategoryID
                                                                Left Join viEntity E ON E.EntityID=I.AddedBy
                                                                Left Join HSN H ON I.HsnID=H.ID
                                                                Left Join SAC S ON I.SacID=S.ID
                                                                Left Join ItemCategory IC ON IC.CategoryID=I.CategoryID
                                                                Left Join ItemGroup IG ON IG.GroupID=I.GroupID
                                                                Left Join ItemBrand IB ON IB.BrandID=I.BrandID
                                                                Left Join Media M ON I.QrCodeMediaID=M.MediaID And M.IsDeleted=0
                                                                Where I.ItemID={itemID} AND I.IsDeleted=0", null, tran);
            item.ItemImages = await GetItemImages(itemID, tran);
            item.ItemVariants = await GetItemVariants(itemID, tran);
            foreach (var itemModel in item.ItemVariants)
            {
                itemModel.ItemVariantImages = await GetItemModelImages(itemModel.ItemVariantID, tran);
            }

            //Single model
            if (item.ItemVariants is not null && item.ItemVariants.Count == 1)
            {
                item.ItemVariantID = item.ItemVariants[0].ItemVariantID;
                item.ItemModelName = item.ItemVariants[0].ItemModelName;
                item.ItemID = item.ItemID;
                item.PackingTypeID = item.ItemVariants[0].PackingTypeID;
                item.UrlCode = item.ItemVariants[0].UrlCode;
                item.PackingTypeName = item.ItemVariants[0].PackingTypeName;
                item.UMUnit = item.ItemVariants[0].UMUnit;
                item.Price = item.ItemVariants[0].Price;
                item.Cost = item.ItemVariants[0].Cost;
                item.SizeID = item.ItemVariants[0].SizeID;
                item.Size = item.ItemVariants[0].Size;
                item.ColorID = item.ItemVariants[0].ColorID;
                item.ColorName = item.ItemVariants[0].ColorName;
                item.DefaultItemVariantImages = item.ItemVariants[0].ItemVariantImages;
                item.ItemVariants = null;
                item.HasMultipleModels = false;
            }

            if (!string.IsNullOrEmpty(item.QrCodeFileName))
                item.QrCodeFileName = _configuration["ServerURL"] + item.QrCodeFileName;
            return item ?? new();
        }
        public async Task<ItemSingleModel?> GetItemByCode(string itemCode, int clientID, IDbTransaction? tran = null)
        {
            if (!string.IsNullOrEmpty(itemCode))
            {
                int itemID = await _dbContext.GetFieldsAsync<Item, int>("ItemID", "ItemCode=@ItemCode And ClientID=@ClientID", new { ItemCode = itemCode, ClientID = clientID }, tran);
                return await GetItemById(itemID, tran);
            }
            return null;
        }
        public async Task<int> GetNonExistingQrCodeCount(int clientID)
        {
            return await _dbContext.GetByQueryAsync<int>($"Select Count(*) From Item Where ClientID={clientID} And IsDeleted=0 And QrCodeMediaID IS NULL", null);
        }
        public async Task InitiateClientItemsQrCodePDFGeneration(int clientID, int currentEntityID, ItemQrCodeGenerationPostModel postModel)
        {
            ItemQrCodeGeneratedPushModel pushModel = new() { DeviceID = postModel.DeviceID };
            List<int> qrCodeNotGeneratedItemIDs = await _dbContext.GetListFieldsAsync<Item, int>("ItemID", $"QrCodeMediaID IS NULL And ClientID={clientID} And IsDeleted=0 And IsGoods=1", null);
            string? pdfName = await _dbContext.GetByQueryAsync<string>(@$"Select M.FileName 
                                                                        From ClientSetting CS
                                                                        Join Media M ON M.MediaID=CS.ItemQrPdfMediaID
                                                                        Where CS.ClientID={clientID} And CS.IsDeleted=0", null);

            if (qrCodeNotGeneratedItemIDs.Count > 0 || string.IsNullOrEmpty(pdfName))
            {
                if (qrCodeNotGeneratedItemIDs.Count > 0)
                {
                    foreach (var itemID in qrCodeNotGeneratedItemIDs)
                    {
                        await CreateItemQrCode(itemID);
                    }
                    pushModel.IsNewQrCodeAdded = true;
                }
                pdfName = await _pdf.GetItemQrCodePdfFile(clientID);
            }

            if (!string.IsNullOrEmpty(pdfName))
            {
                pushModel.FileName = _configuration["ServerURL"] + pdfName;
                await _notification.SendSignalRPush(currentEntityID, pushModel, null, "item_qr_codes_pdf_generated");
            }
        }
        public async Task InitiateClientItemGroupsQrCodePDFGeneration(int clientID, int currentEntityID, ItemQrCodeGenerationPostModel postModel)
        {
            ItemQrCodeGeneratedPushModel pushModel = new() { DeviceID = postModel.DeviceID };
            List<int> qrCodeNotGeneratedItemGroupIDs = await _dbContext.GetListFieldsAsync<ItemGroup, int>("ItemGroupID", $"QrCodeMediaID IS NULL And ClientID={clientID} And IsDeleted=0", null);
            string? pdfName = await _dbContext.GetByQueryAsync<string>(@$"Select M.FileName 
                                                                            From ClientSetting CS
                                                                            Join Media M ON M.MediaID=CS.ItemGroupQrCodeMediaID
                                                                            Where CS.ClientID={clientID} And CS.IsDeleted=0", null);

            if (qrCodeNotGeneratedItemGroupIDs.Count > 0 || string.IsNullOrEmpty(pdfName))
            {
                if (qrCodeNotGeneratedItemGroupIDs.Count > 0)
                {
                    foreach (var itemGroupID in qrCodeNotGeneratedItemGroupIDs)
                    {
                        await CreateItemGroupQrCode(itemGroupID);
                    }
                    pushModel.IsNewQrCodeAdded = true;
                }
                pdfName = await _pdf.GetItemGroupQrCodePdfFile(clientID);
            }

            if (!string.IsNullOrEmpty(pdfName))
            {
                pushModel.FileName = _configuration["ServerURL"] + pdfName;
                await _notification.SendSignalRPush(currentEntityID, pushModel, null, "item_qr_codes_pdf_generated");
            }
        }
        public async Task<IdnValuePair> CreateItemQrCode(int itemID, IDbTransaction? tran = null)
        {
            IdnValuePair result = new();
            string itemCode = await _dbContext.GetFieldsAsync<Item, string>("ItemCode", $"ItemID={itemID}", null, tran);
            Bitmap qrCodeBitmap = GetQrCodeBitmapObject(itemCode);
            if (qrCodeBitmap is not null)
            {
                result = await SaveQrCodeImageToServer(qrCodeBitmap, itemID, tran);
                await _dbContext.ExecuteAsync($"Update Item Set QrCodeMediaID=@QrCodeMediaID Where ItemID={itemID}", new { QrCodeMediaID = result.ID }, tran);
            }
            return result;
        }
        public async Task<IdnValuePair> CreateItemGroupQrCode(int itemGroupID, IDbTransaction? tran = null)
        {
            IdnValuePair result = new();
            string groupName = await _dbContext.GetFieldsAsync<Item, string>("GroupName", $"GroupID={itemGroupID}", null, tran);
            Bitmap qrCodeBitmap = GetQrCodeBitmapObject(groupName);
            if (qrCodeBitmap is not null)
            {
                result = await SaveQrCodeImageToServer(qrCodeBitmap, itemGroupID, tran, false);
                await _dbContext.ExecuteAsync($"Update ItemGroup Set QrCodeMediaID=@QrCodeMediaID Where GroupID={itemGroupID}", new { QrCodeMediaID = result.ID }, tran);
            }
            return result;
        }
        private Bitmap GetQrCodeBitmapObject(string itemCode)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(itemCode, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }
        private async Task<IdnValuePair> SaveQrCodeImageToServer(Bitmap bitmap, int id, IDbTransaction? tran = null, bool isItemQrCode = true)
        {
            byte[] imageBytes;
            int? mediaID = null;
            string mediaFileName = "";
            string fileName = "";
            string filePath = "";
            string folderName = "";

            if (isItemQrCode)
            {
                folderName = "gallery/ItemQRCode";
                if (!Directory.Exists(Path.Combine("wwwroot", folderName)))
                {
                    Directory.CreateDirectory(Path.Combine("wwwroot", folderName));
                }
                fileName = $"{id}-item-qr-image.png";
                mediaFileName = "/" + folderName + "/" + fileName;
                filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", folderName, fileName);
                using (MemoryStream stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Png);
                    imageBytes = stream.ToArray();
                    File.WriteAllBytes(filePath, imageBytes);
                }
                mediaID = await _dbContext.GetFieldsAsync<Item, int?>("QrCodeMediaID", $"ItemID={id}", null, tran);
            }
            else
            {
                folderName = "gallery/ItemGroupQRCode";
                if (!Directory.Exists(Path.Combine("wwwroot", folderName)))
                {
                    Directory.CreateDirectory(Path.Combine("wwwroot", folderName));
                }
                fileName = $"{id}-item-grup-qr-image.png";
                mediaFileName = "/" + folderName + "/" + fileName;
                filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", folderName, fileName);
                using (MemoryStream stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Png);
                    imageBytes = stream.ToArray();
                    File.WriteAllBytes(filePath, imageBytes);
                }
                mediaID = await _dbContext.GetFieldsAsync<ItemGroup, int?>("QrCodeMediaID", $"GroupID={id}", null, tran);
            }

            if (mediaID is null)
            {
                Media media = new()
                {
                    FileName = mediaFileName,
                    ContentLength = imageBytes.Length,
                    ContentType = "image/png",
                    Extension = "png"
                };
                mediaID = await _dbContext.SaveAsync(media, tran);
            }
            return new() { ID = mediaID.Value, Value = mediaFileName };
        }
        public async Task ImportItems(ItemImportPostModel fileUploadModel, int clientID, int userEntityID)
        {
            List<ItemImportSkippedModel> skippedItems = new();
            List<string> itemImages = new();
            string folderName = "gallery/import/";
            if (!Directory.Exists(Path.Combine("wwwroot", folderName)))
            {
                Directory.CreateDirectory(Path.Combine("wwwroot", folderName));
            }
            string filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", folderName);
            filePath = filePath + "/" + "import_item_" + clientID.ToString() + "." + fileUploadModel.Extension;
            string sFileExtension = Path.GetExtension(filePath).ToLower();
            ISheet sheet;
            var stream = System.IO.File.Create(filePath);
            stream.Write(fileUploadModel.Content, 0, fileUploadModel.Content.Length);
            using (stream)
            {
                stream.Position = 0;
                if (sFileExtension == ".xls")
                {
                    HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                    sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  
                }
                else
                {
                    XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                    sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   
                }

                //Headr row validation
                IRow headerRow = sheet.GetRow(0); //Get Header Row
                int cellCount = headerRow.LastCellNum;

                //Reading row values
                for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null) continue;
                    if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                    int columnCount = row.LastCellNum;
                    if (columnCount >= 3)
                    {
                        ItemSingleModel itemSingleModel = new()
                        {
                            ItemName = row.GetCell(0).ToString(),
                            ItemCode = row.GetCell(1).ToString(),
                            TaxPreferenceTypeID = (int)TaxPreferences.NonTaxable,
                            IsGoods = true,
                            HasMultipleModels = false,
                            PackingTypeID = (int)PackingTypes.Piece,
                            AddedBy = userEntityID,
                            ClientID = clientID,
                        };
                        var price = row.GetCell(2).ToString();
                        if (!string.IsNullOrEmpty(price))
                             itemSingleModel.Price = Convert.ToDecimal(price);
                        else
                            itemSingleModel.Price = 0;

                        var groupName = await _dbContext.GetByQueryAsync<string?>($"Select Top 1 GroupName From ItemGroupData Where ItemCode=@ItemCode", new { ItemCode = itemSingleModel.ItemCode });
                        if (!string.IsNullOrEmpty(groupName))
                        {
                            itemSingleModel.GroupID = await _dbContext.GetByQueryAsync<int?>($"Select GroupID From ItemGroup Where GroupName=@GroupName", new { GroupName = groupName });
                        }
                        else
                        {
                            itemSingleModel.GroupID = await _dbContext.GetByQueryAsync<int?>($"Select GroupID From ItemGroup Where GroupName='' Or GroupName Is Null", null);
                        }

                        //var itemName = await _dbContext.GetByQueryAsync<string>(@$"Select ItemName 
                        //                                            from Item
                        //                                            where ItemName=@ItemName and ItemID<>@ItemID and ClientID={clientID} and IsDeleted=0", itemSingleModel);
                        //if (itemName != null)
                        //{
                        //    skippedItems.Add(new()
                        //    {
                        //        RowNumber = i + 1,
                        //        Description = "ItemName : " + itemSingleModel.ItemName + " is already exist in the table"
                        //    });
                        //    continue;
                        //}

                        var itemCode = await _dbContext.GetByQueryAsync<string>(@$"Select ItemCode 
                                                                    from Item
                                                                    where ItemCode=@ItemCode and ItemID<>@ItemID and IsDeleted=0 and ClientID={clientID}", itemSingleModel);

                        if (itemCode != null)
                        {
                            skippedItems.Add(new()
                            {
                                RowNumber = i + 1,
                                Description = "ItemCode : " + itemSingleModel.ItemCode + " is already exist in the table"
                            });
                            continue;
                        }

                        //string pattern = @"^[A-Za-z0-9-]+$";
                        //Regex regex = new Regex(pattern);
                        //if (!string.IsNullOrEmpty(itemSingleModel.ItemCode) && !regex.IsMatch(itemSingleModel.ItemCode))
                        //{
                        //    skippedItems.Add(new()
                        //    {
                        //        RowNumber = i + 1,
                        //        Description = "ItemCode : " + itemSingleModel.ItemCode + "The item code must consist of uppercase letters, digits, and hyphens only."
                        //    });
                        //    continue;
                        //}

                        #region Item Model

                        itemImages = new();
                        //Item model images
                        switch (columnCount)
                        {
                            case 4:
                                if (!string.IsNullOrEmpty(row.GetCell(3).ToString()))
                                    itemImages.Add(row.GetCell(3).ToString());
                                break;
                            case 5:
                                if (!string.IsNullOrEmpty(row.GetCell(3).ToString()))
                                    itemImages.Add(row.GetCell(3).ToString());
                                if (!string.IsNullOrEmpty(row.GetCell(4).ToString()))
                                    itemImages.Add(row.GetCell(4).ToString());
                                break;
                            case 6:
                                if (!string.IsNullOrEmpty(row.GetCell(3).ToString()))
                                    itemImages.Add(row.GetCell(3).ToString());
                                if (!string.IsNullOrEmpty(row.GetCell(4).ToString()))
                                    itemImages.Add(row.GetCell(4).ToString());
                                if (!string.IsNullOrEmpty(row.GetCell(5).ToString()))
                                    itemImages.Add(row.GetCell(5).ToString());
                                break;
                            case 7:
                                if (!string.IsNullOrEmpty(row.GetCell(3).ToString()))
                                    itemImages.Add(row.GetCell(3).ToString());
                                if (!string.IsNullOrEmpty(row.GetCell(4).ToString()))
                                    itemImages.Add(row.GetCell(4).ToString());
                                if (!string.IsNullOrEmpty(row.GetCell(5).ToString()))
                                    itemImages.Add(row.GetCell(5).ToString());
                                if (!string.IsNullOrEmpty(row.GetCell(6).ToString()))
                                    itemImages.Add(row.GetCell(6).ToString());
                                break;
                            case 8:
                                if (!string.IsNullOrEmpty(row.GetCell(3).ToString()))
                                    itemImages.Add(row.GetCell(3).ToString());
                                if (!string.IsNullOrEmpty(row.GetCell(4).ToString()))
                                    itemImages.Add(row.GetCell(4).ToString());
                                if (!string.IsNullOrEmpty(row.GetCell(5).ToString()))
                                    itemImages.Add(row.GetCell(5).ToString());
                                if (!string.IsNullOrEmpty(row.GetCell(6).ToString()))
                                    itemImages.Add(row.GetCell(6).ToString());
                                if (!string.IsNullOrEmpty(row.GetCell(7).ToString()))
                                    itemImages.Add(row.GetCell(7).ToString());
                                break;
                        }

                        if (itemImages.Count > 0)
                        {
                            foreach (var itemImageFileName in itemImages)
                            {
                                string targetFolder = Path.Combine(_hostingEnvironment.WebRootPath, "gallery", "ItemImage");
                                string imageFilePath = Path.Combine(targetFolder, itemImageFileName);
                                string fileExtension = String.Empty;
                                string contentType = String.Empty;
                                if (System.IO.File.Exists(imageFilePath))
                                {
                                    byte[] imageBytes = System.IO.File.ReadAllBytes(imageFilePath);
                                    fileExtension = System.IO.Path.GetExtension(imageFilePath);
                                    contentType = GetContentType(fileExtension);

                                    Media itemImageMedia = new()
                                    {
                                        FileName = $"/gallery/ItemImage/{itemImageFileName}",
                                        ContentType = contentType,
                                        ContentLength = imageBytes.Length,
                                        Extension = fileExtension
                                    };

                                    //Item image
                                    itemImageMedia.MediaID = await _dbContext.SaveAsync(itemImageMedia);
                                    itemSingleModel.ItemImages.Add(new()
                                    {
                                        MediaID = itemImageMedia.MediaID
                                    });

                                    //Default item variant image
                                    itemSingleModel.DefaultItemVariantImages.Add(new()
                                    {
                                        MediaID = itemImageMedia.MediaID
                                    });
                                }
                            }
                        }

                        #endregion

                        itemSingleModel.ItemID = await SaveItem(itemSingleModel);
                    }
                }
            }

            //Response push
            ItemImportResponsePushModel pushModel = new()
            {
                DeviceID = fileUploadModel.DevideID,
                SkippedItems = skippedItems
            };
            await _notification.SendSignalRPush(userEntityID, pushModel, null, "item_import_success");
        }
        private string GetContentType(string fileExtension)
        {
            return fileExtension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                // Add more cases as needed for other image types
                _ => "application/octet-stream", // Default to binary data if the type is unknown
            };
        }

        #endregion

        #region Item Model

        public async Task SaveItemModels(List<ItemVariantModel> itemModels, int itemID, int clientID, IDbTransaction? tran = null)
        {
            if (itemModels.Count > 0)
            {
                foreach (var itemModelView in itemModels)
                {
                    if (!string.IsNullOrEmpty(itemModelView.UrlCode))
                    {
                        var urlCode = await _dbContext.GetByQueryAsync<string?>(@$"Select UrlCode
                                                                                From viItem I
                                                                                Where I.ClientID={clientID} And ItemVariantID<>{itemModelView.ItemVariantID} And UrlCode=@UrlCode", new { UrlCode = itemModelView.UrlCode }, tran);

                        if (urlCode is not null)
                            throw new PBException("InvalidInput", $"UrlCode '{itemModelView.UrlCode}' for another item model");
                    }

                    var itemModel = _mapper.Map<ItemVariant>(itemModelView);
                    itemModel.ItemID = itemID;
                    itemModel.ItemVariantID = await _dbContext.SaveAsync(itemModel, tran);
                    //Item Model Images
                    if (itemModelView.ItemVariantImages.Count > 0)
                    {
                        var itemModelImages = _mapper.Map<List<ItemVariantImage>>(itemModelView.ItemVariantImages);
                        itemModelImages.ForEach(itemModelImage => itemModelImage.ItemVariantID = itemModel.ItemVariantID);
                        await _dbContext.SaveListAsync(itemModelImages, $"ItemVariantID={itemModel.ItemVariantID}", tran, true);
                    }
                }
            }
        }
        public async Task<List<ItemVariantModel>> GetItemVariants(int itemID, IDbTransaction? tran = null)
        {
            var itemVariants  = await _dbContext.GetListByQueryAsync<ItemVariantModel>(@$"Select IM.ItemVariantID,IM.ItemID,IM.ItemName As ItemModelName,IM.UrlCode,IM.PackingTypeID,IM.UMUnit,IM.Price,IM.Cost,IM.SizeID,S.Size,P.PackingTypeName,IM.ColorID,C.ColorName,IM.TaxPreferenceTypeID,IM.TaxPreferenceName,IM.CurrentStock,IM.IsGoods
                                                                            From viItem IM
                                                                            Left Join ItemPackingType P ON P.PackingTypeID=IM.PackingTypeID AND P.IsDeleted=0
                                                                            Left Join ItemSize S ON S.SizeID=IM.SizeID AND S.IsDeleted=0
                                                                            Left Join ItemColor C ON C.ColorID=IM.ColorID
                                                                            Where IM.ItemID = {itemID}", null, tran);
            foreach(var itemVariant in itemVariants)
            {
                itemVariant.ItemVariantImages = await _dbContext.GetListByQueryAsync<ItemVariantImageModel>(@$"Select IVI.*,M.FileName
                                                                                                                From ItemVariantImage IVI
                                                                                                                Left Join Media M ON M.MediaID=IVI.MediaID
                                                                                                                Where IVI.IsDeleted=0 And IVI.ItemVariantID={itemVariant.ItemVariantID}", null);
                itemVariant.ItemVariantImages.ForEach(image => image.FileName = _configuration["ServerURL"] + image.FileName);
            }
            return itemVariants;
        }
        public async Task<ItemVariantModel> GetItemModel(int itemModelID, IDbTransaction? tran = null)
        {
            return await _dbContext.GetByQueryAsync<ItemVariantModel>(@$"Select IM.ItemVariantID,IM.ItemID,IM.ItemName As ItemModelName,IM.UrlCode,IM.PackingTypeID,IM.UMUnit,IM.Price,IM.Cost,IM.SizeID,S.Size,P.PackingTypeName,IM.ColorID,C.ColorName,IM.TaxPreferenceTypeID,IM.TaxPreferenceName,IM.CurrentStock,IM.IsGoods
                                                                            From viItem IM
                                                                            Left Join ItemPackingType P ON P.PackingTypeID=IM.PackingTypeID AND P.IsDeleted=0
                                                                            Left Join ItemSize S ON S.SizeID=IM.SizeID AND S.IsDeleted=0
                                                                            Left Join ItemColor C ON C.ColorID=IM.ColorID
                                                                            Where IM.ItemVariantID = {itemModelID}", null, tran);
        }
        public async Task<ItemVariantImageModel> GetItemModelImage(int itemModelID, IDbTransaction? tran = null)
        {
            var result = await _dbContext.GetByQueryAsync<ItemVariantImageModel>(@$"Select Top 1 IM.*,M.FileName
                                                                From ItemVariantImage IM
                                                                Join Media M ON M.MediaID=IM.MediaID And M.IsDeleted=0
                                                                Where IM.ItemVariantID={itemModelID} And IM.IsDeleted=0", null);
            result = result ?? new();
            result.FileName = _configuration["ServerURL"] + result.FileName;
            return result;
        }
        public async Task<List<ItemVariantImageModel>> GetItemModelImages(int itemModelID, IDbTransaction? tran = null)
        {
            var result = await _dbContext.GetListByQueryAsync<ItemVariantImageModel>(@$"Select IM.*,M.FileName
                                                                From ItemVariantImage IM
                                                                Join Media M ON M.MediaID=IM.MediaID And M.IsDeleted=0
                                                                Where IM.ItemVariantID={itemModelID} And IM.IsDeleted=0", null);
            result.ForEach(itemImage => itemImage.FileName = _configuration["ServerURL"] + itemImage.FileName);
            return result;
        }

        public async Task<List<ItemImageModel>> GetItemImages(int itemID, IDbTransaction? tran = null)
        {
            var result = await _dbContext.GetListByQueryAsync<ItemImageModel>(@$"Select IM.*,M.FileName
                                                                From ItemImage IM
                                                                Join Media M ON M.MediaID=IM.MediaID And M.IsDeleted=0
                                                                Where IM.ItemID={itemID} And IM.IsDeleted=0", null);
            result.ForEach(itemImage => itemImage.FileName = _configuration["ServerURL"] + itemImage.FileName);
            return result;
        }
        public async Task<ItemVariantDetail> GetItemModelDetails(int itemModelID, int currentBranchID, int placeOrSourceOfSupplyID)
        {
            ItemVariantDetail result = await _dbContext.GetByQueryAsync<ItemVariantDetail>(@$"Select IM.ItemVariantID,IM.ItemName,IM.Description,IM.Price,IM.TaxPreferenceTypeID,IM.TaxPreferenceName,IM.CurrentStock,IM.IsGoods,
                                                                                            T.TaxCategoryID,T.TaxCategoryName,T.TaxPercentage
                                                                                            From viItem IM 
                                                                                            Left Join viBranch B ON B.BranchID={currentBranchID}
                                                                                            Left Join viTaxCategory T ON T.TaxCategoryID=
                                                                                            Case 
	                                                                                            When B.StateID={placeOrSourceOfSupplyID} Then IM.IntraTaxCategoryID
	                                                                                            When B.StateID<>{placeOrSourceOfSupplyID} Then IM.InterTaxCategoryID
	                                                                                            End 
                                                                                            Where IM.ItemVariantID={itemModelID}", null);
            if (result.TaxCategoryID is not null)
            {
                result.TaxCategoryItems = await _dbContext.GetListByQueryAsync<TaxCategoryItemModel>($@"
                                                                            Select TaxCategoryItemID,Concat(TaxCategoryItemName,'   [ ',Convert(smallint,Percentage),' ]') AS TaxCategoryItemName,Percentage,0 As Amount
                                                                            From TaxCategoryItem T
                                                                            Where T.TaxCategoryID={result.TaxCategoryID} AND T.IsDeleted=0", null);
            }
            return result ?? new();
        }

        #endregion

    }
}
