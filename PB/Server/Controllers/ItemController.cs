using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PB.Shared.Models;
using PB.Model;
using PB.Shared.Models.Common;
using System.Data;
using System.Reflection;
using PB.Model.Models;
using PB.EntityFramework;
using PB.Server.Repository;
using PB.Shared.Tables.Inventory.Items;
using NPOI.SS.Formula.Functions;
using PB.DatabaseFramework;
using System.IO;
using PB.Shared.Models.Inventory.Item;
using PB.Shared.Models.Inventory.Items;
using PB.Shared.Tables.CRM;
using PB.Client.Pages.SuperAdmin.Client;
using PB.Shared.Enum;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HPSF;
using PB.Shared.Enum.Common;
using Microsoft.AspNetCore.Hosting;
using PB.Shared.Enum.Inventory;
using System.Text.RegularExpressions;
using System.Net.Mail;
using Hangfire;
using PB.Shared.Tables;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using PB.Model.Enums;
using static NPOI.HSSF.Util.HSSFColor;
using PB.Shared;
using PB.Shared.Resources;

namespace PB.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ItemController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection _cn;
        private readonly IMapper _mapper;
        private readonly ICRMRepository _crm;
        private readonly IItemRepository _item;
        private readonly IMediaRepository _media;
        private readonly ICommonRepository _common;
        private readonly IHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly INotificationRepository _notification;
        private readonly IPDFRepository _pdf;
        private readonly IBackgroundJobClient _job;

        public ItemController(IDbContext dbContext, IDbConnection cn, IMapper mapper, ICRMRepository crm, IItemRepository item, IMediaRepository media, ICommonRepository common, IHostEnvironment env, IConfiguration config, IWebHostEnvironment hostingEnvironment, INotificationRepository notification, IPDFRepository pdf, IBackgroundJobClient job)
        {
            _dbContext = dbContext;
            _cn = cn;
            _mapper = mapper;
            _crm = crm;
            _item = item;
            _media = media;
            _common = common;
            _env = env;
            _config = config;
            _hostingEnvironment = hostingEnvironment;
            _notification = notification;
            _pdf = pdf;
            _job = job;
        }

        #region Item

        [HttpPost("save-item")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Item")]
        public async Task<IActionResult> SaveItem(ItemSingleModel itemSingleModel)
        {
            var itemName = await _dbContext.GetByQueryAsync<string>(@$"Select ItemName 
                                                                    from Item
                                                                    where ItemName=@ItemName and ItemID<>@ItemID and ClientID={CurrentClientID} and IsDeleted=0", itemSingleModel);
            if (itemName != null)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Item name already exist",
                    ResponseMessage = "The Item name already exist,try different name"
                });
            }

            var itemCode = await _dbContext.GetByQueryAsync<string>(@$"Select ItemCode 
                                                                    from Item
                                                                    where ItemCode=@ItemCode and ItemID<>@ItemID and IsDeleted=0 and ClientID={CurrentClientID}", itemSingleModel);
            if (itemCode != null)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Item code already exist",
                    ResponseMessage = "Item code already exist, use different name and try again.."
                });
            }

            itemSingleModel.ClientID = CurrentClientID;
            itemSingleModel.AddedBy = itemSingleModel.ItemID == 0 ? CurrentEntityID : itemSingleModel.AddedBy;
            var returnObject = new ItemAddResultModel()
            {
                ItemName = itemSingleModel.ItemName,
                ResponseTitle = "SaveSuccess",
                ResponseMessage = "ItemSavedSucessfully"
            };

            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                returnObject.ItemID = await _item.SaveItem(itemSingleModel, tran);
                tran.Commit();
            }

            if (!itemSingleModel.HasMultipleModels)
                returnObject.ItemVariantID = await _dbContext.GetByQueryAsync<int>($@"Select ItemVariantID From ItemVariant Where ItemID={returnObject.ItemID} And IsDeleted=0", null);

            return Ok(returnObject);
        }

        [HttpGet("get-item/{itemID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Item")]
        public async Task<IActionResult> GetItem(int itemID)
        {
            return Ok(await _item.GetItemById(itemID));
        }

        [HttpGet("delete-item/{Id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Item")]
        public async Task<IActionResult> DeleteItem(int Id)
        {
            int count = await _dbContext.GetByQueryAsync<int>($@"SELECT Count(*) AS count 
                                                                            FROM Enquiry E
                                                                            LEFT JOIN EnquiryItem EI ON E.EnquiryID = EI.EnquiryID 
                                                                            LEFT JOIN ViItem VI ON VI.ItemVariantID=EI.ItemVariantID 
                                                                            WHERE  E.CurrentFollowupNature IN(0,1) and E.IsDeleted =0 and VI.ItemID={Id}
                                                                            GROUP BY E.CurrentFollowupNature", null);

            if (count > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidOperation",
                    ResponseMessage = "ItemUsedInEnquiry"
                });
            }

            count = await _dbContext.GetByQueryAsync<int>($@"SELECT Count(*) AS count 
                                                                        FROM Quotation Q
                                                                        LEFT JOIN QuotationItem QI ON Q.QuotationID = QI.QuotationID 
                                                                        LEFT JOIN ViItem VI ON VI.ItemVariantID=QI.ItemVariantID 
                                                                        WHERE  Q.CurrentFollowupNature IN(0,1) and Q.IsDeleted =0 and VI.ItemID={Id}
                                                                        GROUP BY Q.CurrentFollowupNature", null);

            if (count > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidOperation",
                    ResponseMessage = "ItemUsedInQuotation"
                });
            }

            count = await _dbContext.GetByQueryAsync<int>($@"SELECT Count(*) AS count 
                                                                        FROM Invoice I
                                                                        LEFT JOIN InvoiceItem II ON I.InvoiceID = II.InvoiceID 
                                                                        LEFT JOIN ViItem VI ON VI.ItemVariantID=II.ItemVariantID 
                                                                        WHERE I.IsDeleted =0 and VI.ItemID={Id}", null);

            if (count > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidOperation",
                    ResponseMessage = "ItemUsedInInvoice"
                });
            }

            var model = await _dbContext.GetAsync<Item>(Id);
            var logSummaryId = await _dbContext.InsertDeleteLogSummary(model.ItemID, "Item " + model.ItemName + " deleted by User : " + CurrentUserID);

            await _dbContext.ExecuteAsync($"UPDATE Item SET IsDeleted=1 Where ItemID={Id}");
            await _dbContext.ExecuteAsync($"UPDATE ItemVariant SET IsDeleted=1 Where ItemID={Id}");
            //Qr code
            IdnValuePair itemQrCodesPdf = await _dbContext.GetByQueryAsync<IdnValuePair>($@"Select M.MediaID As ID,M.FileName As Value
                                                                                    From ClientSetting CS
                                                                                    Join Media M ON M.MediaID=CS.ItemQrPdfMediaID
                                                                                    Where CS.ClientID={CurrentClientID}", null);

            if (itemQrCodesPdf is not null)
            {
                await _dbContext.ExecuteAsync($"Update ClientSetting Set ItemQrPdfMediaID=NULL Where ClientID={CurrentClientID}");
                string filePath = _config["ServerURL"] + itemQrCodesPdf.Value;
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            //Item Image--


            return Ok(true);
        }

        [HttpPost("get-hsn-sac-search-list")]
        public async Task<IActionResult> GetHsnSacSearchResult(HsnSacSearchModel searchModel)
        {
            string selectQuery = "";
            string whereCondition = "";
            if (!string.IsNullOrEmpty(searchModel.SearchString))
            {
                selectQuery = searchModel.IsGoods ? "Select * From HSN " : "Select * From SAC ";
                whereCondition = " Where IsDeleted=0 And Code Like @SearchString Or Description Like @SearchString";
            }
            else
            {
                selectQuery = searchModel.IsGoods ? "Select TOP 20 * From HSN " : "Select TOP 20 * From SAC ";
                whereCondition = " Where IsDeleted=0";
            }

            var result = await _dbContext.GetListByQueryAsync<HsnSacModel>(selectQuery + whereCondition, new { SearchString = searchModel.SearchString + '%' });
            return Ok(result);
        }

        [HttpGet("get-selected-hsn-result/{hsnID}")]
        public async Task<IActionResult> GetHsnResult(int hsnID)
        {
            return base.Ok(await _dbContext.GetByQueryAsync<Shared.Models.Inventory.Item.HsnSacModel>($"Select * From HSN Where ID={hsnID}", null));
        }

        [HttpGet("get-selected-sac-result/{sacID}")]
        public async Task<IActionResult> GetSacResult(int sacID)
        {
            return base.Ok(await _dbContext.GetByQueryAsync<Shared.Models.Inventory.Item.HsnSacModel>($"Select * From SAC Where ID={sacID}", null));
        }

        [HttpGet("get-item-menu-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Item")]
        public async Task<IActionResult> GetItemMenuList()
        {
            var result = await _dbContext.GetListByQueryAsync<ViewPageMenuModel>($@"Select ItemID as ID,ItemName as MenuName,
                                                                                    Case 
                                                                                        When ItemID is not null Then 0 
                                                                                        Else 1 
                                                                                        End As IsChecked
                                                                                    From Item
                                                                                    Where IsDeleted=0 and ClientID={CurrentClientID}
                                                                                    Order By MenuName ASC", null);
            return Ok(result);
        }

        [HttpPost("get-items-paged-list")]

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Item")]
        public async Task<IActionResult> GetItems(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;
            query.Select = $@"Select I.ItemID,I.ItemName,I.ItemCode,I.IsGoods,TP.TaxPreferenceName,TP.TaxPreferenceTypeID,
                                    Case When I.IsGoods=1 Then H.Code Else S.Code End As HSNSAC,Concat('{_config["ServerURL"]}/',M.FileName) As QrCodeFileName
                                    From Item I
                                    Left Join TaxPreference TP on I.TaxPreferenceTypeID=TP.TaxPreferenceTypeID and TP.IsDeleted=0
                                    Left Join HSN H ON I.HSNID=H.ID And H.IsDeleted=0
                                    Left Join SAC S ON I.SACID=S.ID And S.IsDeleted=0
                                    Left Join Media M ON M.MediaID=I.QrCodeMediaID";
            query.WhereCondition = $"I.IsDeleted=0 and I.ClientID={CurrentClientID}";
            query.OrderByFieldName = model.OrderByFieldName;
            query.WhereCondition += _common.GeneratePagedListFilterWhereCondition(model, "I.ItemName");
            var res = await _dbContext.GetPagedList<ItemListModel>(query, null);
            return Ok(res);
        }

        [HttpPost("get-updated-item-details-list")]

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Quotation")]
        public async Task<IActionResult> GetUpdatedItemDetails(PLaceOfSupplyChangedPostModel model)
        {
            try
            {
                string fetchQuery = "";
                string ItemModelIDs = string.Join(',', model.ItemVariantIDList);

                List<QuotationItemModel> quotationItems = new();

                if (model.PlaceOfSupplyID != null)
                {
                    fetchQuery = @$"Select IM.ItemVariantID,IM.ItemName,IM.Description,IM.Price As Rate,
								T.TaxCategoryID,T.TaxCategoryName,T.TaxPercentage
								From viItem IM 
								Left Join Item I ON IM.ItemID=I.ItemID AND I.IsDeleted=0
								Left Join EntityAddress EA ON EA.EntityID = {CurrentClientID} AND EA.IsDeleted=0
								Left Join viTaxCategory T ON T.TaxCategoryID=
								Case 
									When EA.StateID={model.PlaceOfSupplyID} AND I.IntraTaxCategoryID IS NOT NULL Then I.IntraTaxCategoryID
									When EA.StateID<>{model.PlaceOfSupplyID} AND I.InterTaxCategoryID IS NOT NULL Then I.InterTaxCategoryID
									End 
								Where IM.ItemVariantID in ({ItemModelIDs})";
                }
                else
                {
                    fetchQuery = @$"Select IM.ItemVariantID,IM.ItemName,IM.Description,IM.Price As Rate
                                From viItem IM 
                                Where IM.ItemVariantID in ({ItemModelIDs})";
                }

                quotationItems = await _dbContext.GetListByQueryAsync<QuotationItemModel>(fetchQuery, null);

                if (quotationItems != null)
                {
                    foreach (var quotationItem in quotationItems)
                    {
                        quotationItem.TotalAmount = quotationItem.Quantity * quotationItem.Rate;
                        quotationItem.NetAmount = quotationItem.TotalAmount - quotationItem.Discount;
                        quotationItem.TaxAmount = quotationItem.NetAmount * quotationItem.TaxPercentage / 100;
                        quotationItem.GrossAmount = quotationItem.NetAmount + quotationItem.TaxAmount;

                        if (quotationItem.TaxCategoryID != null)
                        {
                            quotationItem.TaxCategoryItems = await _dbContext.GetListByQueryAsync<QuotationItemTaxCategoryItemsModel>($@"
                                                                            Select TaxCategoryItemID,Concat(TaxCategoryItemName,'   [ ',Convert(smallint,Percentage),' ]') AS TaxCategoryItemName,Percentage,0 As Amount
                                                                            From TaxCategoryItem T
                                                                            Where T.TaxCategoryID={quotationItem.TaxCategoryID} AND T.IsDeleted=0
                            ", null);

                            if (quotationItem.TaxCategoryItems.Count > 0)
                            {
                                quotationItem.TaxCategoryItems.ForEach(taxCategoryItem => taxCategoryItem.Amount = quotationItem.NetAmount * (taxCategoryItem.Percentage / 100));
                            }
                        }
                    }
                }

                return Ok(quotationItems ?? new());
            }
            catch (Exception e)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseTitle = "Something went wrong",
                    ResponseMessage = e.Message,
                });
            }

        }

        [HttpGet("get-item-import-sample-excel")]
        public async Task<IActionResult> GetItemImportSampleExcelFile()
        {
            string? DomainUrl = _config.GetValue<string>("ServerURL");
            IWorkbook workbook;
            workbook = new XSSFWorkbook();
            ISheet excelSheet = workbook.CreateSheet("Item import example file");
            excelSheet.DefaultColumnWidth = 20;
            // Create cell style for header row
            ICellStyle headerStyle = workbook.CreateCellStyle();
            headerStyle.FillForegroundColor = IndexedColors.BlueGrey.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            headerStyle.BorderBottom = BorderStyle.Thin;
            headerStyle.BottomBorderColor = IndexedColors.White.Index;
            // Create font for header row
            IFont headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerFont.Color = IndexedColors.White.Index;
            headerFont.FontHeightInPoints = 12;
            headerStyle.SetFont(headerFont);

            #region SetColumnWidth

            excelSheet.SetColumnWidth(0, 10 * 256);
            excelSheet.SetColumnWidth(1, 15 * 256);
            excelSheet.SetColumnWidth(2, 10 * 256);
            excelSheet.SetColumnWidth(3, 10 * 256);
            excelSheet.SetColumnWidth(4, 23 * 256);
            excelSheet.SetColumnWidth(5, 10 * 256);
            excelSheet.SetColumnWidth(6, 15 * 256);
            excelSheet.SetColumnWidth(7, 10 * 256);

            #endregion

            IRow row = excelSheet.CreateRow(0);
            excelSheet.DefaultRowHeightInPoints = 20;
            row.CreateCell(0).SetCellValue("ItemName");
            row.CreateCell(1).SetCellValue("ItemCode");
            row.CreateCell(2).SetCellValue("Price");
            row.CreateCell(3).SetCellValue("Image1");
            row.CreateCell(4).SetCellValue("Image2");
            row.CreateCell(5).SetCellValue("Image3");
            row.CreateCell(6).SetCellValue("Image4");
            row.CreateCell(7).SetCellValue("Image5");
            byte[]? fileContents = null;
            using (var memoryStream = new MemoryStream())
            {
                workbook.Write(memoryStream);
                fileContents = memoryStream.ToArray();
            }
            return Ok(fileContents);
        }

        [HttpPost("import-items")]
        public async Task<IActionResult> ImportItems(ItemImportPostModel fileUploadModel)
        {
            await _item.ImportItems(fileUploadModel, CurrentClientID, CurrentEntityID);
            //_job.Enqueue(() => _item.ImportItems(fileUploadModel, CurrentClientID, CurrentEntityID));
            return Ok(new BaseSuccessResponse() { ResponseMessage = "ItemImportBackGroundProcessStarted" });
        }

        [HttpGet("get-non-existing-qr-code-count")]
        public async Task<IActionResult> GetNonExistingQrCodeCount()
        {
            return Ok(await _item.GetNonExistingQrCodeCount(CurrentClientID));
        }

        [HttpPost("generate-all-items-qr-code")]
        public async Task<IActionResult> GenerateAllItemsQrCode(ItemQrCodeGenerationPostModel postModel)
        {
            //await _item.InitiateClientItemsQrCodePDFGeneration(CurrentClientID, CurrentEntityID, postModel);
            _job.Enqueue(() => _item.InitiateClientItemsQrCodePDFGeneration(CurrentClientID, CurrentEntityID, postModel));
            return Ok(new BaseSuccessResponse() { ResponseMessage = "ItemQRBackGroundProcessStarted" });
        }

        [HttpPost("generate-all-item-groups-qr-code")]
        public async Task<IActionResult> GenerateAllItemGroupQrCodes(ItemQrCodeGenerationPostModel postModel)
        {
            //await _item.InitiateClientItemsQrCodePDFGeneration(CurrentClientID, CurrentEntityID, postModel);
            _job.Enqueue(() => _item.InitiateClientItemGroupsQrCodePDFGeneration(CurrentClientID, CurrentEntityID, postModel));
            return Ok(new BaseSuccessResponse() { ResponseMessage = "ItemQRBackGroundProcessStarted" });
        }

        [HttpGet("generate-item-qr-code/{itemID}")]
        public async Task<IActionResult> GenerateItemQrCode(int itemID)
        {
            var result = await _item.CreateItemQrCode(itemID);
            result.Value = _config["ServerURL"] + result.Value;
            return Ok(result);
        }


        [HttpPost("save-item-image")]
        public async Task<IActionResult> SaveItemImage(ItemImageModel itemImageModel)
        {
            var itemModelImage = _mapper.Map<ItemImage>(itemImageModel);
            itemModelImage.ID = await _dbContext.SaveAsync(itemModelImage);
            return Ok(itemModelImage.ID);
        }

        [HttpGet("remove-item-image/{imageID}")]
        public async Task<IActionResult> RemveItemMediaById(int imageID)
        {
            IdnValuePair media = await _dbContext.GetByQueryAsync<IdnValuePair>($@"Select M.MediaID As ID,M.FileName As Value
                                                                                    From ItemImage IM
                                                                                    Join Media M ON M.MediaID=IM.MediaID
                                                                                    Where IM.ID={imageID}", null);

            await _dbContext.ExecuteAsync($"Update ItemImage Set IsDeleted=1 Where ID={imageID}");
            await _dbContext.ExecuteAsync($"Update Media Set IsDeleted=1 Where MediaID={media.ID}");
            //not getting the complete actual path
            if (media.Value is not null)
            {
                string contentRootPath = _env.ContentRootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string fileName = media.Value.Substring(media.Value.LastIndexOf('/') + 1);
                string filePath = Path.Combine(contentRootPath, "wwwroot", "gallery", "ItemImage", fileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            return Ok(true);
        }

        [HttpPost("import-item-group")]
        public async Task<IActionResult> ImportItemGroup(FileUploadModel fileUploadModel)
        {
            string folderName = "gallery/ImportItemGoup/";
            if (!Directory.Exists(Path.Combine("wwwroot", folderName)))
            {
                Directory.CreateDirectory(Path.Combine("wwwroot", folderName));
            }
            string filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", folderName);
            filePath = filePath + "/" + "import_item_" + CurrentClientID.ToString() + "." + fileUploadModel.Extension;
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

                    string groupName = "";
                    string itemCode = ""; 
                    var group = row.GetCell(2);
                    var code = row.GetCell(1);
                    if (group is not null)
                        groupName = group.ToString();
                    if (code is not null)
                        itemCode = code.ToString();

                    //checking group name is empty and already exist then do nothing
                    if (string.IsNullOrEmpty(groupName))
                    {
                        var itemGroup = await _dbContext.GetByQueryAsync<ItemGroup>($"Select * From ItemGroup Where GroupName Is Null Or GroupName='' And ClientID={CurrentClientID}", null);
                        if (itemGroup is null)
                        {
                            itemGroup = new()
                            {
                                GroupName = null,
                                ClientID = CurrentClientID
                            };
                            itemGroup.GroupID = await _dbContext.SaveAsync(itemGroup);
                        }
                    }

                    //checking group is not empty and checking already saved group
                    if (!string.IsNullOrEmpty(groupName))
                    {
                        var itemGroup = await _dbContext.GetByQueryAsync<ItemGroup>($"Select * From ItemGroup Where GroupName=@GroupName And ClientID={CurrentClientID}", new { GroupName = groupName });
                        if (itemGroup is null)
                        {
                            itemGroup = new()
                            {
                                GroupName = groupName,
                                ClientID = CurrentClientID
                            };
                            itemGroup.GroupID = await _dbContext.SaveAsync(itemGroup);
                        }
                    }

                    //checking code is not empty and checking already saved in the item group data table
                    if (!string.IsNullOrEmpty(itemCode))
                    {
                        var temp = await _dbContext.GetByQueryAsync<ItemGroupData>($"Select * From ItemGroupData Where ItemCode=@ItemCode And GroupName=@GroupName", new { ItemCode = itemCode, GroupName = groupName });
                        if (temp is null)
                        {
                            await _dbContext.SaveAsync(new ItemGroupData()
                            {
                                ItemCode = itemCode,
                                GroupName = groupName
                            });
                        }
                    }
                }
            }
            return Ok(new Success());
        }

        #endregion

        #region Item Variant

        [HttpPost("save-item-model")]
        public async Task<IActionResult> SaveItemModel(ItemVariantModel itemModelView)
        {

            int count = await _dbContext.GetByQueryAsync<int>($@"
                        SELECT Count(ItemVariantID) 
                        FROM ItemVariant
                        WHERE ItemVariantID<>@ItemVariantID and ItemID=@ItemID and PackingTypeID=@PackingTypeID and UMUnit=@UMUnit and Price=@Price and Cost=@Cost and SizeID=@SizeID and ColorID=@ColorID and UrlCode=@UrlCode and IsDeleted=0", itemModelView);

            if (count > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "ItemVariantExist"
                });
            }

            if (itemModelView.ItemID is null)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "ProvideItemIdForItemModel"
                });
            }

            List<ItemVariantModel> itemModels = new() { itemModelView };
            await _item.SaveItemModels(itemModels, itemModelView.ItemID.Value, CurrentClientID);
            return Ok(new ItemVariantAddResultModel() { ItemVariantID = itemModelView.ItemVariantID });
        }

        [HttpGet("get-item-model/{itemModelID}")]
        public async Task<IActionResult> GetItemModel(int itemModelID)
        {
            var itemModelView = await _item.GetItemModel(itemModelID);
            itemModelView ??= new();
            return Ok(itemModelView);
        }

        [HttpGet("delete-item-model/{itemModelID}")]
        public async Task<IActionResult> DeleteItemModel(int itemModelID)
        {
            await _dbContext.ExecuteAsync($"UPDATE ItemVariant SET IsDeleted=1 WHERE ItemVariantID={itemModelID}");
            List<ItemVariantImage> itemModelImageList = await _dbContext.GetListByQueryAsync<ItemVariantImage>($"Select * From ItemVariantImage Where ItemVariantID={itemModelID} And IsDeleted=0", null);
            if (itemModelImageList.Count > 0)
            {
                foreach (var image in itemModelImageList)
                {
                    await _dbContext.ExecuteAsync($"Update Media Set IsDeleted=1 Where MediaID={image.MediaID}", null);
                    await _dbContext.ExecuteAsync($"Update ItemVariantImage Set IsDeleted=1 Where ImageID={image.ImageID}", null);
                    string fileName = await _dbContext.GetFieldsAsync<Media, string>("FileName", $"MediaID={image.MediaID}", null);
                    string contentRootPath = _env.ContentRootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    fileName = fileName.Substring(fileName.LastIndexOf('/') + 1);
                    string filePath = Path.Combine(contentRootPath, "wwwroot", "gallery", "ItemImage", fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
            return Ok(new Success());
        }

        [HttpGet("get-item-model-by-item/{itemID}")]
        public async Task<IActionResult> GetItemModelID(int itemID)
        {
            int itemModelID = await _dbContext.GetFieldsAsync<Item, int>("ItemVariantID", $"IsDeleted=0 and ClientID={CurrentClientID} and ItemID={itemID}", null);
            return Ok(itemModelID);
        }

        [HttpPost("save-item-model-image")]
        public async Task<IActionResult> SaveItemModelImage(ItemVariantImageModel itemModelImageModel)
        {
            var itemModelIamge = _mapper.Map<ItemVariantImage>(itemModelImageModel);
            itemModelIamge.ImageID = await _dbContext.SaveAsync(itemModelIamge);
            return Ok(itemModelIamge.ImageID);
        }

        [HttpGet("remove-item-model-image/{imageID}")]
        public async Task<IActionResult> RemveMediaById(int imageID)
        {
            IdnValuePair media = await _dbContext.GetByQueryAsync<IdnValuePair>($@"Select M.MediaID As ID,M.FileName As Value
                                                                                    From ItemVariantImage IMI
                                                                                    Join Media M ON M.MediaID=IMI.MediaID
                                                                                    Where IMI.ImageID={imageID}", null);

            await _dbContext.ExecuteAsync($"Update ItemVariantImage Set IsDeleted=1 Where ImageID={imageID}");
            await _dbContext.ExecuteAsync($"Update Media Set IsDeleted=1 Where MediaID={media.ID}");
            //not getting the complete actual path
            if (media.Value is not null)
            {
                string contentRootPath = _env.ContentRootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string fileName = media.Value.Substring(media.Value.LastIndexOf('/') + 1);
                string filePath = Path.Combine(contentRootPath, "wwwroot", "gallery", "ItemImage", fileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            return Ok(true);
        }

        #endregion

        #region Item Packing Type

        [HttpPost("save-packing-type")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Common")]
        public async Task<IActionResult> SavePackingType(ItemPackingTypeModel model)
        {
            try
            {
                var packingType = await _dbContext.GetByQueryAsync<int?>($@"select PackingTypeID 
                                                                            from ItemPackingType 
                                                                            where (PackingTypeName=@PackingTypeName  or PackingTypeCode=@PackingTypeCode) and PackingTypeID<>@PackingTypeID and ClientID={CurrentClientID} and IsDeleted=0 ", model);

                if (packingType != null)
                {
                    return BadRequest(new BaseErrorResponse()
                    {
                        ErrorCode = 0,
                        ResponseTitle = "Invalid submission",
                        ResponseMessage = "Packing name or code already exist"
                    });
                }

                var logSummaryID = await _dbContext.InsertAddEditLogSummary(model.PackingTypeID, "PackingType : " + model.PackingTypeName + " Added/Edited bu User :" + CurrentUserID);
                var newPackingType = _mapper.Map<ItemPackingType>(model);
                newPackingType.ClientID = CurrentClientID;

                newPackingType.PackingTypeID = await _dbContext.SaveAsync(newPackingType, logSummaryID: logSummaryID);
                return Ok(new PackingTypeAddResultModel() { PackingTypeID = newPackingType.PackingTypeID, PackingTypeName = newPackingType.PackingTypeName });
            }
            catch (PBException err)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Something went wrong",
                    ResponseMessage = err.Message
                });
            }
        }

        [HttpGet("delete-packing-type/{packingTypeID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Common")]
        public async Task<IActionResult> Delete(int packingTypeID)
        {
            int enquiryCount = await _dbContext.GetByQueryAsync<int>($@"SELECT Count(*) AS DataCount FROM EnquiryItem EI
                                                                        LEFT JOIN ItemVariant IM ON IM.ItemVariantID = EI.ItemVariantID
                                                                        WHERE IM.IsDeleted=0 and EI.IsDeleted=0 and PackingTypeID={packingTypeID}
                                                                        GROUP BY PackingTypeID", null);

            if (enquiryCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid operation",
                    ResponseMessage = "Enquiry exist  with the ItemPackingType you are trying to remove"
                });
            }

            int quotationCount = await _dbContext.GetByQueryAsync<int>($@"SELECT Count(*) AS DataCount FROM EnquiryItem EI
                                                                        LEFT JOIN ItemVariant IM ON IM.ItemVariantID = EI.ItemVariantID
                                                                        WHERE IM.IsDeleted=0 and EI.IsDeleted=0 and PackingTypeID={packingTypeID}
                                                                        GROUP BY PackingTypeID", null);

            if (quotationCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid operation",
                    ResponseMessage = "Quotation entry exist with the ItemPackingType you are trying to remove"
                });
            }

            var model = await _dbContext.GetAsync<ItemPackingType>(packingTypeID);

            var logSummaryID = await _dbContext.InsertAddEditLogSummary(model.PackingTypeID, "PackingType : " + model.PackingTypeName + " Deleted by User :" + CurrentUserID);

            await _dbContext.ExecuteAsync($"Update ItemPackingType Set IsDeleted=1 Where PackingTypeID={packingTypeID} and ClientID={CurrentClientID}");

            return Ok(true);
        }

        [HttpGet("get-packing-types-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Common")]
        public async Task<IActionResult> GetPackingTypeList()
        {
            var packingTypes = await _dbContext.GetListByQueryAsync<ItemPackingTypeModel>($@"Select ROW_NUMBER() OVER(ORDER BY (SELECT 1)) AS RowIndex,PackingTypeID,PackingTypeCode,PackingTypeName
                                                                                            From ItemPackingType
                                                                                            Where IsDeleted=0 and ClientID={CurrentClientID}", null);
            return Ok(packingTypes);
        }

        [HttpPost("get-item-packing-type-paged-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Common")]
        public async Task<IActionResult> GetStatusList(PagedListPostModelWithoutFilter model)
        {
            PagedListQueryModel query = model;

            query.Select = $@"Select PackingTypeID,PackingTypeCode,PackingTypeName,ClientID
								From ItemPackingType";

            query.WhereCondition = $"IsDeleted=0 AND (ClientID={CurrentClientID} OR ClientID IS NULL)";
            query.OrderByFieldName = model.OrderByFieldName;
            query.SearchString = model.SearchString;
            query.SearchLikeColumnNames = new() { "PackingTypeName", "PackingTypeCode" };
            var res = await _dbContext.GetPagedList<ItemPackingTypeModel>(query, null);
            return Ok(res);
        }

        [HttpGet("get-item-packing-type/{packingTypeID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Common")]
        public async Task<IActionResult> GetLead(int packingTypeID)
        {
            var res = await _dbContext.GetByQueryAsync<ItemPackingType>($@"Select PackingTypeID,PackingTypeName,PackingTypeCode
                                                                                From ItemPackingType
                                                                                Where PackingTypeID={packingTypeID} and IsDeleted=0", null);
            return Ok(res ?? new());
        }

        #endregion

        #region Item Size

        [HttpPost("save-item-size")]
        public async Task<IActionResult> SaveSize(ItemSizeModel model)
        {

            var itemSizeCount = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) 
                                                                    from ItemSize
                                                                    where LOWER(Size)=LOWER(@Size) and SizeID<>@SizeID and ClientID={CurrentClientID} and IsDeleted=0", model);
            if (itemSizeCount != 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Item size already exist",
                    ResponseMessage = "The size already exist,try different size"
                });
            }


            try
            {
                var newItemSize = _mapper.Map<ItemSize>(model);
                newItemSize.ClientID = CurrentClientID;
                newItemSize.SizeID = await _dbContext.SaveAsync(newItemSize);
                return Ok(new ItemSizeAddResultModel() { SizeID = newItemSize.SizeID, Size = newItemSize.Size });


            }
            catch (Exception err)
            {

                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Something went wrong",
                    ResponseMessage = err.Message
                });
            }
        }

        [HttpPost("get-item-size-paged-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetItemSizeList(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;

            query.Select = $@"Select SizeID,Size
								From ItemSize";

            query.WhereCondition = $" ClientID={CurrentClientID}and IsDeleted=0 ";
            query.OrderByFieldName = model.OrderByFieldName;
            query.SearchString = model.SearchString;
            query.SearchLikeColumnNames = new() { "Size" };
            var res = await _dbContext.GetPagedList<ItemSizeModel>(query, null);
            return Ok(res);
        }

        [HttpGet("get-item-size/{SizeId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetItemSize(int SizeId)
        {
            var res = await _dbContext.GetByQueryAsync<ItemSize>($@"Select SizeID,Size
                                                                                From ItemSize
                                                                                Where SizeID={SizeId} and IsDeleted=0", null);
            return Ok(res ?? new());
        }

        [HttpGet("delete-item-size")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteItemSize(int Id)
        {
            int enquiryCount = await _dbContext.GetByQueryAsync<int>($@"SELECT Count(*) AS DataCount FROM EnquiryItem EI
                                                                        LEFT JOIN ItemVariant IM ON IM.ItemVariantID = EI.ItemVariantID
                                                                        WHERE IM.IsDeleted=0 and EI.IsDeleted=0 and SizeID={Id}
                                                                        GROUP BY SizeID", null);

            if (enquiryCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid operation",
                    ResponseMessage = "Enquiry exist  with the ItemSize you are trying to remove"
                });
            }

            int quotationCount = await _dbContext.GetByQueryAsync<int>($@"SELECT Count(*) AS DataCount FROM EnquiryItem EI
                                                                        LEFT JOIN ItemVariant IM ON IM.ItemVariantID = EI.ItemVariantID
                                                                        WHERE IM.IsDeleted=0 and EI.IsDeleted=0 and SizeID={Id}
                                                                        GROUP BY SizeID", null);

            if (quotationCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid operation",
                    ResponseMessage = "Quotation entry exist with the ItemSize you are trying to remove"
                });
            }
            await _dbContext.DeleteAsync<ItemSize>(Id);
            return Ok(true);
        }

        #endregion

        #region Item Color

        [HttpPost("save-item-color")]
        public async Task<IActionResult> SaveItemColor(ItemColorModel model)
        {
            var itemSizeCount = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) 
                                                                            from ItemColor
                                                                            where LOWER(ColorName)=LOWER(@ColorName) and ColorID<>@ColorID and (ClientID={CurrentClientID} or Isnull(ClientID,0)=0) and IsDeleted=0", model);
            if (itemSizeCount != 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Item color name already exist",
                    ResponseMessage = "The color name already exist,try different color name"
                });
            }

            itemSizeCount = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) 
                                                                            from ItemColor
                                                                            where ColorCode=@ColorCode and ColorID<>@ColorID and (ClientID={CurrentClientID} or Isnull(ClientID,0)=0) and IsDeleted=0", model);
            if (itemSizeCount != 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Item color code already exist",
                    ResponseMessage = "The color code already exist,try different color code"
                });
            }

            ItemColor itemColor = new()
            {
                ColorID = model.ColorID,
                ColorName = model.ColorName,
                ColorCode = model.ColorCode,
                ClientID = CurrentClientID
            };
            itemColor.ColorID = await _dbContext.SaveAsync(itemColor);
            ItemColorSuccessModel itemColorSuccessModel = new()
            {
                ColorID = itemColor.ColorID,
                ColorName = itemColor.ColorName,
                ResponseMessage = "ItemColorSaveSuccess"
            };
            return Ok(itemColorSuccessModel);
        }

        [HttpGet("delete-item-color/{colorID}")]
        public async Task<IActionResult> DeleteItemColor(int colorID)
        {
            int enquiryCount = await _dbContext.GetByQueryAsync<int>($@"Select Count(ColorID)
                                                            From EnquiryItem EI
                                                            Left Join ItemVariant IM ON IM.ItemVariantID=EI.ItemVariantID And IM.IsDeleted=0
                                                            Where IM.ColorID={colorID} And IM.IsDeleted=0
                                                            Group By ColorID", null);
            if (enquiryCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid operation",
                    ResponseMessage = "Enquiry exist  with the Item color you are trying to remove"
                });
            }
            int quotationCount = await _dbContext.GetByQueryAsync<int>($@"Select Count(ColorID)
                                                            From QuotationItem QI
                                                            Left Join ItemVariant IM ON IM.ItemVariantID=QI.ItemVariantID And IM.IsDeleted=0
                                                            Where IM.ColorID={colorID} And IM.IsDeleted=0 And QI.IsDeleted=0
                                                            Group By ColorID", null);
            if (quotationCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid operation",
                    ResponseMessage = "Quotation entry exist with the Item color you are trying to remove"
                });
            }
            int invoiceCount = await _dbContext.GetByQueryAsync<int>($@"Select Count(ColorID)
                                                            From InvoiceItem II
                                                            Left Join ItemVariant IM ON IM.ItemVariantID=II.ItemVariantID And IM.IsDeleted=0
                                                            Where IM.ColorID={colorID} And IM.IsDeleted=0 And II.IsDeleted=0
                                                            Group By ColorID", null);
            if (invoiceCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid operation",
                    ResponseMessage = "Invoice entry exist with the Item color you are trying to remove"
                });
            }
            await _dbContext.ExecuteAsync($"Update ItemColor Set IsDeleted=1 Where ColorID={colorID}", null);
            return Ok(true);
        }

        [HttpGet("get-item-color/{colorID}")]
        public async Task<IActionResult> GetItemColor(int colorID)
        {
            var itemColorModel = _mapper.Map<ItemColorModel>(await _dbContext.GetAsync<ItemColor>(colorID));
            return Ok(itemColorModel ?? new());
        }

        [HttpPost("get-item-color-paged-list")]
        public async Task<IActionResult> GetItemColorPagedList(PagedListPostModel pagedListPostModel)
        {
            PagedListQueryModel queryModel = pagedListPostModel;
            queryModel.Select = "Select ColorID, ColorName, ColorCode, ClientID From ItemColor";
            queryModel.WhereCondition = $"ClientID Is Null Or ClientID={CurrentClientID} And IsDeleted=0";
            queryModel.OrderByFieldName = pagedListPostModel.OrderByFieldName;
            queryModel.SearchLikeColumnNames = new() { "ColorName" };
            queryModel.SearchString = pagedListPostModel.SearchString;
            return Ok(await _dbContext.GetPagedList<ItemColorModel>(queryModel, null));
        }

        #endregion

        #region Item Brand

        [HttpPost("save-item-brand")]
        public async Task<IActionResult> SaveItemBrand(ItemBrandModel model)
        {
            var itemBrandCount = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) 
                                                                            from ItemBrand
                                                                            where LOWER(BrandName)=LOWER(@BrandName) and BrandID<>@BrandID and ClientID={CurrentClientID} and IsDeleted=0", model);
            if (itemBrandCount != 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "ItemBrandExist"
                });
            }

            ItemBrand itemBrand = _mapper.Map<ItemBrand>(model);
            itemBrand.ClientID = CurrentClientID;
            itemBrand.BrandID = await _dbContext.SaveAsync(itemBrand);
            ItemBrandSaveSuccessModel itemBrandSuccessModel = new()
            {
                BrandID = itemBrand.BrandID,
                BrandName = itemBrand.BrandName,
                ResponseMessage = "ItemBrandSaveSuccess"
            };
            return Ok(itemBrandSuccessModel);
        }

        [HttpGet("delete-item-brand/{brandID}")]
        public async Task<IActionResult> DeleteItemBrand(int brandID)
        {
            int enquiryCount = await _dbContext.GetByQueryAsync<int>($@"Select Count(BrandID)
                                                            From EnquiryItem EI
                                                            Left Join ItemVariant IM ON IM.ItemVariantID=EI.ItemVariantID And IM.IsDeleted=0
															Left Join Item I ON I.ItemID=IM.ItemID And I.IsDeleted=0
                                                            Where I.BrandID={brandID} And EI.IsDeleted=0
                                                            Group By BrandID", null);

            if (enquiryCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "ItemBrandInUse"
                });
            }

            int quotationCount = await _dbContext.GetByQueryAsync<int>($@"Select Count(ColorID)
                                                            From QuotationItem QI
                                                            Left Join ItemVariant IM ON IM.ItemVariantID=QI.ItemVariantID And IM.IsDeleted=0
                                                            Where IM.ColorID=1 And IM.IsDeleted=0 And QI.IsDeleted=0
                                                            Group By ColorID", null);

            if (quotationCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "ItemBrandInUse"
                });
            }
            await _dbContext.ExecuteAsync($"Update ItemBrand Set IsDeleted=1 Where BrandID={brandID}", null);
            int mediaID = await _dbContext.GetFieldsAsync<ItemBrand, int>("MediaID", $"BrandID=@BrandID", new { BrandID = brandID });
            await _media.DeleteExistingFileAsync(mediaID);
            return Ok(true);
        }

        [HttpGet("get-item-brand/{brandID}")]
        public async Task<IActionResult> GetItemBrand(int brandID)
        {
            var itemBrandModel = await _dbContext.GetByQueryAsync<ItemBrandModel>(@$"Select IB.*,M.FileName
                                                            From ItemBrand IB
                                                            Join Media M ON IB.MediaID=M.MediaID
                                                            Where IB.IsDeleted=0 And IB.BrandID={brandID}", null);
            return Ok(itemBrandModel ?? new());
        }

        [HttpPost("get-item-brand-paged-list")]
        public async Task<IActionResult> GetItemBrandPagedList(PagedListPostModel pagedListPostModel)
        {
            PagedListQueryModel queryModel = pagedListPostModel;
            queryModel.Select = @"Select BrandID, BrandName, IB.MediaID, FileName
                                                            From ItemBrand IB
                                                            Join Media M ON IB.MediaID=M.MediaID";
            queryModel.WhereCondition = $"ClientID={CurrentClientID}";
            queryModel.OrderByFieldName = pagedListPostModel.OrderByFieldName;
            queryModel.SearchLikeColumnNames = new() { "BrandName" };
            queryModel.SearchString = pagedListPostModel.SearchString;
            var itemBrandList = await _dbContext.GetPagedList<ItemBrandModel>(queryModel, null);
            itemBrandList = itemBrandList ?? new();
            itemBrandList.Data.ForEach(brand => brand.FileName = _config["ServerURL"] + brand.FileName);
            return Ok(itemBrandList);
        }

        #endregion

        #region Item Group

        [HttpPost("save-item-group")]
        public async Task<IActionResult> SaveItemGroup(ItemGroupModel model)
        {
            var itemGroupCount = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) 
                                                                            from ItemGroup
                                                                            where LOWER(GroupName)=LOWER(@GroupName) and GroupID<>@GroupID and ClientID={CurrentClientID} and IsDeleted=0", model);
            if (itemGroupCount != 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "ItemGroupExist"
                });
            }

            ItemGroup itemGroup = _mapper.Map<ItemGroup>(model);
            itemGroup.ClientID = CurrentClientID;
            itemGroup.GroupID = await _dbContext.SaveAsync(itemGroup);
            ItemGroupSaveSuccessModel itemGroupSuccessModel = new()
            {
                GroupID = itemGroup.GroupID,
                GroupName = itemGroup.GroupName
            };
            return Ok(itemGroupSuccessModel);
        }

        [HttpGet("delete-item-group/{groupID}")]
        public async Task<IActionResult> DeleteItemGroup(int categoryID)
        {
            int enquiryCount = await _dbContext.GetByQueryAsync<int>($@"Select Count(GroupID)
                                                            From EnquiryItem EI
                                                            Left Join ItemVariant IM ON IM.ItemVariantID=EI.ItemVariantID And IM.IsDeleted=0
															Left Join Item I ON I.ItemID=IM.ItemID And I.IsDeleted=0
                                                            Where I.GroupID={categoryID} And EI.IsDeleted=0
                                                            Group By GroupID", null);

            if (enquiryCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "ItemGroupInUse"
                });
            }

            int quotationCount = await _dbContext.GetByQueryAsync<int>($@"Select Count(GroupID)
                                                            From QuotationItem QI
                                                            Left Join ItemVariant IM ON IM.ItemVariantID=QI.ItemVariantID And IM.IsDeleted=0
                                                            Where IM.GroupID={categoryID} And IM.IsDeleted=0 And QI.IsDeleted=0
                                                            Group By ColorID", null);

            if (quotationCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "ItemGroupInUse"
                });
            }
            await _dbContext.ExecuteAsync($"Update ItemGroup Set IsDeleted=1 Where GroupID={categoryID}", null);
            return Ok(true);
        }

        [HttpGet("get-item-group/{categoryID}")]
        public async Task<IActionResult> GetItemGroup(int categoryID)
        {
            return Ok(await _dbContext.GetByQueryAsync<ItemGroupModel>($"Select * From ItemGroup Where GroupID={categoryID}", null));
        }

        [HttpPost("get-item-group-paged-list")]
        public async Task<IActionResult> GetItemGroupPagedList(PagedListPostModel pagedListPostModel)
        {
            PagedListQueryModel queryModel = new();
            queryModel.Select = @"Select GroupID, GroupName
                                            From ItemGroup";
            queryModel.WhereCondition = $"ClientID={CurrentClientID}";
            queryModel.OrderByFieldName = pagedListPostModel.OrderByFieldName;
            queryModel.SearchLikeColumnNames = new() { "GroupName" };
            queryModel.SearchString = pagedListPostModel.SearchString;
            var itemGroupList = await _dbContext.GetPagedList<ItemGroupModel>(queryModel, null);
            return Ok(itemGroupList ?? new());
        }

        #endregion

        #region Item Category

        [HttpPost("save-item-category")]
        public async Task<IActionResult> SaveItemCategory(ItemCategoryModel model)
        {
            var itemCategoryCount = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) 
                                                                            from ItemCategory
                                                                            where LOWER(CategoryName)=LOWER(@CategoryName) and CategoryID<>@CategoryID and ClientID={CurrentClientID} and IsDeleted=0", model);
            if (itemCategoryCount != 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "ItemCategoryExist"
                });
            }

            ItemCategory itemGroup = _mapper.Map<ItemCategory>(model);
            itemGroup.ClientID = CurrentClientID;
            itemGroup.CategoryID = await _dbContext.SaveAsync(itemGroup);
            ItemCategorySuccessModel itemCategorySuccessModel = new()
            {
                CategoryID = itemGroup.CategoryID,
                CategoryName = itemGroup.CategoryName
            };
            return Ok(itemCategorySuccessModel);
        }

        [HttpGet("delete-item-category")]
        public async Task<IActionResult> DeleteItemCategory(int categoryID)
        {
            int enquiryCount = await _dbContext.GetByQueryAsync<int>($@"Select Count(CategoryID)
                                                            From EnquiryItem EI
                                                            Left Join ItemVariant IM ON IM.ItemVariantID=EI.ItemVariantID And IM.IsDeleted=0
															Left Join Item I ON I.ItemID=IM.ItemID And I.IsDeleted=0
                                                            Where I.CategoryID={categoryID} And EI.IsDeleted=0
                                                            Group By CategoryID", null);

            if (enquiryCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "ItemCategoryInUse"
                });
            }

            int quotationCount = await _dbContext.GetByQueryAsync<int>($@"Select Count(CategoryID)
                                                            From QuotationItem QI
                                                            Left Join ItemVariant IM ON IM.ItemVariantID=QI.ItemVariantID And IM.IsDeleted=0
                                                            Where IM.CategoryID={categoryID} And IM.IsDeleted=0 And QI.IsDeleted=0
                                                            Group By CategoryID", null);

            if (quotationCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "ItemCategoryInUse"
                });
            }
            await _dbContext.ExecuteAsync($"Update ItemCategory Set IsDeleted=1 Where CategoryID={categoryID}", null);
            return Ok(true);
        }

        [HttpGet("get-item-category/{categoryID}")]
        public async Task<IActionResult> GetItemCategory(int categoryID)
        {
            return Ok(await _dbContext.GetByQueryAsync<ItemCategoryModel>($"Select * From ItemCategory Where CategoryID={categoryID}", null));
        }

        [HttpPost("get-item-category-paged-list")]
        public async Task<IActionResult> GetItemCategoryPagedList(PagedListPostModel pagedListPostModel)
        {
            PagedListQueryModel queryModel = new();
            queryModel.Select = @"Select C.CategoryID, C.CategoryName, C.ParentID, P.CategoryName As ParentCategoryName
                                            From ItemCategory C
                                            Left Join ItemCategory P ON P.CategoryID=C.ParentID";
            queryModel.WhereCondition = $"C.ClientID={CurrentClientID}";
            queryModel.OrderByFieldName = pagedListPostModel.OrderByFieldName;
            queryModel.SearchLikeColumnNames = new() { "C.CategoryName" };
            queryModel.SearchString = pagedListPostModel.SearchString;
            var itemCategoryList = await _dbContext.GetPagedList<ItemCategoryModel>(queryModel, null);
            return Ok(itemCategoryList ?? new());
        }

        #endregion

        #region Ordering App

        [HttpGet("get-cart-items")]
        public async Task<IActionResult> GetCartItems()
        {
            List<CartListItemModel>? cartItems = null;
            int enquiryID = await _dbContext.GetByQueryAsync<int>($@"
                                                        Select Top 1 EnquiryID
	                                                    From Enquiry 
	                                                    Where IsInCart=1 And IsDeleted=0 And UserEntityID={CurrentEntityID}
	                                                    Order By EnquiryID Desc", null);
            if (enquiryID > 0)
            {
                cartItems = await _dbContext.GetListByQueryAsync<CartListItemModel>($@"
                                                    Select EI.EnquiryItemID,EI.ItemVariantID,EI.EnquiryID,EI.Quantity,I.ItemName,I.ItemCode,I.Price,EI.Description,CR.Symbol,
                                                    (
                                                        SELECT TOP 1 M.FileName
                                                        FROM ItemVariantImage IMI
                                                        LEFT JOIN Media M ON M.MediaID=IMI.MediaID
                                                        WHERE EI.ItemVariantID=IMI.ItemVariantID
                                                        ORDER BY 1 ASC
                                                    ) AS FileName
                                                    From Enquiry E
                                                    LEFT JOIN EnquiryItem EI ON EI.EnquiryID=E.EnquiryID And EI.IsDeleted=0
                                                    LEFT JOIN viItem I ON EI.ItemVariantID=I.ItemVariantID
                                                    LEFT JOIN viBranch B ON B.BranchID=E.BranchID
                                                    LEFT JOIN EntityAddress A ON A.AddressID=B.AddressID
                                                    LEFT JOIN Country C ON C.CountryID=A.CountryID
                                                    LEFT JOIN Currency CR ON CR.CurrencyID=C.CurrencyID
                                                    Where E.EnquiryID={enquiryID}", null);

                foreach (var cartItem in cartItems)
                {
                    if (cartItem.Description is not null)
                    {
                        int startIndex = cartItem.Description.IndexOf('{');
                        int endIndex = cartItem.Description.IndexOf('}');
                        if (startIndex != -1 && endIndex != -1 && startIndex < endIndex)
                        {
                            string digits = cartItem.Description.Substring(startIndex + 1, endIndex - startIndex - 1);
                            if (AreAllDigits(digits))
                                cartItem.Price = Convert.ToDecimal(digits);
                        }
                    }
                }
            }
            return Ok(cartItems ?? new());
        }

        [HttpGet("get-items-by-item-code/{groupName}")]
        public async Task<IActionResult> GetItemsByItemCode(string groupName)
        {
            if(!string.IsNullOrEmpty(groupName))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseMessage = "Please provide group name",
                    ResponseTitle = "Error"
                });
            }

            int itemGroupID = await _dbContext.GetByQueryAsync<int>($"Select GroupID From ItemGroup Where GroupName=@GroupName And IsDeleted=0", new { GroupName = groupName });
            var cartItemDetails = await _dbContext.GetListByQueryAsync<ItemDetailsForOrderAppModel>(@$"Select I.ItemVariantID,I.ItemName,I.ItemCode,I.Price As MRP,I.Price As SellingPrice,CR.Symbol
                                                                                                        From viItem I
                                                                                                        LEFT JOIN viBranch B ON B.BranchID={CurrentBranchID}
                                                                                                        LEFT JOIN EntityAddress A ON A.AddressID=B.AddressID
                                                                                                        LEFT JOIN Country C ON C.CountryID=A.CountryID
                                                                                                        LEFT JOIN Currency CR ON CR.CurrencyID=C.CurrencyID
                                                                                                        Where I.GroupID=@ItemGroupID And I.ClientID={CurrentClientID}", new { ItemGroupID = itemGroupID });
            foreach (var cartItem in cartItemDetails)
            {
                var image = await _item.GetItemModelImage(cartItem.ItemVariantID);
                if (image is not null)
                {
                    cartItem.Images = new();
                    cartItem.Images = new() { image.FileName };
                }
            }
            return Ok(cartItemDetails ?? new());
        }

        [HttpGet("get-item-details-by-item-model-id/{itemModelID}")]
        public async Task<IActionResult> GetItemDetailsByItemCode(int itemModelID)
        {
            var cartItemDetails = await _dbContext.GetByQueryAsync<ItemDetailsForOrderAppModel>(@$"Select I.ItemVariantID,I.ItemName,I.ItemCode,I.Price As MRP,I.Price As SellingPrice,CR.Symbol
                                                                                                        From viItem I
                                                                                                        LEFT JOIN viBranch B ON B.BranchID={CurrentBranchID}
                                                                                                        LEFT JOIN EntityAddress A ON A.AddressID=B.AddressID
                                                                                                        LEFT JOIN Country C ON C.CountryID=A.CountryID
                                                                                                        LEFT JOIN Currency CR ON CR.CurrencyID=C.CurrencyID
                                                                                                        Where I.ItemVariantID=@ItemVariantID And I.ClientID={CurrentClientID}", new { ItemVariantID = itemModelID });
            if (cartItemDetails is not null)
            {
                var itemModelImages = await _item.GetItemModelImages(cartItemDetails.ItemVariantID);
                cartItemDetails.Images = new();
                if (itemModelImages is not null && itemModelImages.Count > 0)
                {
                    cartItemDetails.Images
                        .AddRange(
                            itemModelImages.Select(img => img.FileName)
                        .ToList());
                }
            }
            return Ok(cartItemDetails ?? new());
        }

        [HttpPost("add-to-cart")]
        public async Task<IActionResult> AddToCart(AddToCartModel cartModel)
        {
            Enquiry cart = _mapper.Map<Enquiry>(cartModel);
            if (!cart.IsInCart)
            {
                int entityID = await _dbContext.GetByQueryAsync<int>($"Select EntityID From Entity Where Phone=@Phone And ClientID=@ClientID", new { Phone = cartModel.Phone, ClientID = CurrentClientID });
                if (entityID == 0)
                {
                    Entity entity = new()
                    {
                        Phone = cartModel.Phone,
                        EmailAddress = cartModel.Email,
                        EntityTypeID = (int)EntityType.Customer,
                        ClientID = CurrentClientID
                    };
                    entityID = await _dbContext.SaveAsync(entity);
                    EntityPersonalInfo entityPersonalInfo = new()
                    {
                        FirstName = cartModel.CustomerName,
                        EntityID = entityID
                    };
                    entityPersonalInfo.EntityPersonalInfoID = await _dbContext.SaveAsync(entityPersonalInfo);
                }
                cart.CustomerEntityID = entityID;
            }
            cart.UserEntityID = CurrentEntityID;
            cart.BranchID = CurrentBranchID;

            if (cart.EnquiryID == 0)
            {
                int? enquiryNumber = await _dbContext.GetByQueryAsync<int?>($"Select Max(ISNULL(EnquiryNo,0))+1 As EnquiryNo From Enquiry Where  BranchID={CurrentBranchID} And IsDeleted=0 And IsInCart=0", null);
                cart.EnquiryNo = enquiryNumber is not null ? enquiryNumber.Value : 1;
            }
            else
            {
                cart.EnquiryNo = await _dbContext.GetFieldsAsync<Enquiry, int>("EnquiryNo", $"EnquiryID={cart.EnquiryID}", null);
            }
            cart.EnquiryID = await _dbContext.SaveAsync(cart);

            var cartItems = _mapper.Map<List<EnquiryItem>>(cartModel.CartItems);
            if (cartItems.Count > 0)
                await _dbContext.SaveSubItemListAsync(cartItems, "EnquiryID", cart.EnquiryID, null, false);

            #region Notification

            if (!cart.IsInCart)
            {
                string msg = "";
                string title = "";
                string notificationType = "";
                notificationType = "Order";
                title = $"Order created";
                msg = $"A new order(#ORD-{cart.EnquiryNo}) created successfully with {cartModel.CartItems.Count} items";
                await _notification.SendNotificationWithPush(CurrentEntityID, msg, title, notificationType, 2);
            }

            #endregion

            return Ok(new EnquiryAddResultModel()
            {
                EnquiryID = cart.EnquiryID,
                ResponseMessage = cart.IsInCart ? "added to cart successfully" : "Order created successfully"
            });
        }

        [HttpGet("get-cart-item/{enquiryItemID}")]
        public async Task<IActionResult> GetEnquiryItem(int enquiryItemID)
        {
            var cartItemDetails = await _dbContext.GetByQueryAsync<ItemDetailsForOrderAppModel>(@$"Select I.ItemVariantID,I.ItemName,I.ItemCode,EI.Quantity,EI.Description,I.Price As MRP,I.Price As SellingPrice,CR.Symbol
                                                                                                        From EnquiryItem EI
                                                                                                        LEFT JOIN viItem I ON I.ItemVariantID=EI.ItemVariantID
                                                                                                        LEFT JOIN viBranch B ON B.BranchID={CurrentBranchID}
                                                                                                        LEFT JOIN EntityAddress A ON A.AddressID=B.AddressID
                                                                                                        LEFT JOIN Country C ON C.CountryID=A.CountryID
                                                                                                        LEFT JOIN Currency CR ON CR.CurrencyID=C.CurrencyID
                                                                                                        Where EI.EnquiryItemID={enquiryItemID}", null);
            if (cartItemDetails is not null)
            {
                var itemModelImages = await _item.GetItemModelImages(cartItemDetails.ItemVariantID);
                cartItemDetails.Images = new();
                if (itemModelImages is not null && itemModelImages.Count > 0)
                {
                    cartItemDetails.Images
                        .AddRange(
                            itemModelImages.Select(img => img.FileName)
                        .ToList());
                }

                if (cartItemDetails.Description is not null)
                {
                    int startIndex = cartItemDetails.Description.IndexOf('{');
                    int endIndex = cartItemDetails.Description.IndexOf('}');
                    if (startIndex != -1 && endIndex != -1 && startIndex < endIndex)
                    {
                        string digits = cartItemDetails.Description.Substring(startIndex + 1, endIndex - startIndex - 1);
                        if (AreAllDigits(digits))
                            cartItemDetails.SellingPrice = Convert.ToDecimal(digits);
                    }
                }
            }
            return Ok(cartItemDetails ?? new());
        }

        [HttpPost("remove-from-cart/{enquiryItemID}")]
        public async Task<IActionResult> RemoveFromCart(int enquiryItemID)
        {
            int enquiryID = await _dbContext.GetFieldsAsync<EnquiryItem, int>("EnquiryID", $"EnquiryItemID={enquiryItemID}", null);
            await _dbContext.ExecuteAsync($"Update EnquiryItem Set IsDeleted=1 Where EnquiryItemID={enquiryItemID}", null);
            int cartItemCount = await _dbContext.GetByQueryAsync<int>($"Select Count(*) From EnquiryItem Where EnquiryID={enquiryID} And IsDeleted=0", null);
            if (cartItemCount == 0)
                await _dbContext.ExecuteAsync($"Update Enquiry Set IsDeleted=1 Where EnquiryID={enquiryID}");
            return Ok(true);
        }

        [HttpGet("delete-all-carts")]
        public async Task<IActionResult> DeleteAllCarts()
        {
            await _dbContext.ExecuteAsync($"Update Enquiry Set IsDeleted=1 Where UserEntityID={CurrentEntityID} And IsInCart=1");
            return Ok(true);
        }

        //Digit checking
        static bool AreAllDigits(string str)
        {
            foreach (char c in str)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }

        #endregion

    }
}
