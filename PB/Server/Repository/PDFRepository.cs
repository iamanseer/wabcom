
using DinkToPdf;
using DinkToPdf.Contracts;
using PB.Shared.Models;
using PB.DatabaseFramework;
using PB.Model;
using PB.Shared.Enum.Common;
using PB.Shared.Tables;
using System.Data;
using System.Text;
using PB.Shared.Tables.CRM;
using PB.EntityFramework;
using PB.Shared.Models.Inventory.Items;
using NPOI.HPSF;
using PB.Client.Pages.CRM;
using PB.Shared.Tables.Inventory.Invoices;
using PB.Model.Models;
using Bee.NumberToWords.Enums;
using Bee.NumberToWords;
using static Org.BouncyCastle.Math.EC.ECCurve;
using PB.Shared.Models.Inventory.Invoices;
using PB.Shared.Helpers;
using PB.Shared.Tables.Tax;
using Microsoft.IdentityModel.Tokens;

namespace PB.Server.Repository
{
    public interface IPDFRepository
    {
        #region Quotation Pdf

        Task<int> CreateQuotationPdf(int quotationID, int branchID, string fileName, IDbTransaction? tran = null);
        Task<string> GetQuotationPdfHtmlContent(int quotationID, int branchID, IDbTransaction? tran = null);
        Task<QuotationPdfDetailsModel?> GetQuotationPdfDetailsModel(int quotationID, int branchID, IDbTransaction? tran = null);

        #endregion

        #region Invoice Pdf

        Task<int> CreateInvoicePdf(int invoiceID, int branchID, int clientID, string fileName, IDbTransaction? tran = null);
        Task<string> GetInvoicePdfHtmlContent(int invoiceID, int clientID, IDbTransaction? tran = null);
        Task<InvoicePdfModel> GetIvoicePdfDetails(int invoiceID, int clientID);


        #endregion

        #region Item Qr code Pdf

        Task<string> GetItemQrCodePdfFile(int clientID, IDbTransaction? tran = null);
        Task<ItemQrCodeResponseMode> GetItemQrPdfHtmlContentAndData(int clientID, PagedListPostModel searchModel);

        #endregion

        #region Item Qr code Pdf

        Task<string> GetItemGroupQrCodePdfFile(int clientID, IDbTransaction? tran = null);
        Task<ItemGroupQrCodeResponseMode> GetItemGroupQrPdfHtmlContentAndData(int clientID, PagedListPostModel searchModel);

        #endregion

        #region Client Invoice Pdf

        Task<int> CreateClientInvoicePdf(int invoiceID, string fileName, IDbTransaction? tran = null);

        #endregion


        #region New Quotation Pdf
        Task<int> GenerateBasicQuotationPdf(int quotationID, int branchID, string fileName, IDbTransaction? tran = null);
        Task<string> GenerateAllQuotationContent(int quotationID, int branchID, IDbTransaction? tran = null);
        #endregion

    }

    public class PDFRepository : IPDFRepository
    {
        private readonly IDbContext _dbContext;
        private readonly IConverter _converter;
        private readonly IHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly ISuperAdminRepository _superAdmin;
        private readonly ICustomerRepository _customer;

        public PDFRepository(IDbContext dbContext, IConverter converter, IHostEnvironment env, IConfiguration config, ISuperAdminRepository superAdmin, ICustomerRepository customer)
        {
            this._dbContext = dbContext;
            this._converter = converter;
            this._env = env;
            this._config = config;
            this._superAdmin = superAdmin;
            this._customer = customer;
        }

        #region Quotaion Pdf

        public async Task<int> CreateQuotationPdf(int quotationID, int branchID, string fileName, IDbTransaction? tran = null)
        {
            string htmlContent = "";
            int? mediaID = null;

            string folderName = "gallery/" + PDFType.Quotation.ToString().ToLower();
            if (!Directory.Exists(Path.Combine("wwwroot", folderName)))
            {
                Directory.CreateDirectory(Path.Combine("wwwroot", folderName));
            }
            string filePath = Path.Combine(_env.ContentRootPath, "wwwroot", folderName, $"{fileName}.pdf");
            string mediaFileName = $"/{folderName}/{fileName}.pdf";
            string cssfilePath = Path.Combine(_env.ContentRootPath, "wwwroot", "assets");
            cssfilePath += "/style.css";

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = DinkToPdf.PaperKind.A4Small,
                // Margins = new MarginSettings { Top = 10 },
                DocumentTitle = "Quotation Pdf Report",
                Out = filePath,
            };

            htmlContent = await GetQuotationPdfHtmlContent(quotationID, branchID, tran);
            mediaID = await _dbContext.GetFieldsAsync<Quotation, int?>("MediaID", $"BranchID={branchID} and QuotationID={quotationID} and IsDeleted=0", null, tran);
            mediaID ??= 0;
            var objectSettings = new ObjectSettings
            {
                // PagesCount = true,
                HtmlContent = htmlContent,
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = cssfilePath },
                //  HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                // FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "Report Footer" }
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
            _converter.Convert(pdf);

            await Task.Delay(100);

            Media media = new Media
            {
                MediaID = mediaID,
                ContentType = "application/pdf",
                Extension = "pdf",
                FileName = mediaFileName
            };

            mediaID = await _dbContext.SaveAsync(media, tran);

            await _dbContext.ExecuteAsync($"Update Quotation set MediaID={mediaID} where QuotationID={quotationID} and BranchID={branchID} and IsDeleted=0", null, tran);

            return mediaID.Value;
        }
        public async Task<string> GetQuotationPdfHtmlContent(int quotationID, int branchID, IDbTransaction? tran = null)
        {
            try
            {
                string dateFormat = "dd/MM/yyyy";
                var quotation = await GetQuotationPdfDetailsModel(quotationID, branchID, tran);

                decimal totalAmount = quotation.Items.Sum(i => i.TotalAmount);
                //await ConvertNumberToWords(totalAmount);

                var BillingAddress = new StringBuilder();


                BillingAddress.Append(@$"
                        {quotation.CustomerName},<br>
                ");

                BillingAddress.Append($@"
                        {quotation.BillingAddressLine1},<br>
                ");

                if (quotation.BillingAddressLine2 != null)
                {
                    BillingAddress.Append($@"
                        {quotation.BillingAddressLine2},<br>
                    ");
                }

                if (quotation.BillingAddressLine3 != null)
                {
                    BillingAddress.Append($@"
                        {quotation.BillingAddressLine3},<br>
                     ");
                }

                if (quotation.BillingState != null)
                {
                    BillingAddress.Append($@"
                        {quotation.BillingState},<br>
                    ");
                }

                if (quotation.BillingPinCode != null)
                {
                    BillingAddress.Append($@"
                        {quotation.BillingPinCode},<br>
                    ");
                }

                if (quotation.BillingCountry != null)
                {
                    BillingAddress.Append($@"
                        {quotation.BillingCountry}<br>
                    ");
                }

                BillingAddress.Append($@"
                    Phone :  {quotation.Phone},<br>
                ");

                if (!string.IsNullOrEmpty(quotation.EmailAddress))
                {
                    BillingAddress.Append(@$"
                        Email : {quotation.EmailAddress},<br>
                    ");
                }

                var ShippingAddress = new StringBuilder();

                ShippingAddress.Append(@$"
                        {quotation.CustomerName},<br>
                ");

                ShippingAddress.Append($@"
                    {quotation.ShippingAddressLine1},<br>
                ");

                if (quotation.ShippingAddressLine2 != null)
                {
                    ShippingAddress.Append($@"
                        {quotation.ShippingAddressLine2},<br>
                    ");
                }

                if (quotation.ShippingAddressLine3 != null)
                {
                    ShippingAddress.Append($@"
                        {quotation.ShippingAddressLine3},<br>
                     ");
                }
                if (quotation.ShippingState != null)
                {
                    ShippingAddress.Append($@"
                        {quotation.ShippingState},<br>
                    ");
                }
                if (quotation.ShippingPinCode != null)
                {
                    ShippingAddress.Append($@"
                        {quotation.ShippingPinCode},<br>
                ");
                }
                if (quotation.ShippingCountry != null)
                {
                    ShippingAddress.Append($@"
                        {quotation.ShippingCountry}<br>
                    ");
                }


                ShippingAddress.Append($@"
                    Phone :  {quotation.Phone},<br>
                ");

                if (!string.IsNullOrEmpty(quotation.EmailAddress))
                {
                    ShippingAddress.Append(@$"
                        Email : {quotation.EmailAddress},<br>
                    ");
                }

                var ClientAddress = new StringBuilder();

                ClientAddress.Append($@"
                    {quotation.ClientAddressLine1},<br>
                ");

                if (quotation.ClientAddressLine2 != null)
                {
                    ClientAddress.Append($@"
                        {quotation.ClientAddressLine2},<br>
                    ");
                }

                if (quotation.ClientAddressLine3 != null)
                {
                    ClientAddress.Append($@"
                        {quotation.ClientAddressLine3},<br>
                     ");
                }
                if (quotation.ClientState != null)
                {
                    ClientAddress.Append($@"
                        {quotation.ClientState},<br>
                    ");
                }
                if (quotation.ClientPinCode != null)
                {
                    ClientAddress.Append($@"
                        {quotation.ClientPinCode},<br>
                ");
                }
                if (quotation.ClientCountry != null)
                {
                    ClientAddress.Append($@"
                        {quotation.ClientCountry}<br>
                    ");
                }

                var sb = new StringBuilder();

                sb.Append($@"<!DOCTYPE html>
                                <html lang='en'>
                                <head>
                                    <meta charset='UTF-8'>
                                    <meta http-equiv='X-UA-Compatible' content='IE=edge'>
                                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                                    <title>Quotation</title>
                 ");

                sb.Append($@"
                                    <style>
                                        .invoice {{
                                            width: 100%;
                                            margin: 0px;
                                            font-family: Arial, sans-serif;
                                            font-size: 12px
                                          }}
                                            @media print {{
                                                @page {{
                                                size: A4;
                                                margin: 0
                                            }}
                                            body {{
                                                margin: 1cm
                                            }}
                                            }}
                                          .header table{{ 
                                                    border-left: 1px solid #000000;
			                                        border-top: 1px solid #000000;
			                                        border-right: 1px solid #000000;
			                                        border-collapse: collapse
                                          }}
                                        .main-table table thead td{{ 
                                                    padding: 10px
                                        }}
		                                .main-table table ,.main-table th ,.main-table td{{
                                            border-left: 1px solid #000000;
			                                border-bottom: 1px solid #000000;
			                                border-right: 1px solid #000000;
			                                border-collapse: collapse
		                                }}
		                                .main-table td{{ 
                                            text-align: center
		                                }}
                                        .main-table th{{ 
                                            background-color: #F2F3F4
                                        }}
		                                .table-1 th{{ 
                                            text-align: left
		                                }}
		                                .table-1 table{{ 
                                            border-left: 1px solid #000000;
			                                border-top: 1px solid #000000;
			                                border-right: 1px solid #000000;
			                                border-collapse: collapse;
			                                float: left
		                                }}
		                                .table-1 td{{ 
                                            padding-left: 10px
		                                }}

                                        .table-1-1 table{{ 
                                            border-top: 1px solid #000000;
			                                border-right: 1px solid #000000;
			                                border-collapse: collapse
		                              }}
                                        .table-1-1 th{{ 
                                            text-align: left
		                               }}
		                                .table-1-1 td{{ 
                                            padding-left: 10px
		                               }}

                                        .table-2 table {{ 
                                                float: left

                                       }}

		                                .table-2 table , .table-2 th , .table-2 td{{
                                                border: 1px solid #000000;
			                                    border-collapse: collapse
		                                }}
		                                 .table-2 th{{ 
                                                text - align: left;
	                                            padding-left: 10px
		                                }}
		                                    .table-2 td , .table-2-1 td{{
                                                padding-left: 10px
		                                }}
                                            .table-2-1 table  {{
                                                  float: left;
                                                  border-bottom: 1px solid #000000;
                                                  border-right : 1px solid #000000;
                                                  border-collapse: collapse
                                        }}
                                            .table-2-1 th {{ 
                                                text-align: left;
                                                border-top: 1px solid #000000;
                                                border-bottom: 1px solid #000000;
	                                            padding-left: 10px
                                        }}
		                                    .table-3 table{{ 
                                                float: left;
			                                    border-left : 1px solid #000000;
			                                    border-collapse: collapse
		                                }}
                                            .table-3 td {{ 
                                                padding: 10px
                                        }}
		                                    .table-4 table , .table-5 table{{
                                                border - left: 1px solid #000000;
			                                    border-bottom: 1px solid #000000;
			                                    border-right: 1px solid #000000;
			                                    border-collapse: collapse
		                                }}
		                                    .table-4 th , .table-4 td {{ 
                                                text-align: right;
			                                    padding-right: 10px
		                                }}
		                                    .table-5 table{{ 
                                                float: right
		                                }}
		                                    .table-5 td{{ 
                                                text-align: center;
                                                padding-top: 70px
		                                }}
		                                    .table-bottom{{ 
                                                float: left;
			                                    border-left: 1px solid #000000;
			                                    border-bottom: 1px solid #000000;
			                                    border-right: 1px solid #000000;
			                                    border-collapse: collapse
		                                }}
                                    </style>
                                </head>
                 ");

                sb.Append($@"
                                    <body>
                                        <div class='invoice'>
                                            <div class='header'>
                                                <table style='width: 100%;'>
                                                    <tr>
                                                        <td style='width: 23%; padding-left: 10px;'><img style='width: 160px; height: 160px;' src='{quotation.DomainUrl}/{quotation.ClientImage}' alt=''> </td>
                                                        <td style='text-align: start; padding-left: 10px; width: 53%;'><h3>{quotation.ClientName}</h3><br> {ClientAddress} <br> {quotation.GSTNo}</td>
                                                        <td style='text-align: end; padding-top: 130px; padding-right: 10px;  width: 24%;'><h1>QUOTATION</h1></td>

                                                    </tr>
                                                </table>

                                            </div>
                                            <div class='table-1'>
                                                <table style='width: 50%; height: 100px;'>
                                                    <tr>
                                                        <td>Quotation No</td>
                                                        <th>: #QT-{quotation.QuotationNo}</th>
                                                    </tr>
                                                    <tr>
                                                        <td>Quotation Date </td>
                                                        <th>: {(quotation.Date.HasValue ? quotation.Date.Value.ToString(dateFormat) : string.Empty)}</th>
                                                    </tr>
                                                    <tr>
                                                        <td>Terms </td>
                                                        <th>: --</th>
                                                    </tr>
                                                    <tr>
                                                        <td>Expiry Date</td>
                                                        <th>: {(quotation.ExpiryDate.HasValue ? quotation.ExpiryDate.Value.ToString(dateFormat) : string.Empty)}</th>
                                                    </tr>

                                                  </table>
                                             </div>
                                             <div class='table-1-1'>
                                                <table style='width:50%; height: 100px; '>
                                                    <tr>
                                                        <td></td>
                                                        <th></th>
                                                    </tr>

                                                  </table>
                                             </div>

                                             <div class='table-2' >
                                                <table style='width:50%; '>
                                                   <tr>

                                                        <th style='height: 25px; background-color:#F2F3F4;text-align:left;'>Bill To</th>


                                                    </tr>
                                                    <tr style='height:130px;'>
                                                        <td style='height: 35px;'> <b>{BillingAddress}</td>

                                                    </tr>

                                                </table>
                                             </div>");

                if (quotation.ShippingAddressID != null)
                {
                    sb.Append(@$"<div class='table-2-1' >
                                        <table style='width: 50%; '>
                                            <tr>


                                                <th style='height: 25px; background-color:#F2F3F4;'>Ship To</th>

                                            </tr>
                                            <tr style='height:130px;'>

                                                <td style='height: 35px;'>{ShippingAddress}</td>
                                            </tr>


                                        </table>
                                     </div>");
                }
                else
                {
                    sb.Append($@"<div class='table-2-1' >
                                        <table style='width: 50%; '>
                                            <tr>


                                                <th style='height: 25px; background-color:#F2F3F4;'>Ship To</th>

                                            </tr>
                                            <tr style='height:130px;'>

                                                <td style='height: 35px;'></td>
                                            </tr>


                                        </table>
                                     </div>

                    ");
                }

                sb.Append(@$"
                                            <div class='main-table'>
                                                <table style='width: 100%;'>
                                                    <thead >
                                                        <tr>
                                                            <td colspan='9' style='text-align: left;'>Subject : <br> <br> {quotation.Subject}</td>
                                                        </tr>
                                                      <tr >
                                                        <th rowspan='2' style='padding-left: 10px; text-align: left;'>No.</th>
                                                        <th rowspan='2'>Item & Description</th>
                                                        <th rowspan='2'> Qty</th>
                                                        <th rowspan='2'> Rate </th>
                ");

                sb.Append(@$"
                                            
                                                        <th colspan='2'>CGST</th>
                                                        <th colspan='2'>SGST</th >
                ");

                sb.Append(@$"
                                            
                                                        <th rowspan='2'> Amount</th>
                                                      </tr>
                                                      <tr>

                                                        <th>%</th>
                                                        <th>Amt</th>
                                                        <th>%</th>
                                                        <th>Amt</th>


                                                      </tr>
                                                    </thead>
                                                    <tbody>
                ");

                foreach (var item in quotation.Items)
                {
                    sb.Append($@"
                                                        <tr>
                                                            <td style='padding-left: 10px; text-align: left;'>{item.RowIndex}</td>
                                                            <td style='color:#212020; text-align: left; padding: 10px;'>{item.ItemName} <br> {item.Description}</td>
                                                            <td>{item.Quantity}</td>
                                                            <td>{item.Rate}</td>
                    ");

                    switch (item.TaxCategoryItems.Count)
                    {
                        case 0:

                            sb.Append($@"
                                                            <td>--</td>
                                                            <td>--</td>
                                                            <td>--</td>
                                                            <td>--</td>
                     ");

                            break;

                        case 1:

                            sb.Append($@"
                                                            <td>{item.TaxCategoryItems[0].Percentage}</td>
                                                            <td>{item.TaxCategoryItems[0].Amount}</td>
                                                            <td>--</td>
                                                            <td>--</td>
                ");

                            break;

                        case 2:

                            sb.Append($@"
                                                            <td>{item.TaxCategoryItems[0].Percentage}</td>
                                                            <td>{item.TaxCategoryItems[0].Amount}</td>
                                                            <td>{item.TaxCategoryItems[1].Percentage}</td>
                                                            <td>{item.TaxCategoryItems[1].Amount}</td>
                ");

                            break;
                    }


                    sb.Append($@"
                                                            <td>{item.TotalAmount}</td>
                                                        </tr>
                    ");
                }

                sb.Append($@"
                                                    </tbody>
                                                  </table>
                                            </div>
                                            <div class='table-3'>

                                               <table style='width: 60%;height:250px;'>
                                            <td >Total In Words <br> <b> {quotation.CurrencyName + " " + quotation.AmountInWords} </b>
                                            </td>
                                        </tr>
                
                                        <tr>
                                            <td> <span style='color: gray;'>Customer Notes :</span> <br> {quotation.CustomerNote}</td>
                                        </tr>
                                        <tr>
                                            <td> <span style='color: gray;'>Terms & Conditions</span>  <br> {quotation.TermsandCondition}</td>
                                        </tr>

                                        </table>

                                            </div>
                                            <div class='table-4'>
                                                <table style='width: 40%; height: 120px;border-left: 1px solid #000;'>
                                                        <tr>
                                                            <td style='padding-top:10px;'>Sub Total</td>
                                                            <td style='padding-top:10px;'>{quotation.Items.Sum(item => item.TotalAmount)} </td>
                                                        </tr>
                                                        <tr>
                                                            <td style='padding-top:10px;'>Discount</td>
                                                            <td style='padding-top:10px;'>{quotation.Items.Sum(i => i.Discount)} </td>
                                                        </tr>
                                                        <tr>
                                                            <th>Net Amount</th>
                                                            <th> {quotation.CurrencySymbol} {(quotation.Items.Sum(i => i.NetAmount))} </th>
                                                        </tr>
                ");

                var allTaxCategoryItems = quotation.Items.SelectMany(item => item.TaxCategoryItems).ToList();
                var groupedTaxCategoryItems = allTaxCategoryItems
                .GroupBy(item => item.TaxCategoryItemID)
                .Select(group => new QuotationItemTaxCategoryItemsModel
                {
                    TaxCategoryItemID = group.Key,
                    TaxCategoryItemName = group.First().TaxCategoryItemName,
                    Percentage = group.First().Percentage,
                    Amount = group.Sum(item => item.Amount)
                }).ToList();

                foreach (var item in groupedTaxCategoryItems)
                {
                    sb.Append(@$"
                                                        <tr>
                                                            <td>{item.TaxCategoryItemName}</td>
                                                            <td>{item.Amount}</td>
                                                        </tr>
                    ");

                }

                sb.Append(@$"
                                                        <tr>
                                                            <th>Gross Amount</th>
                                                            <th> {quotation.CurrencySymbol} {(quotation.Items.Sum(item => item.GrossAmount))} </th>
                                                        </tr>
                                                        <tr>
                                                            <td>Payment Made </td>
                                                            <td style='color: red;'>  {quotation.CurrencySymbol} 0.00 </td>
                                                        </tr>
                                                        <tr>
                                                            <th style='padding-bottom:10px;'>Balance Due </th>
                                                            <th style='padding-bottom:10px;'>  {quotation.CurrencySymbol} {(quotation.Items.Sum(item => item.GrossAmount))}</th>
                                                        </tr>

                                                </table>
                                            </div>
                                            <div class='table-5''>
                                                <table style='width: 40%; height: 100px;border-left: 1px solid #000;'>

                                                    <tr>
                                                        <td  >Authorized Signature</td>
                                                    </tr>


                                                </table>
                                            </div>
                                            <div class='table-bottom' style='width: 99.7%; height: 200px;'>



                                            </div>

                                        </div>





                                    </body>
                                </html>
                ");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return "error";
            }
        }
        public async Task<QuotationPdfDetailsModel?> GetQuotationPdfDetailsModel(int quotationID, int branchID, IDbTransaction? tran = null)
        {

            try
            {
                var quotation = await _dbContext.GetByQueryAsync<QuotationPdfDetailsModel>($@"
                                                                     Select Q.QuotationNo,Q.Date,ExpiryDate,Q.CustomerEntityID,Q.CustomerNote,Q.TermsandCondition,Cust.TaxNumber,
                                                                     Q.BillingAddressID,Q.ShippingAddressID,E.Phone,E.EmailAddress,E.Name As CustomerName,
                                                                     M.FileName,BA.AddressLine1 as BillingAddressLine1,BA.AddressLine2 as BillingAddressLine2,BA.AddressLine3 as BillingAddressLine3,
                                                                     BA.State as BillingState,BA.PinCode as BillingPinCode,BC.CountryName as BillingCountry,
                                                                     SA.AddressLine1 as ShippingAddressLine1,SA.AddressLine2 as ShippingAddressLine2,SA.AddressLine3 as ShippingAddressLine3,
                                                                     SA.State as ShippingState,SA.PinCode as ShippingPinCode,SC.CountryName as ShippingCountry,Subject,
                                                                     CE.Name as ClientName,CEA.AddressLine1 as ClientAddressLine1,CEA.AddressLine2 as ClientAddressLine2,CEA.AddressLine3 as ClientAddressLine3,
                                                                     S.StateName as ClientState,CEA.PinCode as ClientPinCode,CBC.CountryName as ClientCountry,MM.FileName as ClientImage,MM.MediaID as ClientMediaID,C.GSTNo,
                                                                     Concat(CR.CurrencyName,' ( ',CR.Symbol,' )') AS CurrencyName,CR.Symbol AS CurrencySymbol,CR.MainSuffix,CR.SubSuffix,
                                                                     BusinessTypeName,cV.Name as StaffName,StaffPhoneNo,Q.BusinessTypeID,Q.QuotationCreatedFor,Description,cV.Phone2 as StaffPhone2
                                                                     From Quotation Q
                                                                     Left Join viEntity E ON E.EntityID=Q.CustomerEntityID
                                                                     Left Join Customer Cust ON Cust.EntityID = Q.CustomerEntityID
                                                                     Left Join EntityAddress BA on BA.AddressID=Q.BillingAddressID and BA.IsDeleted=0
                                                                     Left Join EntityAddress SA on SA.AddressID=Q.ShippingAddressID and SA.IsDeleted=0
                                                                     Left Join Country BC on BC.CountryID=BA.CountryID and BC.IsDeleted=0
                                                                     Left Join Country SC on SC.CountryID=SA.CountryID and SC.IsDeleted=0
                                                                     Left Join Media M ON M.MediaID=Q.MediaID and M.IsDeleted=0
                                                                     Left Join Client C on C.ClientID=Q.ClientID
                                                                     Left Join viEntity CE on CE.EntityID=C.EntityID
                                                                     Left Join EntityAddress CEA on CEA.EntityID=CE.EntityID and CEA.IsDeleted=0
                                                                     Left Join Country CBC on CBC.CountryID=CEA.CountryID and CBC.IsDeleted=0
                                                                     Left Join State S on S.StateID=CEA.StateID and S.IsDeleted=0
                                                                     Left Join Media MM on MM.MediaID=CE.MediaID and MM.Isdeleted=0
                                                                     Left Join Currency CR ON CR.CurrencyID=Q.CurrencyID AND CR.IsDeleted=0
                                                                     Left Join BusinessType BT on BT.BusinessTypeID=Q.BusinessTypeID and BT.IsDeleted=0
                                                                     Left Join viEntity cV on cV.EntityID=Q.QuotationCreatedFor
                                                                    Where Q.IsDeleted=0 and Q.BranchID={branchID} and Q.QuotationID={quotationID}
                ", null, tran);
                if (quotation.TermsandCondition != null)
                {
                    quotation.TermsList = quotation.TermsandCondition.Split('#').ToList();
                    if(quotation.TermsList.Count>0)
                    {
                        if (string.IsNullOrEmpty(quotation.TermsList[0]))
                        {
                            quotation.TermsList.RemoveAt(0);
                        }
                    }
                }
                if (quotation.CustomerNote != null)
                {
                    quotation.CustomerNoteList = quotation.CustomerNote.Split('#').ToList();
                    if (quotation.CustomerNoteList.Count > 0)
                    {
                        if (string.IsNullOrEmpty(quotation.CustomerNoteList[0]))
                        {
                            quotation.CustomerNoteList.RemoveAt(0);
                        }
                    }
                }
                //  quotation.BillingAddress = await _dbContext.GetByQueryAsync<AddressModel>($@"
                //                              Select EA.*,C.CountryName
                //                              From EntityAddress EA
                //                              Left Join Country C ON C.CountryID=EA.CountryID and C.IsDeleted=0
                //Where EA.IsDeleted=0 and EA.AddressID={quotation.BillingAddressID}
                //  ", null, tran);

                //  quotation.ShippingAddress = await _dbContext.GetByQueryAsync<AddressModel>($@"
                //                              Select EA.*,C.CountryName
                //                              From EntityAddress EA
                //                              Left Join Country C ON C.CountryID=EA.CountryID and C.IsDeleted=0
                //Where EA.IsDeleted=0 and EA.AddressID={quotation.ShippingAddressID}
                //  ", null, tran);

                quotation.Items = await _dbContext.GetListByQueryAsync<QuotationItemPDFModel>($@"
                                        Select ROW_NUMBER() OVER(ORDER BY (SELECT 1)) AS RowIndex,ItemName,QI.Description,TaxCategoryID,NetAmount,GrossAmount,TaxAmount,(Rate * Quantity) As TotalAmount,VI.Description as ItemDescription,
                                        QI.Quantity,QI.Rate,Discount
                                        From QuotationItem QI
										Left Join viItem VI on VI.ItemVariantID=QI.ItemVariantID
                                        Where QI.QuotationID={quotationID} and QI.IsDeleted=0
                ", null, tran);

                if (quotation.Items.Count > 0)
                {
                    foreach (var quotationItem in quotation.Items)
                    {
                        if(quotationItem.ItemDescription!=null)
                        {
                            quotationItem.ItemDescriptionList= quotationItem.ItemDescription.Split('#').ToList();
                            if (quotationItem.ItemDescriptionList.Count > 0)
                            {
                                if (string.IsNullOrEmpty(quotationItem.ItemDescriptionList[0]))
                                {
                                    quotationItem.ItemDescriptionList.RemoveAt(0);
                                }
                            }
                        }
                        quotationItem.TaxCategoryItems = await _dbContext.GetListByQueryAsync<QuotationItemTaxCategoryItemsModel>(@$"
                                                                            Select TaxCategoryItemID,Concat(TaxCategoryItemName,'   [ ',Convert(smallint,Percentage),' ]') AS TaxCategoryItemName,Percentage,0 As Amount
                                                                            From TaxCategoryItem T
                                                                            Where T.TaxCategoryID={quotationItem.TaxCategoryID} AND T.IsDeleted=0", null, tran);

                        if (quotationItem.TaxCategoryItems.Count > 0)
                        {
                            quotationItem.TaxCategoryItems
                                .ForEach(taxCategoryItem => taxCategoryItem.Amount = Math.Round(quotationItem.NetAmount * (taxCategoryItem.Percentage / 100), 2));

                        }
                    }
                }

                decimal totalAmount = quotation.Items.Where(item => item.GrossAmount != 0).Sum(item => item.GrossAmount);

                #region Total Amount to Currency conversion

                var AmtToWords = new CurrencyWordsConverter(new CurrencyWordsConversionOptions
                {
                    Culture = Culture.International,
                    OutputFormat = OutputFormat.English,
                    CurrencyUnit = quotation.MainSuffix,
                    SubCurrencyUnit = quotation.SubSuffix,
                    CurrencyUnitSeparator = "and"
                });
                quotation.AmountInWords = AmtToWords.ToWords(totalAmount);

                #endregion

                quotation.DomainUrl = _config.GetValue<string>("ServerURL");
                return quotation ?? new();
            }
            catch (Exception e)
            {
                return null;

            }
        }

        #endregion

        #region Invoice Pdf

        public async Task<int> CreateInvoicePdf(int invoiceID, int branchID, int clientID, string fileName, IDbTransaction? tran = null)
        {
            string htmlContent = "";
            int mediaID = 0;

            string folderName = "gallery/" + PDFType.Invoice.ToString().ToLower();
            if (!Directory.Exists(Path.Combine("wwwroot", folderName)))
            {
                Directory.CreateDirectory(Path.Combine("wwwroot", folderName));
            }
            string filePath = Path.Combine(_env.ContentRootPath, "wwwroot", folderName, $"{fileName}.pdf");
            string mediaFileName = $"/{folderName}/{fileName}.pdf";
            string cssfilePath = Path.Combine(_env.ContentRootPath, "wwwroot", "assets");
            cssfilePath += "/style.css";

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = DinkToPdf.PaperKind.A4Small,
                // Margins = new MarginSettings { Top = 10 },
                DocumentTitle = "Invoice Pdf Report",
                Out = filePath,
            };

            htmlContent = await GetInvoicePdfHtmlContent(invoiceID, clientID, tran);
            mediaID = await _dbContext.GetFieldsAsync<Invoice, int>("MediaID", $"BranchID={branchID} and QuotationID={invoiceID} and IsDeleted=0", null, tran);

            var objectSettings = new ObjectSettings
            {
                // PagesCount = true,
                HtmlContent = htmlContent,
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = cssfilePath },
                //  HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                // FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "Report Footer" }
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
            _converter.Convert(pdf);

            await Task.Delay(100);

            Media media = new Media
            {
                MediaID = mediaID,
                ContentType = "application/pdf",
                Extension = "pdf",
                FileName = mediaFileName
            };

            mediaID = await _dbContext.SaveAsync(media, tran);

            await _dbContext.ExecuteAsync($"Update Invoice set MediaID={mediaID} where InvoiceID={invoiceID} and BranchID={branchID} and IsDeleted=0", null, tran);

            return mediaID;
        }
        public async Task<string> GetInvoicePdfHtmlContent(int invoiceID, int clientID, IDbTransaction? tran = null)
        {
            StringBuilder invoicePdf = new();
            var invoicePdfDetails = await GetIvoicePdfDetails(invoiceID, clientID);
            StringBuilder clientAddress = new();
            StringBuilder customerAddress = new();
            StringBuilder shippingAddress = new();

            invoicePdf.Append(@"<!DOCTYPE html>
                    <html>
                    <head>
                      <style>
                        @import url('https://fonts.cdnfonts.com/css/bein-black');

                        html {
                          font-size: 14px;
                          font-family: Arial, Helvetica, sans-serif;
                        }

                        body {
                          margin: 0;
                        }

                        p {
                          margin: 5px 0;
                          font-size: 13px;
                        }

                        .pdf-page {
                          margin: 0 auto;
                          box-sizing: border-box;
                          color: #333;
                          position: relative;
                        }

                        .pdf-header {
                          position: absolute;
                          top: .5in;
                          height: .6in;
                          left: .5in;
                          right: .5in;
                          border-bottom: 1px solid #e5e5e5;
                        }

                        .invoice-number {
                          padding-top: .17in;
                          float: right;
                        }

                        .pdf-footer {
                          position: absolute;
                          bottom: .5in;
                          height: .6in;
                          left: .5in;
                          right: .5in;
                          padding-top: 10px;
                          border-top: 1px solid #e5e5e5;
                          text-align: left;
                          color: #787878;
                          font-size: 12px;
                        }

                        .pdf-body {
                          position: absolute;
                          top: 3.7in;
                          bottom: 1.2in;
                          left: .5in;
                          right: .5in;
                        }

                        .size-a4 {
                          width: 8.3in;
                          height: 11.7in;
                        }

                        .size-letter {
                          width: 8.5in;
                          height: 11in;
                        }

                        .size-executive {
                          width: 7.25in;
                        }

                        table.customTable {
                          width: 100%;
                          background-color: #FFFFFF;

                          color: #000000;

                        }

                        table.customTable td,
                        table.customTable th {
                          padding: 3px;
                          font-size: 12px;
                          line-height: 14px;
                          font-family: 'beIN Black', sans-serif;

                        }

                        table.customTable th {
                          color: #111;
                        }

                        table.customTablefooter td,
                        table.customTablefooter th {

                          padding: 2px !important;
                          font-size: 11px !important;
                        }

                        .table-top-data {
                          position: absolute;
                          top: 1%;
                          right: 14px;
                          left: 14px;
                        }

                        .rwd-table {

                          color: #fff;
                          border-radius: .4em;
                          overflow: hidden;
                        }

                        .rwd-table td {
                          border-color: #e5e5e5;
                          border: 1px solid #222;
                        }

                        .rwd-table th {
                          background-color: #e5e5e5;
                          padding: 10px !important;
                        }

                        .line-space{
                          line-height: 1.1rem !important;
                        }
                        
                      </style>
                    </head>

                    <body>
                        <div id='example'>
                            <div class='page-container hidden-on-narrow'>
                                <div class='pdf-page size-a4'>");

            if (invoicePdfDetails != null)
            {
                //Client details
                if (invoicePdfDetails.Client is not null)
                {
                    clientAddress.Append(@$"
                                    <div class='table-top-data'>
                                        <table style='width: 100%;'>
                                            <tr>
                                                <td style='vertical-align: bottom;'>
                                                    <table style='width: 100%;'>
                                                        <tr align='left'>
                                                            <td align='left' style='width: 20%;'><img src='{_config["ServerUrl"] + invoicePdfDetails.Client.ClientLogoFileName}' alt='' style='width: 150px;border-radius: 7PX;'></td>
                                                            <td align='right' style='width: 30%;position: absolute;top: 0;right: .15in;'>
                                                                <b style='text-align: right;'>{invoicePdfDetails.Client.ClientName}</b>
                    ");
                    if (invoicePdfDetails.Client.ClientAddress.CompleteAddress != null)
                    {
                        foreach (var addressLine in invoicePdfDetails.Client.ClientAddress.CompleteAddress.Split(',').Select((line, index) => new { index, line }))
                        {
                            clientAddress.Append($"             <p>{addressLine.line}{(addressLine.index != (addressLine.line.Length - 1) ? "," : "")}</p>");
                        }
                    }
                    clientAddress.Append(@$"
                                                                <p>{invoicePdfDetails.Client.TaxNumber}</p>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                        <br>
                                    </div>
                                    <br>");

                    invoicePdf.Append(clientAddress);
                }

                //Invoice detials
                invoicePdf.Append(@$"
                                    <table class='customTable' style='margin-top: 150px;'>
                                        <tbody>
                                            <tr>
                                                <th align='left' style='font-size: 16px;'>
                                                    TAX INVOICE
                                                </th>
                                                <th align='right'>
                                                </th>
                                            </tr>
                                            <tr>
                                                <td align='left'>Invoice number: <b>{invoicePdfDetails.Prefix}-{invoicePdfDetails.InvoiceNumber}</b></td>
                                                <td align='right'>Place Of Supply: <b>{invoicePdfDetails.PlaceOfSupplyName}</b></td>
                                            </tr>
                                            <tr>
                                                <td align='left'>Invoice Date: <b>{invoicePdfDetails.InvoiceDate.Value.ToString("dd/MM/yyyy")}</b></td>
                                                <td align='right'></td>
                                            </tr>
                                            <tr>
                                                <td align='left'>Terms: <b>{invoicePdfDetails.Terms}</b></td>
                                                <td align='right'></td>
                                            </tr>
                                            <tr>
                                                <td align='left'>Due Date: <b>{invoicePdfDetails.DueDate.Value.ToString("dd/MM/yyyy")}</b></td>
                                                <td align='right'></td>
                                            </tr>
                                        </tbody>
                                    </table>
                                <br>
                ");

                //Customer 
                customerAddress.Append(@$"
                                <table class='customTable ship' style='border: #e5e5e5 2px solid;'>
                                    <tr>
                                        <th style='width: 50%;text-align: left;background-color: #e5e5e5;'>
                                            Bill To
                                        </th>
                                        <th style='width: 50%;text-align: left;background-color: #e5e5e5;'>
                                            Ship To
                                        </th>
                                    </tr>
                                    <tr>
                                        <td class='line-space'>
                                            <b>{invoicePdfDetails.Customer.CustomerName}</b><br>");
                if (!string.IsNullOrEmpty(invoicePdfDetails.Customer.BillingAddress.CompleteAddress))
                {
                    foreach (var addressLine in invoicePdfDetails.Customer.BillingAddress.CompleteAddress.Split(',').Select((line, index) => new { index, line }))
                    {
                        customerAddress.Append($"{addressLine.line}{(addressLine.index != (addressLine.line.Length - 1) ? ",<br>" : "")}");
                    }
                }
                customerAddress.Append($@"{invoicePdfDetails.Customer.TaxNumber}<br></td>");

                customerAddress.Append(@$"<td class='line-space'>");
                if (invoicePdfDetails.Customer.ShippingAddressID is not null)
                {
                    customerAddress.Append(@$"<b>{invoicePdfDetails.Customer.CustomerName}</b><br>");
                    if (!string.IsNullOrEmpty(invoicePdfDetails.Customer.ShippingAddress.CompleteAddress))
                    {
                        foreach (var addressLine in invoicePdfDetails.Customer.ShippingAddress.CompleteAddress.Split(',').Select((line, index) => new { index, line }))
                        {
                            customerAddress.Append($"{addressLine.line}{(addressLine.index != (addressLine.line.Length - 1) ? ",<br>" : "")}");
                        }
                    }
                    customerAddress.Append($@"{invoicePdfDetails.Customer.TaxNumber}<br>");
                }
                customerAddress.Append("</td></tr></table><br>");
                invoicePdf.Append(customerAddress);

                invoicePdf.Append(@$"<table class='customTable'>
                                      <tbody>
                                        <tr>
                                          <td>Subject:</td>
                                        </tr>
                                        <tr>
                                          <td>{invoicePdfDetails.Subject}</td>
                                        </tr>
                                      </tbody>
                                    </table>
                                    <br>");

                //Items
                invoicePdf.Append(@$"<table class='customTable rwd-table' style=' border: 2px solid #e5e5e5;border-radius: 0;'>
                                          <tr>
                                            <th>#</th>
                                            <th>Item & Description</th>
                                            <th> Qty</th>
                                            <th>Rate</th>");

                foreach (var taxCategoryItemHeading in invoicePdfDetails.TaxCategoryHeadings)
                {
                    invoicePdf.Append(@$"<th>{taxCategoryItemHeading}</th>");
                }
                invoicePdf.Append(@$"<th>Amout</th></tr>");

                foreach (var item in invoicePdfDetails.Items.Select((value, index) => new { index, value }))
                {
                    item.value.TotalAmount = Math.Round(item.value.Quantity * item.value.Rate, 2);
                    invoicePdf.Append(@$"<tr>
                                            <td>{(item.index + 1)}</td>
                                            <td>{item.value.ItemName}</td>
                                            <td>{item.value.Quantity}</td>
                                            <td>{item.value.Rate}</td>");
                    foreach (var taxItem in item.value.TaxItems)
                    {
                        invoicePdf.Append(@$"<td>{taxItem.Amount} ({taxItem.Percentage}%)</td>");
                    }
                    invoicePdf.Append(@$"<td>{item.value.TotalAmount}</td></tr>");
                }

                //Item Summary
                invoicePdf.Append(@$"
                                  <tr>
                                    <td style='border: none;'></td>
                                    <td style='border: none;'></td>
                                    <td style='border: none;'></td>
                                    <td style='border: none;'></td>
                                    <td style='text-align: left;border: none;font-weight: 600;'>Sub Total</td>
                                    <td style='border: none;'></td>
                                    <td style='text-align: right;width: 94px;border: none;font-weight: 600;'>{Math.Round(invoicePdfDetails.Items.Sum(item => item.TotalAmount), 2)}</td>
                                  </tr>
                                  <tr>
                                    <td style='border: none;'></td>
                                    <td style='border: none;'></td>
                                    <td style='border: none;'></td>
                                    <td style='border: none;'></td>
                                    <td style='text-align: left;border: none;'>Total Discount</td>
                                    <td style='border: none;'></td>
                                    <td style='text-align: right;width: 94px;border: none;'>{Math.Round(invoicePdfDetails.Items.Sum(item => item.Discount + item.ChargeDiscountDivided), 2)}</td>
                                  </tr>
                                  <tr>
                                    <td style='border: none;'></td>
                                    <td style='border: none;'></td>
                                    <td style='border: none;'></td>
                                    <td style='border: none;'></td>
                                    <td style='text-align: left;border: none;font-weight: 600;'>Net Amount</td>
                                    <td style='border: none;'></td>
                                    <td style='text-align: right;width: 94px;border: none;font-weight: 600;'>{Math.Round(invoicePdfDetails.Items.Sum(item => item.NetAmount), 2)}</td>
                                  </tr>
                                  <tr>
                                    <td style='border: none;'></td>
                                    <td style='border: none;'></td>
                                    <td style='border: none;'></td>
                                    <td style='border: none;'></td>
                                    <td style='text-align: left;border: none;font-weight: 600;'>Tax Total</td>
                                    <td style='border: none;'></td>
                                    <td style='text-align: right;width: 94px;border: none;font-weight: 600;'>{Math.Round(invoicePdfDetails.Items.Sum(item => item.TaxAmount), 2)}</td>
                                  </tr>
                                  <tr>
                                    <td style='border: none;'></td>
                                    <td style='border: none;'></td>
                                    <td style='border: none;'></td>
                                    <td style='border: none;'></td>
                                    <td style='text-align: left;border: none;font-weight: 600;'>Gross Amount</td>
                                    <td style='border: none;'></td>
                                    <td style='text-align: right;width: 94px;border: none;font-weight: 600;'>{Math.Round(invoicePdfDetails.Items.Sum(item => item.GrossAmount), 2)}</td>
                                  </tr>
                ");

                //Tax Summary
                invoicePdf.Append(@$"<table class='customTable rwd-table' style='width:100%;'>
                                      <tr>
                                        <th style='background-color: transparent; text-align: left;'>Tax Summury</th>
                                      </tr>
                                    </table><br>");

                invoicePdf.Append(@$"<table class='customTable rwd-table' style='width:100%;  border: 2px solid #e5e5e5;border-radius: 0;' >
                                      <tr>
                                        <th style='width: 50%;'>Tax Details</th>
                                        <th>Taxable Amout</th> 
                                        <th>Tax Amout</th>
                                      </tr>");

                var allTaxCategoryItems = invoicePdfDetails.Items.SelectMany(item => item.TaxItems).ToList();
                var groupedTaxCategoryItems = allTaxCategoryItems
                .GroupBy(item => item.TaxCategoryItemID)
                .Select(group => new InvoicePdfItemTaxDetails()
                {
                    TaxCategoryItemID = group.Key,
                    TaxCategoryID = group.First().TaxCategoryID,
                    CategoryName = group.First().CategoryName + $" [{group.First().Percentage}]",
                    Percentage = group.First().Percentage
                }).ToList();

                foreach (var taxCategoryItem in groupedTaxCategoryItems)
                {
                    decimal taxableAmount = invoicePdfDetails.Items
                        .Where(item => item.TaxCategoryID == taxCategoryItem.TaxCategoryID)
                        .Sum(invoiceItem => invoiceItem.NetAmount);
                    taxableAmount = Math.Round(taxableAmount, 2);
                    taxCategoryItem.Amount = taxableAmount * (taxCategoryItem.Percentage / 100);

                    invoicePdf.Append(@$"<tr>
                                            <td>{taxCategoryItem.CategoryName}</td>
                                            <td>{taxableAmount}</td>
                                            <td>{taxCategoryItem.Amount}</td>
                                          </tr>");
                }

                invoicePdf.Append(@$"<tr>
                                        <th style='background-color: transparent;text-align: left;'>Total</th>
                                        <th style='background-color: transparent;text-align: left;'>{Math.Round(invoicePdfDetails.Items.Sum(item => item.NetAmount), 2)}</th>
                                        <th style=""background-color: transparent;text-align: left;"">{Math.Round(invoicePdfDetails.Items.Sum(item => item.TaxAmount), 2)}</th>
                                      </tr></table>");

                //Footer
                invoicePdf.Append(@$"<table class='customTable customTablefooter'>
                                        <tr>
                                            <td class=''>Total In Words:</td>
                                        </tr>
                                        <tr>
                                            <td class='' style='font-size: 11px !important;width: 50%;'><b>{(invoicePdfDetails.CurrencyName + " (" + invoicePdfDetails.Symbol + ")")} {invoicePdfDetails.AmountInWords} only</b></td>
                                            <td class='' style='margin-top: 10px !important;text-align: right;padding: 0 !important;'>Terms & Conditions</td>
                                        </tr>
                                        <tr>
                                            <td class='' style='font-size: 11px !important;width: 50%;'>{invoicePdfDetails.CustomerNote}</td>
                                            <td class='' style='margin-top: 10px !important;text-align: right;padding: 0 !important;'>{invoicePdfDetails.TermsandCondition}</td>
                                        </tr>
                                        <tr>
                                            <td class='gradient-3' style='width:25%;'><b>Authorized Signature</b></td>
                                        </tr>
                                    </table>
                                </div>
                            </div>
                        </div>
                    </body>
                </html>");

            }
            return invoicePdf.ToString();
        }
        public async Task<InvoicePdfModel> GetIvoicePdfDetails(int invoiceID, int clientID)
        {
            var invoicePdf = await _dbContext.GetByQueryAsync<InvoicePdfModel>(@$"Select I.Prefix,I.InvoiceNumber,I.InvoiceDate,I.Subject,I.CustomerEntityID,Concat(P.StateName,' (',P.StateCode,')') As PlaceOfSupplyName,I.CustomerNote,I.TermsandCondition,CR.CurrencyName,CR.Symbol,CR.MainSuffix,CR.SubSuffix
                                                                                    From Invoice I
																					Join CountryState P ON P.StateID=I.PlaceOfSupplyID
                                                                                    Join Currency CR ON CR.CurrencyID=I.CurrencyID
                                                                                    Where InvoiceID={invoiceID}", null);
            invoicePdf.DueDate = invoicePdf.InvoiceDate.Value.AddDays(7);

            #region Client details

            invoicePdf.Client = await _dbContext.GetByQueryAsync<InvoicePdfClientDetails>(@$"Select E.Name As ClientName,EA.AddressID,E.ProfileImage As ClientLogoFileName
                                                                                    From Client C
                                                                                    Join viEntity E ON E.EntityID=C.EntityID
                                                                                    Join EntityAddress EA ON EA.EntityID=C.EntityID
                                                                                    Where C.ClientID={clientID}", null);

            invoicePdf.Client.ClientAddress = await _customer.GetAddressView(invoicePdf.Client.AddressID);
            invoicePdf.Client.TaxNumber = await _dbContext.GetFieldsAsync<ClientCustom, string?>("GSTNo", $"ClientID={clientID}", null);

            #endregion

            #region Customer details

            invoicePdf.Customer = await _dbContext.GetByQueryAsync<InvoicePdfCustomerDetails>(@$"Select E.Name As CustomerName,C.TaxNumber,I.BillingAddressID,I.ShippingAddressID
                                                                                    From Invoice I
                                                                                    Join Customer C ON C.EntityID=I.CustomerEntityID
                                                                                    Join viEntity E ON E.EntityID=C.EntityID
                                                                                    Where I.InvoiceID={invoiceID}", null);
            invoicePdf.Customer.BillingAddress = await _customer.GetAddressView(invoicePdf.Customer.BillingAddressID);
            if (invoicePdf.Customer.ShippingAddressID is not null)
                invoicePdf.Customer.ShippingAddress = await _customer.GetAddressView(invoicePdf.Customer.ShippingAddressID.Value);

            #endregion

            #region Invoice items

            invoicePdf.Items = await _dbContext.GetListByQueryAsync<InvoicePdfItemDetails>(@$"Select IT.ItemName,II.*
                                                                                    From Invoice I
                                                                                    Join InvoiceItem II ON I.InvoiceID=II.InvoiceID And II.IsDeleted=0
                                                                                    Join viItem IT ON IT.ItemVariantID=II.ItemVariantID
                                                                                    Where I.InvoiceID={invoiceID}", null);
            var taxCategoryHeadings = await _dbContext.GetByQueryAsync<string>($"Select TaxCategoryItemNames From ClientSetting Where ClientID={clientID} And IsDeleted=0", null);
            string[] items = System.Array.Empty<string>();
            if (!string.IsNullOrEmpty(taxCategoryHeadings))
                items = taxCategoryHeadings.Split(',');
            invoicePdf.TaxCategoryHeadings = new List<string>(items);

            foreach (var item in invoicePdf.Items)
            {
                var taxCategory = await _superAdmin.GetTaxCategoryDetailsById(item.TaxCategoryID);
                foreach (var taxCategoryItem in taxCategory.TaxCategoryItems)
                {
                    InvoicePdfItemTaxDetails invoicePdfItemTaxDetails = new()
                    {
                        Amount = Math.Round(item.NetAmount * (taxCategoryItem.Percentage / 100), 2),
                        CategoryName = taxCategoryItem.TaxCategoryItemName,
                        Percentage = taxCategoryItem.Percentage,
                        TaxCategoryItemID = taxCategoryItem.TaxCategoryItemID,
                        TaxCategoryID = taxCategoryItem.TaxCategoryID
                    };
                    item.TaxItems.Add(invoicePdfItemTaxDetails);
                }
            }

            #endregion

            #region Total Amount to Currency conversion

            decimal totalAmount = invoicePdf.Items.Where(item => item.GrossAmount != 0).Sum(item => item.GrossAmount);
            var AmtToWords = new CurrencyWordsConverter(new CurrencyWordsConversionOptions
            {
                Culture = Culture.International,
                OutputFormat = OutputFormat.English,
                CurrencyUnit = invoicePdf.MainSuffix,
                SubCurrencyUnit = invoicePdf.SubSuffix,
                CurrencyUnitSeparator = "and"
            });
            invoicePdf.AmountInWords = AmtToWords.ToWords(totalAmount);

            #endregion

            return invoicePdf;
        }


        #endregion

        #region Item Qr code

        public async Task<string> GetItemQrCodePdfFile(int clientID, IDbTransaction? tran = null)
        {
            #region Filename

            string folderName = "gallery/ItemQRCode";
            if (!Directory.Exists(Path.Combine("wwwroot", folderName)))
            {
                Directory.CreateDirectory(Path.Combine("wwwroot", folderName));
            }
            string fileName = $"{clientID}-item-qr-codes.pdf";
            string filePath = Path.Combine(_env.ContentRootPath, "wwwroot", folderName, fileName);
            string mediaFileName = $"/{folderName}/{fileName}";

            #endregion

            var doc = new HtmlToPdfDocument();
            PagedListPostModel postModel = new() { PageSize = 12, PageIndex = 1 };
            ItemQrCodeResponseMode responseData;

            do
            {
                responseData = await GetItemQrPdfHtmlContentAndData(clientID, postModel);
                if (responseData != null)
                {
                    doc.Objects.Add(new ObjectSettings()
                    {
                        HtmlContent = responseData.HtmlContent,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    });

                    if (responseData.Result.TotalPages >= postModel.PageIndex)
                    {
                        postModel.PageIndex++;
                    }
                }
            }
            while (responseData != null && responseData.Result.TotalPages >= postModel.PageIndex);

            doc.GlobalSettings = new GlobalSettings
            {
                PaperSize = PaperKind.A4,
                Orientation = Orientation.Portrait,
                DocumentTitle = "Item QR Code Report",
                Out = filePath,
                Margins = new MarginSettings
                {
                    Top = 10,
                    Left = 10,
                    Right = 10,
                    Bottom = 10
                }
            };
            _converter.Convert(doc);

            #region Media & Client Setting updation

            int? mediaID = await _dbContext.GetFieldsAsync<ClientSetting, int?>("ItemQrPdfMediaID", $"ClientID={clientID}", null);
            if (mediaID is null)
            {
                Media media = new Media
                {
                    MediaID = Convert.ToInt32(mediaID),
                    ContentType = "application/pdf",
                    Extension = "pdf",
                    FileName = mediaFileName
                };
                media.MediaID = await _dbContext.SaveAsync(media);

                var clientSetting = await _dbContext.GetByQueryAsync<ClientSetting>($"Select * From ClientSetting Where ClientID={clientID} And IsDeleted=0", null);
                clientSetting = clientSetting ?? new() { ClientID = clientID };
                clientSetting.ItemQrPdfMediaID = media.MediaID;
                await _dbContext.SaveAsync(clientSetting);
            }

            #endregion

            return mediaFileName;
        }
        public async Task<ItemQrCodeResponseMode> GetItemQrPdfHtmlContentAndData(int clientID, PagedListPostModel searchModel)
        {
            PagedListQueryModel query = searchModel;
            query.Select = @"Select ItemName,ItemCode,M.FileName As QrCodeImageFile 
                                From Item I
                                Left Join Media M ON I.QrCodeMediaID=M.MediaID";
            query.WhereCondition = $"I.ClientID={clientID} And I.IsDeleted=0";
            var result = await _dbContext.GetPagedList<ItemQrCodePdfModel>(query, null);

            var pdfHtmlContent = new StringBuilder();
            pdfHtmlContent.Append(@"
                <!doctype html>
                <html lang='en' dir='ltr'>

                <head>

                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0, user-scalable=0'>
                    <meta http-equiv='X-UA-Compatible' content='IE=edge'>
                    <meta name='description' content=''>
                    <meta name='author' content=''>
                    <meta name='keywords' content=''>
                    <link rel='shortcut icon' type='image/png' href='/assets/images/brand/fevicon.png' />
                    <title></title>

                    <style>
                        .nw-btn .btn {
                            cursor: pointer;
                            font-weight: 700;
                            letter-spacing: 0.03em;
                            font-size: 0.8125rem;
                            min-width: 12.375rem;
                            font-size: 19px;
                        } 

                        .qr-img{
                            width:400px
                        }

                        .btn {
                            display: inline-block;
                            font-weight: 400;
                            text-align: center;
                            white-space: nowrap;
                            vertical-align: middle;
                            -webkit-user-select: none;
                            -moz-user-select: none;
                            -ms-user-select: none;
                            user-select: none;
                            border: 1px solid transparent;
                            padding: 0.375rem 1rem;
                            font-size: 0.9375rem;
                            line-height: 1.84615385;
                            border-radius: 5px;
                            transition: color 0.15s ease-in-out, background-color 0.15s ease-in-out, border-color 0.15s ease-in-out, box-shadow 0.15s ease-in-out;
                        }

                        .btn-primary {
                            color: #fff;
                            background-color: #00A2B8;
                            border-color: #0d6efd;
                        }

                        img {
                            vertical-align: middle;
                            border-style: none;
                            max-width: 70%;
                        }

                        h3 {
                            margin-bottom: 0.50em;
                            font-family: inherit;
                            font-weight: 400;
                            line-height: 1.1;
                            color: inherit;
                            font-family: 'IBM Plex Sans', sans-serif;
                        }

                        h3,
                        .h3 {
                            font-size: 1.2rem;
                        }

                        .card-body {
                            -ms-flex: 1 1 auto;
                            flex: 1 1 auto;
                            padding: 25px;
                            margin: 0;
                            position: relative;
                        }

                        .text-wrap table th,
                        .text-wrap table td {
                            padding: 0.75rem;
                            vertical-align: top;
                        }

                        table {
                            caption-side: bottom;
                            border-collapse: collapse;
                        }

                        @media print {
                            .no-print {
                                display: none;
                            }
                        }
                    </style>
                </head>
                <body>
                    <div class='text-wrap'>
                        <table style='width: 100%;'>
            ");

            int itemsPerGroup = 3;
            int totalItems = result.Data.Count;
            for (int i = 0; i < totalItems; i++)
            {
                if (i % itemsPerGroup == 0)
                {
                    pdfHtmlContent.Append("<tr>");
                }

                pdfHtmlContent.Append($@"<td style='width: 33%;'>
                    <img class='qr-img'  src='{_config["ServerUrl"] + "/" + result.Data[i].QrCodeImageFile}' alt='QR Code'>
                    <h3>{result.Data[i].ItemName}</h3>
                </td>");

                if ((i + 1) % itemsPerGroup == 0 || i == totalItems - 1)
                {
                    pdfHtmlContent.Append("</tr>");
                }
            }

            pdfHtmlContent.Append(@$"
                        </table>
                    </div>
                </body>
            </html>");

            ItemQrCodeResponseMode responseModel = new()
            {
                Result = result,
                HtmlContent = pdfHtmlContent.ToString()
            };

            return responseModel;
        }

        #endregion

        #region Item Group Qr code

        public async Task<string> GetItemGroupQrCodePdfFile(int clientID, IDbTransaction? tran = null)
        {
            #region Filename

            string folderName = "gallery/ItemGroupQRCode";
            if (!Directory.Exists(Path.Combine("wwwroot", folderName)))
            {
                Directory.CreateDirectory(Path.Combine("wwwroot", folderName));
            }
            string fileName = $"{clientID}-item-group-qr-codes.pdf";
            string filePath = Path.Combine(_env.ContentRootPath, "wwwroot", folderName, fileName);
            string mediaFileName = $"/{folderName}/{fileName}";

            #endregion

            var doc = new HtmlToPdfDocument();
            PagedListPostModel postModel = new() { PageSize = 12, PageIndex = 1 };
            ItemGroupQrCodeResponseMode responseData;

            do
            {
                responseData = await GetItemGroupQrPdfHtmlContentAndData(clientID, postModel);
                if (responseData != null)
                {
                    doc.Objects.Add(new ObjectSettings()
                    {
                        HtmlContent = responseData.HtmlContent,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    });

                    if (responseData.Result.TotalPages >= postModel.PageIndex)
                    {
                        postModel.PageIndex++;
                    }
                }
            }
            while (responseData != null && responseData.Result.TotalPages >= postModel.PageIndex);

            doc.GlobalSettings = new GlobalSettings
            {
                PaperSize = PaperKind.A4,
                Orientation = Orientation.Portrait,
                DocumentTitle = "Item Group QR Code Report",
                Out = filePath,
                Margins = new MarginSettings
                {
                    Top = 10,
                    Left = 10,
                    Right = 10,
                    Bottom = 10
                }
            };
            _converter.Convert(doc);

            #region Media & Client Setting updation

            int? mediaID = await _dbContext.GetFieldsAsync<ClientSetting, int?>("ItemGroupQrPdfMediaID", $"ClientID={clientID}", null);
            if (mediaID is null)
            {
                Media media = new Media
                {
                    MediaID = Convert.ToInt32(mediaID),
                    ContentType = "application/pdf",
                    Extension = "pdf",
                    FileName = mediaFileName
                };
                media.MediaID = await _dbContext.SaveAsync(media);

                var clientSetting = await _dbContext.GetByQueryAsync<ClientSetting>($"Select * From ClientSetting Where ClientID={clientID} And IsDeleted=0", null);
                clientSetting = clientSetting ?? new() { ClientID = clientID };
                clientSetting.ItemGroupQrPdfMediaID = media.MediaID;
                await _dbContext.SaveAsync(clientSetting);
            }

            #endregion

            return mediaFileName;
        }
        public async Task<ItemGroupQrCodeResponseMode> GetItemGroupQrPdfHtmlContentAndData(int clientID, PagedListPostModel searchModel)
        {
            PagedListQueryModel query = searchModel;
            query.Select = @"Select G.GroupName,M.FileName As QrCodeImageFile
                                From ItemGroup G
                                Left Join Media M ON G.QrCodeMediaID=M.MediaID";
            query.WhereCondition = $"G.ClientID={clientID} And G.IsDeleted=0";
            var result = await _dbContext.GetPagedList<ItemGroupQrCodePdfModel>(query, null);

            var pdfHtmlContent = new StringBuilder();
            pdfHtmlContent.Append(@"
                <!doctype html>
                <html lang='en' dir='ltr'>

                <head>

                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0, user-scalable=0'>
                    <meta http-equiv='X-UA-Compatible' content='IE=edge'>
                    <meta name='description' content=''>
                    <meta name='author' content=''>
                    <meta name='keywords' content=''>
                    <link rel='shortcut icon' type='image/png' href='/assets/images/brand/fevicon.png' />
                    <title></title>

                    <style>
                        .nw-btn .btn {
                            cursor: pointer;
                            font-weight: 700;
                            letter-spacing: 0.03em;
                            font-size: 0.8125rem;
                            min-width: 12.375rem;
                            font-size: 19px;
                        } 

                        .qr-img{
                            width:400px
                        }

                        .btn {
                            display: inline-block;
                            font-weight: 400;
                            text-align: center;
                            white-space: nowrap;
                            vertical-align: middle;
                            -webkit-user-select: none;
                            -moz-user-select: none;
                            -ms-user-select: none;
                            user-select: none;
                            border: 1px solid transparent;
                            padding: 0.375rem 1rem;
                            font-size: 0.9375rem;
                            line-height: 1.84615385;
                            border-radius: 5px;
                            transition: color 0.15s ease-in-out, background-color 0.15s ease-in-out, border-color 0.15s ease-in-out, box-shadow 0.15s ease-in-out;
                        }

                        .btn-primary {
                            color: #fff;
                            background-color: #00A2B8;
                            border-color: #0d6efd;
                        }

                        img {
                            vertical-align: middle;
                            border-style: none;
                            max-width: 70%;
                        }

                        h3 {
                            margin-bottom: 0.50em;
                            font-family: inherit;
                            font-weight: 400;
                            line-height: 1.1;
                            color: inherit;
                            font-family: 'IBM Plex Sans', sans-serif;
                        }

                        h3,
                        .h3 {
                            font-size: 1.2rem;
                        }

                        .card-body {
                            -ms-flex: 1 1 auto;
                            flex: 1 1 auto;
                            padding: 25px;
                            margin: 0;
                            position: relative;
                        }

                        .text-wrap table th,
                        .text-wrap table td {
                            padding: 0.75rem;
                            vertical-align: top;
                        }

                        table {
                            caption-side: bottom;
                            border-collapse: collapse;
                        }

                        @media print {
                            .no-print {
                                display: none;
                            }
                        }
                    </style>
                </head>
                <body>
                    <div class='text-wrap'>
                        <table style='width: 100%;'>
            ");

            int itemsPerGroup = 3;
            int totalItems = result.Data.Count;
            for (int i = 0; i < totalItems; i++)
            {
                if (i % itemsPerGroup == 0)
                {
                    pdfHtmlContent.Append("<tr>");
                }

                pdfHtmlContent.Append($@"<td style='width: 33%;'>
                    <img class='qr-img'  src='{_config["ServerUrl"] + "/" + result.Data[i].QrCodeImageFile}' alt='QR Code'>
                    <h3>{result.Data[i].GroupName}</h3>
                </td>");

                if ((i + 1) % itemsPerGroup == 0 || i == totalItems - 1)
                {
                    pdfHtmlContent.Append("</tr>");
                }
            }

            pdfHtmlContent.Append(@$"
                        </table>
                    </div>
                </body>
            </html>");

            ItemGroupQrCodeResponseMode responseModel = new()
            {
                Result = result,
                HtmlContent = pdfHtmlContent.ToString()
            };

            return responseModel;
        }

        #endregion

        #region Client Invoice pdf

        public async Task<int> CreateClientInvoicePdf(int invoiceID, string fileName, IDbTransaction? tran = null)
        {
            string htmlContent = "";
            int? mediaID = null;
            try
            {

                string folderName = "gallery/ClientInvoice";

                if (!Directory.Exists(Path.Combine("wwwroot", folderName)))
                {
                    Directory.CreateDirectory(Path.Combine("wwwroot", folderName));
                }

                string filePath = Path.Combine(_env.ContentRootPath, "wwwroot", folderName, $"{fileName}.pdf");

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);

                }

                await Task.Delay(50);

                string mediaFileName = $"/{folderName}/{fileName}.pdf";

                string cssfilePath = Path.Combine(_env.ContentRootPath, "wwwroot", "assets");
                cssfilePath += "/style.css";

                var globalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = DinkToPdf.PaperKind.A4Small,
                    // Margins = new MarginSettings { Top = 10 },
                    DocumentTitle = "PDF Report",
                    Out = filePath,
                };

                // Reading the html string content and MediaID if the pdf was previously generated

                htmlContent = await GetClientInvoicePdfHtmlContent(invoiceID, tran);
                mediaID = await _dbContext.GetFieldsAsync<ClientInvoice, int?>("InvoiceMediaID", $"InvoiceID={invoiceID} and IsDeleted=0", null, tran);


                var objectSettings = new ObjectSettings
                {
                    // PagesCount = true,
                    HtmlContent = htmlContent,
                    WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = cssfilePath },
                    //  HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                    // FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "Report Footer" }
                };
                var pdf = new HtmlToPdfDocument()
                {
                    GlobalSettings = globalSettings,
                    Objects = { objectSettings }
                };
                _converter.Convert(pdf);
                await Task.Delay(100);

                Media media = new Media
                {
                    MediaID = Convert.ToInt32(mediaID),
                    ContentType = "application/pdf",
                    Extension = "pdf",
                    FileName = mediaFileName
                };

                mediaID = await _dbContext.SaveAsync(media, tran);

                await _dbContext.ExecuteAsync($"Update ClientInvoice set InvoiceMediaID={mediaID} where InvoiceID={invoiceID} and IsDeleted=0", null, tran);

                return mediaID.Value;
            }
            catch (Exception e)
            {
                return 0;
            }

        }
        public async Task<string> GetClientInvoicePdfHtmlContent(int InvoiceID, IDbTransaction? tran = null)
        {
            try
            {
                var clientInvoice = await GetClientInvoicePDFDataModel(InvoiceID, tran);

                var CompanyAddress = new StringBuilder();

                CompanyAddress.Append(@$"
                        {clientInvoice.companyDetails.CompanyName}<br>
                ");
                if (clientInvoice.companyDetails.CompanyAddress1 != null)
                {
                    CompanyAddress.Append(@$"
                        {clientInvoice.companyDetails.CompanyAddress1},<br>
                ");
                }
                if (clientInvoice.companyDetails.CompanyAddress2 != null)
                {
                    CompanyAddress.Append(@$"
                        {clientInvoice.companyDetails.CompanyAddress2},<br>
                ");
                }
                if (clientInvoice.companyDetails.CompanyAddress3 != null)
                {
                    CompanyAddress.Append(@$"
                        {clientInvoice.companyDetails.CompanyAddress3},<br>
                ");
                }
                if (clientInvoice.companyDetails.CompanyCity != null)
                {
                    CompanyAddress.Append(@$"
                        {clientInvoice.companyDetails.CompanyCity},<br>
                ");
                }
                if (clientInvoice.companyDetails.CompanyState != null)
                {
                    CompanyAddress.Append(@$"
                        {clientInvoice.companyDetails.CompanyState},<br>
                ");
                }
                if (clientInvoice.companyDetails.CompanyCountry != null)
                {
                    CompanyAddress.Append(@$"
                        {clientInvoice.companyDetails.CompanyCountry},<br>
                ");
                }
                if (clientInvoice.companyDetails.CompanyPincode != null)
                {
                    CompanyAddress.Append(@$"
                        {clientInvoice.companyDetails.CompanyPincode},<br>
                ");
                }
                if (clientInvoice.companyDetails.CompanyGSTNo != null)
                {
                    CompanyAddress.Append(@$"
                        {clientInvoice.companyDetails.CompanyGSTNo}
                ");
                }



                var ClientAddress = new StringBuilder();

                ClientAddress.Append(@$"
                        {clientInvoice.ClientName}<br>
                ");
                if (clientInvoice.ClientAddress1 != null)
                {
                    ClientAddress.Append(@$"
                        {clientInvoice.ClientAddress1},<br>
                ");
                }
                if (clientInvoice.ClientAddress2 != null)
                {
                    ClientAddress.Append(@$"
                        {clientInvoice.ClientAddress2},<br>
                ");
                }
                if (clientInvoice.ClientAddress3 != null)
                {
                    ClientAddress.Append(@$"
                        {clientInvoice.ClientAddress3},<br>
                ");
                }
                if (clientInvoice.ClientCity != null)
                {
                    ClientAddress.Append(@$"
                        {clientInvoice.ClientCity},<br>
                ");
                }
                if (clientInvoice.ClientState != null)
                {
                    ClientAddress.Append(@$"
                        {clientInvoice.ClientState},<br>
                ");
                }
                if (clientInvoice.ClientCountry != null)
                {
                    ClientAddress.Append(@$"
                        {clientInvoice.ClientCountry},<br>
                ");
                }
                if (clientInvoice.ClientPincode != null)
                {
                    ClientAddress.Append(@$"
                        {clientInvoice.ClientPincode},<br>
                ");
                }
                if (clientInvoice.ClientGSTNo != null)
                {
                    ClientAddress.Append(@$"
                        {clientInvoice.ClientGSTNo}
                ");
                }



                clientInvoice.Discount = Math.Round(clientInvoice.Discount, 2);
                decimal NetFee = Math.Round(clientInvoice.Fee - clientInvoice.Discount, 2);
                decimal Tax = Math.Round(NetFee * (clientInvoice.TaxPercentage / 100), 2);
                decimal GrossFee = Math.Round(NetFee + Tax, 2);

                var sb = new StringBuilder();

                sb.Append($@"<!DOCTYPE html>
                        <html lang='en'>
                        <head>
                            <meta charset='UTF-8'>
                            <meta http-equiv='X-UA-Compatible' content='IE=edge'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                            <title>Invoice</title>
                 ");
                sb.Append($@"   <style>

                                .invoice {{
                                    width: 100%;
                                    max-width: 800px;
                                    margin: 10px auto;
	                                height: 1050px;
	                                border:1px solid #000000 !important;
	                                border-collapse: collapse;
                                    font-size: 12px;
                                }}

                                @media print {{
                                    @page {{
                                        size: A4;
                                        margin: 0;
                                    }}

                                    body {{
                                        margin: 1cm;
                                    }}
                                }}

                                .main-table table thead td{{
                                    padding: 10px;
	                                border-bottom: 1px solid #000000;
	  
                                }}

	                            .main-table th{{
		                            border-bottom: 1px solid #000000;
			                            border-right: 1px solid #000000;
			                            border-collapse: collapse;

	                            }}

	                            .main-table tbody  td{{
	
		                            border-bottom: 1px solid #000000;
		                            border-right: 1px solid #000000;
		                            border-collapse: collapse;
	                            }}	

	                            .main-table table{{
			                            border-top: 1px solid #000000;
			                            border-bottom: 1px solid #000000;
			
			                            border-collapse: collapse;
		                        }}

		                        .main-table td{{
			                        text-align: center;
		                        }}

                                .main-table th{{
                                    background-color: #141212;
	                                color: #ddd;
                                }}
		                        .table-1 th{{
			                        text-align: left;

		                        }}

		                        .table-1 table{{
			                        border-top: 1px solid #000000;
			                        border-bottom: 1px solid #000000;
			                        border-collapse: collapse;
			                        float: left;
		                        }}

		                        .table-1 td{{
			                        padding-left: 10px;
		                        }}

		                        .table-4{{
			                        float: right;
		                        }}

		                        .table-4 {{
			                        border-left: 1px solid #000000;
			                        border-bottom: 1px solid #000000;
			                        border-collapse: collapse;
		                        }}

		                        .table-4 th , .table-4 td {{
			                        text-align: right;
			                        padding-right: 10px;
		                        }}

                                .tab-1 table{{
	                                border-top: 1px solid #000000;
	
                                }}
                            </style>
                      </head>
                ");

                sb.Append($@"<body>
                        <div class='invoice'>
                            <div class='header'>
                                <table style='width: 100%;'>
                                    <tr>
                                        <td style='width: 23%; padding-left: 10px;'><img style='width: 160px; height: 160px;' src='{clientInvoice.companyDetails.CompanyProfileImage}' alt=''> </td>
                                        <td style='text-align: start; padding-left: 10px; width: 53%;'><h3>Progbiz Private Limited</h3>{CompanyAddress}</td>
                                        <td style='text-align: end; padding-top: 120px; padding-right: 10px;  width: 24%;'><h1>INVOICE </h1></td>

                                    </tr>
                                    <tr>
                                        <td></td>
                                        <td></td>
                                        <th style='text-align: end;  padding-right: 10px; '>Invoice No: #{clientInvoice.InvoiceNo}</th>
                                    </tr>

                                </table>
          
                            </div>");
                sb.Append($@"<div class='tab-1'>
                            <table style='width: 100%;'>
                                <tr>
                                    <th style='text-align: start; padding-left: 10px; padding-top: 20px; '>Client Details:</th>
                                </tr>
                                <tr>
                                    <td style='width: 12%; padding-left: 10px;'><img style='width: 80px; height: 80px;' src='assets/logoipsum-235.webp' alt=''> </td>  
                                    <td style='text-align: start; padding-left: 10px; '>{ClientAddress} </td>
                   
                                </tr>
                            </table>
                        </div>");
                sb.Append($@"  <div class='table-1' style='padding-bottom: 20px; padding-top: 20px; '>
                            <table style='width: 100%; height: 57px;'>
                                <tr>
                                    <td style='width: 100px;'>Start Date </td>
                                    <th  style='border-right: 1px solid #000000;'>: {clientInvoice.InvoiceDate.Value.ToString("dd/MM/yyyy")}</th>
                                    <td style='width: 120px;'>Disconnection Date</td>
                                    <th  >: {clientInvoice.DisconnectionDate.Value.ToString("dd/MM/yyyy")}</th>
                                </tr>
                              </table>
                         </div>
                 ");
                sb.Append($@"<div class='main-table' style='padding-top: 51px;'>
                            <table style='width: 100%;'>
                                <thead >
                   
                                 <tr >
                                    <th rowspan='2' style='padding-left: 10px; text-align: left; '>#</th>
                                    <th rowspan='2' style='max-width: 120px;'>Package Name </th>
                                    <th rowspan='2' style='max-width: 128px;'> Description</th>
                                    <th rowspan='2' style='max-width: 100px;'> Features </th>
                                    <th rowspan='2'>Capacity</th>
                                    <th rowspan='2'>Billing Type</th>
                                    <th rowspan='2'>Amount</th>
                                    <th rowspan='2' style='border-right: none ' > Discount</th>
                                </tr>
                
                            </thead>
                        <tbody>

                            <tr>
                                <td style='padding-left: 10px; text-align: left;'>1</td>
                                <td style='color:gray; text-align: left; padding: 10px; max-width: 120px;'>{clientInvoice.PackageName}</td>
                                <td style='max-width: 128px;'>{clientInvoice.PackageDescription}</td>
                                <td style='max-width: 100px;'>{clientInvoice.AvailableFeatures}</td>
                                <td>{clientInvoice.Capacity}</td>
                                <td>{clientInvoice.PlanName}</td>
                                <td>{clientInvoice.Fee}</td>
                   
                                <td style='border-right: none '>{clientInvoice.Discount}</td>
                            </tr>   
                        </tbody>
                    </table>
                </div>
                ");

                sb.Append($@"   <div style='width: 100%; ' >
                                <table class='table-4' style='width: 40%; '>
                                <tr>
                                    <td style='padding-top:10px;'>Total Amount</td>
                                    <td style='padding-top:10px;'>{clientInvoice.Fee} </td>
                                </tr>
                                <tr>
                                    <th>Discount</th>
                                    <td>{clientInvoice.Discount}</td>
                                </tr>
                                <tr>
                                    <th>Net Amount</th>
                                    <td>{NetFee}</td>
                                </tr>
                                <tr>
                                    <th>Tax %</th>
                                    <td>{clientInvoice.TaxPercentage}</td>
                                </tr>
                ");

                if (clientInvoice.Taxcategoryitem.Count > 0)
                {
                    foreach (var item in clientInvoice.Taxcategoryitem)
                    {
                        decimal? taxCategoryItemAmount = Math.Round((NetFee * (item.Percentage / 100)), 2);
                        sb.Append($@"   <tr>
                                            <td>{item.TaxCategoryItemName}</td>
                                            <td>{taxCategoryItemAmount}</td>
                                        </tr>
                        ");
                    }
                }

                sb.Append($@" 
                                    <tr>
                                        <th>Tax </th>
                                        <td>{Tax}</td>
                                    </tr>
                                    <tr>
                                        <th style='padding-bottom:10px; color: red;'>Gross Amount </th>
                                        <th style='padding-bottom:10px;'> ₹{GrossFee}</th>
                                    </tr>

                            </table>
                            </div>
                        </div>
                    </body>
                    </html>
                ");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return "error";
            }
        }
        public async Task<ClientInvoicePdfViewModel?> GetClientInvoicePDFDataModel(int InvoiceID, IDbTransaction? tran = null)
        {
            var res = new ClientInvoicePdfViewModel();
            try
            {
                res = await _dbContext.GetByQueryAsync<ClientInvoicePdfViewModel>($@"Select V.Name as ClientName,V.EmailAddress as ClientEmail,
                                                                                    V.Phone as ClientPhone,M.MediaID as ClientMediaID,ProfileImage as ClientProfileImage,
                                                                                    GSTNo as ClientGSTNo,PackageName,Capacity,MP.PackageID,PlanName,StartDate,EndDate,InvoiceID,
                                                                                    DisconnectionDate,IsNull(TaxPercentage,0)TaxPercentage,Tax,MP.TaxCategoryID,IsNull(CI.Discount,0)as Discount,MP.Fee,InvoiceDate,
                                                                                    AddressLine1 as ClientAddress1,AddressLine2 as ClientAddress2,AddressLine2 as ClientAddress3,CityName as ClientCity,MP.PackageDescription,
                                                                                    StateName as ClientState,CountryName as ClientCountry,Pincode as ClientPincode,InvoiceNo
                                                                                    From ClientInvoice CI
                                                                                    Left Join Client C on CI.ClientID=C.ClientID and C.IsDeleted=0
                                                                                    Left Join viEntity V on V.EntityID=C.EntityID
                                                                                    Left Join Entity E on E.EntityID=V.EntityID and E.IsDeleted=0
                                                                                    Left Join EntityAddress EA on EA.EntityID=E.EntityID and EA.IsDeleted=0
                                                                                    Left Join media m on M.MediaID=V.MediaID and M.IsDeleted=0
                                                                                    Left Join Country CO on CO.CountryID=E.CountryID and CO.IsDeleted=0
                                                                                    Left Join CountryState CS on CS.StateID=EA.StateID and CS.IsDeleted=0
                                                                                    Left Join CountryCity CC on CC.StateID=EA.CityID and CC.IsDeleted=0
                                                                                    Left Join MembershipPackage MP on MP.PackageID=C.PackageID and MP.IsDeleted=0
                                                                                    Left Join MembershipUserCapacity Mu on Mu.CapacityID=MP.CapacityID and Mu.IsDeleted=0
                                                                                    Left Join MembershipPlan MBP on MBP.PlanID=MP.PlanID and MBP.IsDeleted=0
                                                                                    Where CI.InvoiceID={InvoiceID} and CI.IsDeleted=0
                ", null, tran);

                res.companyDetails = await _dbContext.GetByQueryAsync<CompanyDetailsViewmodel>($@"Select V.Name as CompanyName,V.EmailAddress as CompanyEmail,
                                                                                                V.Phone as Companyphone,M.MediaID as CompanyMediaID,ProfileImage as CompanyProfileImage,GSTNo as CompanyGSTNo,
                                                                                                AddressLine1 as CompanyAddress1,AddressLine2 as CompanyAddress2,AddressLine3 as CompanyAddress3,CityName as CompanyCity,
                                                                                                StateName as CompanyState,CountryName as CompanyCountry,Pincode as CompanyPincode
                                                                                                From Client C 
                                                                                                Left Join viEntity V on V.EntityID=C.EntityID
                                                                                                Left Join Entity E on E.EntityID=V.EntityID and E.IsDeleted=0
                                                                                                Left Join EntityAddress EA on EA.EntityID=E.EntityID and EA.IsDeleted=0
                                                                                                Left Join media m on M.MediaID=V.MediaID and M.IsDeleted=0
                                                                                                Left Join Country CO on CO.CountryID=E.CountryID and CO.IsDeleted=0
                                                                                                Left Join CountryState CS on CS.StateID=EA.StateID and CS.IsDeleted=0
                                                                                                Left Join CountryCity CC on CC.StateID=EA.CityID and CC.IsDeleted=0
                                                                                                Where C.ClientID={PDV.ProgbizClientID} and C.Isdeleted=0
                ", null, tran);

                res.Features = await _dbContext.GetListByQueryAsync<MembershipFeature>($@"Select MF.FeatureID,FeatureName 
                                                                                    From MembershipFeature MF
                                                                                    Left Join MembershipPackageFeature MPF on MPF.FeatureID=MF.FeatureID and MPF.IsDeleted=0
                                                                                    Left Join MembershipPackage MP on MP.PackageID=MPF.PackageID and MP.IsDeleted=0
                                                                                    Where MP.PackageID={res.PackageID} and MF.IsDeleted=0
                ", null, tran);

                if (res.TaxCategoryID != null)
                {
                    res.Taxcategoryitem = await _dbContext.GetListByQueryAsync<TaxCategoryItem>($@"Select TaxCategoryItemID,TaxCategoryItemName,TC.Percentage
                                                                                            From MembershipPackage MP 
                                                                                            Left Join viTaxCategory V on V.TaxCategoryID=MP.TaxCategoryID
                                                                                            Left Join TaxCategoryItem TC on TC.TaxCategoryID=V.TaxCategoryID and TC.IsDeleted=0
                                                                                            Where (MP.TaxCategoryID is null or  MP.TaxCategoryID={res.TaxCategoryID}) and MP.PackageID={res.PackageID} and MP.IsDeleted=0
                    ", null, tran);
                }

                string features = "";
                foreach (var item in res.Features)
                {
                    features += item.FeatureName + ",";
                }
                features = features.TrimEnd(',');
                res.AvailableFeatures = features;

                return res ?? new();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion


        #region New Quotation Pdf


        public async Task<int> GenerateBasicQuotationPdf(int quotationID, int branchID, string fileName, IDbTransaction? tran = null)
        {
            string htmlCoverContent = "", htmlBasicContent = "", htmlAwardContent = "", htmlProductContent = "", htmlItemContent = "", htmlTermContent = "";
            int? mediaID = null;

            string folderName = "gallery/" + PDFType.Quotation.ToString().ToLower();
            if (!Directory.Exists(Path.Combine("wwwroot", folderName)))
            {
                Directory.CreateDirectory(Path.Combine("wwwroot", folderName));
            }
            string filePath = Path.Combine(_env.ContentRootPath, "wwwroot", folderName, $"{fileName}.pdf");
            string mediaFileName = $"/{folderName}/{fileName}.pdf";
            string cssfilePath = Path.Combine(_env.ContentRootPath, "wwwroot", "assets");
            cssfilePath += "/style.css";

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = DinkToPdf.PaperKind.A4Small,
                // Margins = new MarginSettings { Top = 10 },
                DocumentTitle = "Quotation Pdf Report",
                Out = filePath,
            };
            mediaID = await _dbContext.GetFieldsAsync<Quotation, int?>("MediaID", $"BranchID={branchID} and QuotationID={quotationID} and IsDeleted=0", null, tran);
            mediaID ??= 0;

            // Html cover content

            htmlCoverContent = await GetQuotationCoverPdfHtmlContentAndData(quotationID, branchID, tran);
            var objectCoverSettings = new ObjectSettings
            {
                HtmlContent = htmlCoverContent,
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = cssfilePath },
            };

            // Html basic content

            htmlBasicContent = await GetQuotationBasicPdfHtmlContentAndData(quotationID, branchID, tran);

            var objectBasicSettings = new ObjectSettings
            {
                HtmlContent = htmlBasicContent,
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = cssfilePath },
            };


            // Html award content

            htmlAwardContent = await GetQuotationAwardPdfHtmlContentAndData(quotationID, branchID, tran);

            var objectAwardSettings = new ObjectSettings
            {
                HtmlContent = htmlAwardContent,
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = cssfilePath },
            };


            // Html product content

            htmlProductContent = await GetQuotationProductPdfHtmlContentAndData(quotationID, branchID, tran);

            var objectProductSettings = new ObjectSettings
            {
                HtmlContent = htmlProductContent,
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = cssfilePath },
            };


            // Html item content


            htmlItemContent = await GetQuotationItemPdfHtmlContentAndData(quotationID, branchID, tran);

            var objectItemSettings = new ObjectSettings
            {
                HtmlContent = htmlItemContent,
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = cssfilePath },
            };


            // Html terms content

            htmlTermContent = await GetQuotationTermsPdfHtmlContentAndData(quotationID, branchID, tran);

            var objectTermSettings = new ObjectSettings
            {
                HtmlContent = htmlTermContent,
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = cssfilePath },
            };


            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectCoverSettings, objectBasicSettings, objectAwardSettings, objectProductSettings, objectItemSettings, objectTermSettings }
            };
            _converter.Convert(pdf);

            await Task.Delay(100);

            Media media = new Media
            {
                MediaID = mediaID,
                ContentType = "application/pdf",
                Extension = "pdf",
                FileName = mediaFileName
            };

            mediaID = await _dbContext.SaveAsync(media, tran);

            await _dbContext.ExecuteAsync($"Update Quotation set MediaID={mediaID} where QuotationID={quotationID} and BranchID={branchID} and IsDeleted=0", null, tran);

            return mediaID.Value;
        }


        public async Task<string> GetQuotationCoverPdfHtmlContentAndData(int quotationID, int branchID, IDbTransaction? tran = null)
        {
            string? DomainUrl = _config.GetValue<string>("ServerURL");
            var pdfHtmlContent = new StringBuilder();
            pdfHtmlContent.Append($@"
                <!DOCTYPE html>
                        <html>
                        <head>
                        <title>WABCOM</title>
                        <link rel='stylesheet' type='text/css' href='styles.css'>
                        <style>
                        body {{
                        font-family: Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                        }}
                        .container {{
                        width: 21cm;
                        height: 29.7cm;
                        page-break-after: always;
                        margin: 0 auto;
                        padding: 20px;
                        }}
                        .main{{
                        border: 2px solid #000;
                        padding: 30px 20px 10px 20px;
                        background-image: url('{DomainUrl}/assets/images/wabcom/logo-bg.png');
                        background-position: center;
                        background-repeat: no-repeat;
                        position: relative;
                        height: 28.7cm;
                        }}
                        h4{{
                        color: #000;
                        }}
                        .footer-img{{
                        display: flex;
                        justify-content: space-between;
                        }}
                        .header {{
                        display: flex;
                        justify-content: space-between;
                        margin-bottom: 20px;
                        }}
                        .header img {{
                        height: 50px;
                        margin: 0 10px;
                        }}
                        .content {{
                        line-height: 1.3;
                        text-align: justify;

                        }}
                        .content p{{
                        margin: 5px;
                        font-size: 15px;
                        }}
                        .content h2 {{
                        color: #000;
                        font-size: 18px;
                        text-transform: uppercase;
                        margin-bottom: 0;
                        font-weight: 600;
                        }}
                        h3{{
                        color: #000;
                        }}
                        .footer {{
                        position: absolute;
                        bottom: 10px;
                        text-align: center;
                        margin-top: 20px;
                        font-size: 12px;
                        color: #666;
                        left: 32%;
                        transform: translateX(-23%)
                        }}
                        .footer img {{
                        height: 50px;
                        margin: 0 5px;
                        }}
                        .style ul{{
                        padding: 0;
                        margin: 0;
                        }}
                        .style ul li{{
                        list-style: none;
                        line-height: 26px;
                        }}
                        table {{
                            border-collapse: collapse;
                            border-spacing: 0;
                        }}
                        td, th {{
    	                        border: 1px solid #000;
    	                        text-align: left;
    	                        padding: 8px;
                            }}
                        .second-table th{{
                            background-color: #03aad4;
                        }}
                        table td p{{
                            font-size: 14px !important;
                        }}
                        table li, li{{
                            font-size: 14px;
                        }}
                        table ul{{
                            padding: 0 27px;
                        margin:  0;
                        font-weight: 400;
                        }}
                        .custom-list {{
                            list-style-type: none; 
                            padding-left: 10px; 
                            font-weight: normal;
                        }}
                        .custom-list li::before {{
                            content: '\27A3'; 
                            color: #000000; 
                            margin-right: 5px; 
                        }}
                        .style-2 h4{{
                            margin-bottom: 0;
                        }}
                        .style-2 th, .style-2 td{{
                            font-size: 14px;
                            padding: 3px;
                        }}
                        </style>
                        </head>");
            pdfHtmlContent.Append($@"<body>
                        <div class='container'>
                        <div class='main'>
                        <div class='header'>
                            <img src='{DomainUrl}/assets/images/wabcom/logo.png' alt='WABCOM'>
                            <img src='{DomainUrl}/assets/images/wabcom/partner.png' alt='Gold Partner'>
                        </div>
                        <div class='content'>
                            <p>COVER Form.</p>
                        </div>
                        <div class='footer'>
                            <div class='footer-top'>
                            <h3><b>WAHAT AL BUSTAN COMPUTER TR. </b></h3>
                            <p>#201,Team Business Center, Al Samar Street, P.O.Box: 83274, Sharjah, UAE, Tel:- +971 6 5343120</p>
                            <p>mail to: <a href='mailto:info@wabcomdubai.com'>info@wabcomdubai.com</a>, Website: <a href='www.wabcom.ae'>www.wabcom.ae</a></p>
                            <div class='footer-img'>
                            <img src='{DomainUrl}/assets/images/wabcom/greythr.png' alt='GreyMatrix'>
                            <img src='{DomainUrl}/assets/images/wabcom/dummy-logo.png' alt='Tally' >
                            </div>
                            </div>
                        </div>
                        </div>
                        </div>
   
                        </body>
                        </html> ");


            return pdfHtmlContent.ToString();
        }

        public async Task<string> GetQuotationBasicPdfHtmlContentAndData(int quotationID, int branchID, IDbTransaction? tran = null)
        {
            string? DomainUrl = _config.GetValue<string>("ServerURL");
            var pdfHtmlContent = new StringBuilder();
            pdfHtmlContent.Append($@"
                <!DOCTYPE html>
                        <html>
                        <head>
                        <title>WABCOM</title>
                        <link rel='stylesheet' type='text/css' href='styles.css'>

                        <style>
                        body {{
                        font-family: Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                        }}

                        .container {{
                        width: 21cm;
                        height: 29.7cm;
                        page-break-after: always;
                        margin: 0 auto;
                        padding: 20px;
                        }}
                        .main{{
                        border: 2px solid #000;
                        padding: 30px 20px 10px 20px;
                        background-image: url('{DomainUrl}/assets/images/wabcom/logo-bg.png');
                        background-position: center;
                        background-repeat: no-repeat;
                        position: relative;
                        height: 28.7cm;

                        }}
                        h4{{
                        color: #000;
                        }}
                        .footer-img{{
                        display: flex;
                        justify-content: space-between;
                        }}


                        .header {{
                        display: flex;
                        justify-content: space-between;
                        margin-bottom: 20px;
                        }}

                        .header img {{
                        height: 50px;
                        margin: 0 10px;
                        }}

                        .content {{
                        line-height: 1.3;
                        text-align: justify;

                        }}
                        .content p{{
                        margin: 5px;
                        font-size: 15px;
                        }}

                        .content h2 {{
                        color: #000;
                        font-size: 18px;
                        text-transform: uppercase;
                        margin-bottom: 0;
                        font-weight: 600;
                        }}
                        h3{{
                        color: #000;
                        }}

                        .footer {{
                        position: absolute;
                        bottom: 10px;
                        text-align: center;
                        margin-top: 20px;
                        font-size: 12px;
                        color: #666;
                        left: 32%;
                        transform: translateX(-23%)
                        }}

                        .footer img {{
                        height: 50px;
                        margin: 0 5px;
                        }}
                        .style ul{{
                        padding: 0;
                        margin: 0;
                        }}
                        .style ul li{{
                        list-style: none;
                        line-height: 26px;
                        }}
                        table {{
                            border-collapse: collapse;
                            border-spacing: 0;
                        }}

                        td, th {{
    	                        border: 1px solid #000;
    	                        text-align: left;
    	                        padding: 8px;
                            }}
                        .second-table th{{
                            background-color: #03aad4;
                        }}
                        table td p{{
                            font-size: 14px !important;
                        }}
                        table li, li{{
                            font-size: 14px;
                        }}
                        table ul{{
                            padding: 0 27px;
                        margin:  0;
                        font-weight: 400;
                        }}
                        .custom-list {{
                            list-style-type: none; 
                            padding-left: 10px; 
                            font-weight: normal;
                        }}
                        .custom-list li::before {{
                            content: '\27A3'; 
                            color: #000000; 
                            margin-right: 5px; 
                        }}
                        .style-2 h4{{
                            margin-bottom: 0;
                        }}
                        .style-2 th, .style-2 td{{
                            font-size: 14px;
                            padding: 3px;
                        }}
                        </style>
                        </head>
                        <body>
                        <div class='container style'>
                        <div class='main'>
                        <div class='header'>
                            <img src='{DomainUrl}/assets/images/wabcom/logo.png' alt='WABCOM'>
                            <img src='{DomainUrl}/assets/images/wabcom/partner.png' alt='Gold Partner'>
                        </div>
                        <div class='content'>
                            <p>WABCOM is UAE headquartered business specialized in providing business automation, IT and IT 
                                enabled services to businesses operating across the world.</p>
                                <p>For over 10 years we are leading provider of software solutions, services and customization to the 
                                industries across UAE. We are specialized in ERP Implementation, Training, Support, Customization, Data 
                                Migration, Data integration and Import. Our strength resides in our understanding of the requirements 
                                and in our ability to develop process-based tools and applications, which are tailored to the specific 
                                needs of the high value and high-reliability that the industry demands. We deliver affordable and highquality services also we are conducting series of events to help our customers accelerate their business 
                                growth.
                                </p>
                            <h2>Our Team</h2>
                            <p>Our team consists of highly skilled industry professionals specialized in providing cost effective solutions 
                                to suit every business requirements. All our professionals are having depth knowledge and technical 
                                expertise in giving efficient, effective and satisfying services to our clients. We have complete in-house 
                                team of programmers & developers for all software’s that we are dealing in and dedicated support 
                                center is based in UAE for easy access and timely support for our customers. </p>
                            <h2>Clients</h2>
                            <p>WABCOM has a broad range of clients within the Middle East industry and is continuously seeking to 
                                extend our services into new areas with new clients. Our clients are from various industries like all kind 
                                of Trading, Manufacturing, Advertising, Hospitality, Automobiles, Real Estate, Construction, Property 
                                Management, Restaurants, Transportation, Educational institutions and Ecommerce. Through our 
                                clients, our portfolio of activities has diversified to include a range of applications supporting business 
                                process re-engineering and integrated information systems.</p>
                            <h2>Partnership</h2>
                            <p>In order to deliver cutting edge solutions and technologies, Wabcom has formed key Partnerships with 
                                other specialist service providers. We have been a leading part of the Tally Partner Network and as an 
                                Independent Software Vendors. We have partnership with Greytip Software for their cloud application 
                                greytHR, which is an HRMS solution that automates HR processes & empowers employee self-service 
                                and with Vtiger which is an all-in-one CRM that helps to achieve business goals faster.</p>
                            <h3>TallyPrime Software – A complete business management solution</h3>
                            <p>For almost 30 years, Tally has been helping customers to manage their business systems and information 
                                with reliable, secure, and integrated technologies. Tally Solutions was selected as the <b>“Editor’s Choice”</b>  
                                for Best Accounting and Inventory Management Software for the SMB Sector, in the Middle East. Tally 
                                Solutions has won the <b>‘Choice of Channel Award’</b> for the ‘Best Channel Program’ voted by the Channel 
                                in the Middle-East and conferred by the prestigious VAR Magazine for Middle East & Africa. Awarded as 
                                Best ERP solution in the year 2020.Tally is at the forefront of simplifying business management for 
                                business through automation. Maintain accounting, inventory, taxation, banking and much more with 
                                automation and greater flexibility which brings the best business management experience.TallyPrime is 
                                very simple to learn and easier to use with anywhere, anytime and secure access.
                                </p>
                        </div>
                        <div class='footer'>
                            <div class='footer-top'>
                            <h3><b>WAHAT AL BUSTAN COMPUTER TR. </b></h3>
                            <p>#201,Team Business Center, Al Samar Street, P.O.Box: 83274, Sharjah, UAE, Tel:- +971 6 5343120</p>
                            <p>mail to: <a href='mailto:info@wabcomdubai.com'>info@wabcomdubai.com</a>, Website: <a href='www.wabcom.ae'>www.wabcom.ae</a></p>
                            <div class='footer-img'>
                            <img src='{DomainUrl}/assets/images/wabcom/greythr.png' alt='GreyMatrix'>
                            <img src='{DomainUrl}/assets/images/wabcom/dummy-logo.png' alt='Tally'>
                            </div>
                            </div>
                        </div>
                        </div>
                        </div>
   
                        </body>
                        </html> ");


            return pdfHtmlContent.ToString();
        }

        public async Task<string> GetQuotationAwardPdfHtmlContentAndData(int quotationID, int branchID, IDbTransaction? tran = null)
        {
            string? DomainUrl = _config.GetValue<string>("ServerURL");
            var pdfHtmlContent = new StringBuilder();
            pdfHtmlContent.Append($@"
               <!DOCTYPE html>
                        <html>
                        <head>
                            <title>WABCOM</title>
                            <link rel='stylesheet' type='text/css' href='styles.css'>

                            <style>
                                body {{
                            font-family: Arial, sans-serif;
                            margin: 0;
                            padding: 0;
                        }}

                        .container {{
                            width: 21cm;
                            height: 29.7cm;
                            page-break-after: always;
                            margin: 0 auto;
                            padding: 20px;
                        }}
                        .main{{
                            border: 2px solid #000;
                            padding: 30px 20px 10px 20px;
                            background-image: url('{DomainUrl}/assets/images/wabcom/logo-bg.png');
                            background-position: center;
                            background-repeat: no-repeat;
                            position: relative;
                            height: 28.7cm;

                        }}
                        h4{{
                            color: #000;
                        }}
                        .footer-img{{
                            display: flex;
                            justify-content: space-between;
                        }}


                        .header {{
                            display: flex;
                            justify-content: space-between;
                            margin-bottom: 20px;
                        }}

                        .header img {{
                            height: 50px;
                            margin: 0 10px;
                        }}

                        .content {{
                            line-height: 1.3;
                            text-align: justify;

                        }}
                        .content p{{
                            margin: 5px;
                            font-size: 15px;
                        }}

                        .content h2 {{
                            color: #000;
                            font-size: 18px;
                            text-transform: uppercase;
                            margin-bottom: 0;
                            font-weight: 600;
                        }}
                        h3{{
                            color: #000;
                        }}

                        .footer {{
                            position: absolute;
                            bottom: 10px;
                            text-align: center;
                            margin-top: 20px;
                            font-size: 12px;
                            color: #666;
                            left: 32%;
                            transform: translateX(-23%)
                        }}

                        .footer img {{
                            height: 50px;
                            margin: 0 5px;
                        }}
                        .style ul{{
                            padding: 0;
                            margin: 0;
                        }}
                        .style ul li{{
                            list-style: none;
                            line-height: 26px;
                        }}
                        table {{
                                    border-collapse: collapse;
                                    border-spacing: 0;
                                }}

                        td, th {{
    		                            border: 1px solid #000;
    		                            text-align: left;
    		                            padding: 8px;
    		                        }}
                                .second-table th{{
                                    background-color: #03aad4;
                                }}
                                table td p{{
                                    font-size: 14px !important;
                                }}
                                table li, li{{
                                    font-size: 14px;
                                }}
                                table ul{{
                                    padding: 0 27px;
                            margin:  0;
                            font-weight: 400;
                                }}
                                .custom-list {{
                                    list-style-type: none; 
                                    padding-left: 10px; 
                                    font-weight: normal;
                                }}
                                .custom-list li::before {{
                                    content: '\27A3'; 
                                    color: #000000; 
                                    margin-right: 5px; 
                                }}
                                .style-2 h4{{
                                    margin-bottom: 0;
                                }}
                                .style-2 th, .style-2 td{{
                                    font-size: 14px;
                                    padding: 3px;
                                }}
                            </style>
                        </head>
                        <body>
    
                            <div class='container style'>
                                <div class='main'>
                                <div class='header'>
                                    <img src='{DomainUrl}/assets/images/wabcom/logo.png' alt='WABCOM'>
                                    <img src='{DomainUrl}/assets/images/wabcom/partner.png' alt='Gold Partner'>
                                </div>
                                <div class='content'>
                                    <p>Wabcom is using several products from the Tally from their database to the middleware (application 
                                        server) to some specific applications to provide the underlying structures to its applications.We have 
                                        developed and delivered many vertical solutions on top of Tally which includes retail trading, 
                                        manufacturing process, service industry, insurance brokers, supply chain management, transportation 
                                        management etc. We have successfully implemented many integration projects with other software and 
                                        applications.</p>
              
                                    <h3>greytHR- Essential HR Business Tool to Survive and Thrive</h3>
                                    <p>Greytip Software a leading HR & Payroll Software Specialist in India & Middle East, incorporated in 1994; 
                                        for their flagship cloud application *greytHR*.Greytip is an award winning and ISO 27001:2013 certified 
                                        company, serving 15,000+ clients (30+ fortune 500 Companies) across India and MENA Region and 
                                        processing 1.5 Million+ pay slips every month.</p>
                                    <p>greytHR has been awarded as BEST BRAND AWARD 2021 from the Economics Times and 
                                        BEST HRMS VENDOR AND BEST PAYROLL SOFTWARE. </p>
                                    <p>The software is fully compatible with all the GCC countries and covers all the necessary Statutory 
                                        Compliances. Many clients are enrolled with us for HRMS software and are maintaining Centralized 
                                        system to enhance employee relationship. greytHR Support for great HRs.To simplify every HR 
                                        operation.</p>
                                    <p>greytHR software automates all the HR & payroll activities and have a centralized system, which will 
                                        enhance employee relationship and helps to get all-around productivity. The productivity and efficiency 
                                        of the business can be improved through the automation of manual and repetitive tasks, the software 
                                        has different modules such as Core HR, Payroll & Compliance, Leave, Attendance, ESS Portal & Mobile, 
                                        Onboarding, Exits & Checklist.</p>
                                        <p><b>Awards & Recognitions</b>-WABCOM is Tally <b>GOLD PARTNER </b>and has been awarded as:</p>
                                        <ul>
                                        <li>THE BEST TALLY PARTNER-FY 2017-18</li>
                                        <li>THE LEADING TALLY PARTNER-FY 2018-19</li>
                                        <li>LORD OF THE SALES-FY 2018-19</li>
                                        <li>RETENTION KING-FY 2018-19</li>
                                        <li>EXCELLENTIA LMS LEADER-FY 2021-22
                                        </li>
                                        <li>EXCELLENTIA PATRON-FY 2021-2022</li>
                                        <li>EXCELLENTIA LEGENDS-FY 2021-2022</li>
                                        <li>EXCELLENTIA CONQUERER-FY 2021-2022 </li>
                                        <li>GROWTH DRIVER CHAMPIONS-FY 2022</li>
                                        <li>SERVER OF SERVERS- FY 2022-2023
                                        </li>
                                        <li>ELITE BUSINESS ACHIEVER-FY 2022-2023</li>
                                        <li>THE REFERRAL ROCKSTAR-FY 2022-2023</li>
                                        </ul>
                                        <p>We have more than 5000 clients and we are proud to be associated with businesses and as the result of 
                                        maintaining a good relationship with our clients, most of our business comes through our existing 
                                        customers.
                                        </p>
                                </div>
                                <div class='footer'>
                                    <div class='footer-top'>
                                    <h3><b>WAHAT AL BUSTAN COMPUTER TR. </b></h3>
                                    <p>#201,Team Business Center, Al Samar Street, P.O.Box: 83274, Sharjah, UAE, Tel:- +971 6 5343120</p>
                                    <p>mail to: <a href='mailto:info@wabcomdubai.com'>info@wabcomdubai.com</a>, Website: <a href='www.wabcom.ae'>www.wabcom.ae</a></p>
                                    <div class='footer-img'>
                                    <img src='{DomainUrl}/assets/images/wabcom/greythr.png' alt='GreyMatrix'>
                                    <img src='{DomainUrl}/assets/images/wabcom/dummy-logo.png' alt='Tally'>
                                    </div>
                                    </div>
                                </div>
                                </div>
                            </div>
   
                        </body>
                        </html> ");


            return pdfHtmlContent.ToString();
        }

        public async Task<string> GetQuotationProductPdfHtmlContentAndData(int quotationID, int branchID, IDbTransaction? tran = null)
        {
            string? DomainUrl = _config.GetValue<string>("ServerURL");
            var pdfHtmlContent = new StringBuilder();
            pdfHtmlContent.Append($@"
                    <!DOCTYPE html>
                            <html>
                            <head>
                            <title>WABCOM</title>
                            <link rel='stylesheet' type='text/css' href='styles.css'>

                            <style>
                            body {{
                            font-family: Arial, sans-serif;
                            margin: 0;
                            padding: 0;
                            }}

                            .container {{
                            width: 21cm;
                            height: 29.7cm;
                            page-break-after: always;
                            margin: 0 auto;
                            padding: 20px;
                            }}
                            .main{{
                            border: 2px solid #000;
                            padding: 30px 20px 10px 20px;
                            background-image: url('{DomainUrl}/assets/images/wabcom/logo-bg.png');
                            background-position: center;
                            background-repeat: no-repeat;
                            position: relative;
                            height: 28.7cm;

                            }}
                            h4{{
                            color: #000;
                            }}
                            .footer-img{{
                            display: flex;
                            justify-content: space-between;
                            }}


                            .header {{
                            display: flex;
                            justify-content: space-between;
                            margin-bottom: 20px;
                            }}

                            .header img {{
                            height: 50px;
                            margin: 0 10px;
                            }}

                            .content {{
                            line-height: 1.3;
                            text-align: justify;

                            }}
                            .content p{{
                            margin: 5px;
                            font-size: 15px;
                            }}

                            .content h2 {{
                            color: #000;
                            font-size: 18px;
                            text-transform: uppercase;
                            margin-bottom: 0;
                            font-weight: 600;
                            }}
                            h3{{
                            color: #000;
                            }}

                            .footer {{
                            position: absolute;
                            bottom: 10px;
                            text-align: center;
                            margin-top: 20px;
                            font-size: 12px;
                            color: #666;
                            left: 32%;
                            transform: translateX(-23%)
                            }}

                            .footer img {{
                            height: 50px;
                            margin: 0 5px;
                            }}
                            .style ul{{
                            padding: 0;
                            margin: 0;
                            }}
                            .style ul li{{
                            list-style: none;
                            line-height: 26px;
                            }}
                            table {{
                                border-collapse: collapse;
                                border-spacing: 0;
                            }}

                            td, th {{
    	                            border: 1px solid #000;
    	                            text-align: left;
    	                            padding: 8px;
                                }}
                            .second-table th{{
                                background-color: #03aad4;
                            }}
                            table td p{{
                                font-size: 14px !important;
                            }}
                            table li, li{{
                                font-size: 14px;
                            }}
                            table ul{{
                                padding: 0 27px;
                            margin:  0;
                            font-weight: 400;
                            }}
                            .custom-list {{
                                list-style-type: none; 
                                padding-left: 10px; 
                                font-weight: normal;
                            }}
                            .custom-list li::before {{
                                content: '\27A3'; 
                                color: #000000; 
                                margin-right: 5px; 
                            }}
                            .style-2 h4{{
                                margin-bottom: 0;
                            }}
                            .style-2 th, .style-2 td{{
                                font-size: 14px;
                                padding: 3px;
                            }}
                            </style>
                            </head>
                            <body>
                            <div class='container style'>
                            <div class='main'>
                            <div class='header'>
                                <img src='{DomainUrl}/assets/images/wabcom/logo.png' alt='WABCOM'>
                                <img src='{DomainUrl}/assets/images/wabcom/partner.png' alt='Gold Partner'>
                            </div>
                            <div class='content'>
                                <p>Product Description.</p>
                            </div>
                            <div class='footer'>
                                <div class='footer-top'>
                                <h3><b>WAHAT AL BUSTAN COMPUTER TR. </b></h3>
                                <p>#201,Team Business Center, Al Samar Street, P.O.Box: 83274, Sharjah, UAE, Tel:- +971 6 5343120</p>
                                <p>mail to: <a href='mailto:info@wabcomdubai.com'>info@wabcomdubai.com</a>, Website: <a href='www.wabcom.ae'>www.wabcom.ae</a></p>
                                <div class='footer-img'>
                                <img src='{DomainUrl}/assets/images/wabcom/greythr.png' alt='GreyMatrix'>
                                <img src='{DomainUrl}/assets/images/wabcom/dummy-logo.png' alt='Tally'>
                                </div>
                                </div>
                            </div>
                            </div>
                            </div>
   
                            </body>
                            </html> ");


            return pdfHtmlContent.ToString();
        }


        public async Task<string> GetQuotationItemPdfHtmlContentAndData(int quotationID, int branchID, IDbTransaction? tran = null)
        {
            string dateFormat = "dd/MM/yyyy";
            var quotation = await GetQuotationPdfDetailsModel(quotationID, branchID, tran);

            decimal totalAmount = quotation.Items.Sum(i => i.TotalAmount);
            var pdfHtmlContent = new StringBuilder();
            pdfHtmlContent.Append($@"
                   <!DOCTYPE html>
                    <html>
                    <head>
                        <title>WABCOM</title>
                        <link rel='stylesheet' type='text/css' href='styles.css'>");

            pdfHtmlContent.Append($@" <style>
                            body {{
                        font-family: Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                    }}

                    .container {{
                        width: 21cm;
                        height: 29.7cm;
                        page-break-after: always;
                        margin: 0 auto;
                        padding: 20px;
                    }}
                    .main{{
                        border: 2px solid #000;
                        padding: 30px 20px 10px 20px;
                        background-image: url('{quotation.DomainUrl}/assets/images/wabcom/logo-bg.png');
                        background-position: center;
                        background-repeat: no-repeat;
                        position: relative;
                        height: 28.7cm;

                    }}
                    h4{{
                        color: #000;
                    }}
                    .footer-img{{
                        display: flex;
                        justify-content: space-between;
                    }}


                    .header {{
                        display: flex;
                        justify-content: space-between;
                        margin-bottom: 20px;
                    }}

                    .header img {{
                        height: 50px;
                        margin: 0 10px;
                    }}

                    .content {{
                        line-height: 1.3;
                        text-align: justify;

                    }}
                    .content p{{
                        margin: 5px;
                        font-size: 15px;
                    }}

                    .content h2 {{
                        color: #000;
                        font-size: 18px;
                        text-transform: uppercase;
                        margin-bottom: 0;
                        font-weight: 600;
                    }}
                    h3{{
                        color: #000;
                    }}

                    .footer {{
                        position: absolute;
                        bottom: 10px;
                        text-align: center;
                        margin-top: 20px;
                        font-size: 12px;
                        color: #666;
                        left: 32%;
                        transform: translateX(-23%)
                    }}

                    .footer img {{
                        height: 50px;
                        margin: 0 5px;
                    }}
                    .style ul{{
                        padding: 0;
                        margin: 0;
                    }}
                    .style ul li{{
                        list-style: none;
                        line-height: 26px;
                    }}
                    table {{
                                border-collapse: collapse;
                                border-spacing: 0;
                            }}

                    td, th {{
    		                        border: 1px solid #000;
    		                        text-align: left;
    		                        padding: 8px;
    		                    }}
                            .second-table th{{
                                background-color: #03aad4;
                            }}
                            table td p{{
                                font-size: 14px !important;
                            }}
                            table li, li{{
                                font-size: 14px;
                            }}
                            table ul{{
                                padding: 0 27px;
                        margin:  0;
                        font-weight: 400;
                            }}
                            .custom-list {{
                                list-style-type: none; 
                                padding-left: 10px; 
                                font-weight: normal;
                            }}
                            .custom-list li::before {{
                                content: '\27A3'; 
                                color: #000000; 
                                margin-right: 5px; 
                            }}
                            .style-2 h4{{
                                margin-bottom: 0;
                            }}
                            .style-2 th, .style-2 td{{
                                font-size: 14px;
                                padding: 3px;
                            }}
                        </style>
                    </head>");
            pdfHtmlContent.Append($@" <body>
    
                        <div class='container style-2'>
                            <div class='main'>
                            <div class='header'>
                                <img src='{quotation.DomainUrl}/assets/images/wabcom/logo.png' alt='WABCOM'>
                                <img src='{quotation.DomainUrl}/assets/images/wabcom/partner.png' alt='Gold Partner'>
                            </div>
                            <div class='content'>
            
              
                                <h4>QUOTATION: </h4>
                                <table style='width:100%' >
                                    <tr>
                                    <th>Description </th>
                                    <td>{quotation.Description}</td>
                                    <th>Date </th>
                                    <td>{(quotation.Date.HasValue ? quotation.Date.Value.ToString(dateFormat) : string.Empty)}</td>
                                    </tr>
                                    <tr>
                                    <th>Type </th>
                                    <td>{quotation.BusinessTypeName}</td>
                                    <th>Our Ref</th>
                                    <td> WQ/{quotation.QuotationNo}/{(quotation.Date.HasValue ? quotation.Date.Value.ToString("yyyy") : string.Empty)}</td>
                                    </tr>
                                    <tr>
                                    <th>Party</th>
                                    <td colspan=""3"">{quotation.CustomerName}</td>
                
                                    </tr>
                                </table>
                                <h4>COMMERCIALS:</h4>");

            pdfHtmlContent.Append(@" <table style='width:100%' class='second-table'>
                                    <tr>
                                    <th>Particulars </th>
                                    <th>Rate </th>
                                    <th>Qty </th>
                                    <th>Amount(AED) </th>
                                    </tr>");
            if(quotation.Items.Count>0)
            {
                foreach (var item in quotation.Items)
                {
                    pdfHtmlContent.Append($@"  <tr>
                                    <td>
                                        <p><b>{item.ItemName}</p>  <ul>");
                    foreach (var description in item.ItemDescriptionList)
                    {
                        pdfHtmlContent.Append($@"
                                        <li>{description}</li>
                                    ");
                    }
                    pdfHtmlContent.Append($@"</ul> </td>
                                    <td><b>{item.Rate}</b></td>
                                    <td><b>{item.Quantity}</b></td>
                                    <td><b>{item.TotalAmount}</b></td>
                
                                    </tr>");
                }
            }
            var allTaxCategoryItems = quotation.Items.SelectMany(item => item.TaxCategoryItems).ToList();
                    var groupedTaxCategoryItems = allTaxCategoryItems
                    .GroupBy(item => item.TaxCategoryItemID)
                    .Select(group => new QuotationItemTaxCategoryItemsModel
                    {
                        TaxCategoryItemID = group.Key,
                        TaxCategoryItemName = group.First().TaxCategoryItemName,
                        Percentage = group.First().Percentage,
                        Amount = group.Sum(item => item.Amount)
                    }).ToList();

                    pdfHtmlContent.Append($@" <tr>
                                    <td colspan='3' style='text-align: right;'><b>SUB TOTAL</b></td>
                                    <td><b>{quotation.Items.Sum(item => item.TotalAmount)}</b></td>
                                    </tr>");
                    foreach (var tax in groupedTaxCategoryItems)
                    {

                        pdfHtmlContent.Append($@"<tr>
                                    <td colspan='3' style='text-align: right;'><b>{tax.TaxCategoryItemName}</b></td>
                                    <td><b>{tax.Amount}</b></td>
                                    </tr>");

                    }
                    pdfHtmlContent.Append($@"<tr>
                                    <td colspan='3' style='text-align: right;'><b>TOTAL</b></td>
                                    <td><b>{quotation.CurrencySymbol} {(quotation.Items.Sum(item => item.GrossAmount))}</b></td>
                                    </tr>");
                
           
            pdfHtmlContent.Append(@"</table>
                                    <h4>Our Bank Details: </h4>
                                <table style='width: 100%;'>
                                    <tr>
                                    <th>Account Name </th>
                                    <td>WAHAT AL BUSTAN COMPUTER TR.</td>
                                    </tr>
                                    <tr>
                                    <th>Bank Name </th>
                                    <td>RAK Bank</td>
                                    </tr>
                                    <tr>
                                    <th>Account No. </th>
                                    <td>8362389685901</td>
                                    </tr>
                                    <tr>
                                    <th>IBAN No. </th>
                                    <td>AE630400008362389685901</td>
                                    </tr>
                                    <tr>
                                    <th>SWIFT CODE  </th>
                                    <td>NRAKAEAK</td>
                                    </tr>
                                </table>
                                <p>Make all cheques payable to <b>WAHAT AL BUSTAN COMPUTER TR</b> </p> <h4>Features of Tally Software Subscription</h4>
                                <ul>");
            if(quotation.CustomerNoteList.Count>0)
            {
                foreach(var notes in quotation.CustomerNoteList)
                {
                    pdfHtmlContent.Append($@" 
                                    <li>{notes}</li>
                                ");
                }
            }

            pdfHtmlContent.Append($@"</ul> </div>
                            <div class='footer'>
                                <div class='footer-top'>
                                <h3><b>WAHAT AL BUSTAN COMPUTER TR. </b></h3>
                                <p>#201,Team Business Center, Al Samar Street, P.O.Box: 83274, Sharjah, UAE, Tel:- +971 6 5343120</p>
                                <p>mail to: <a href='mailto:info@wabcomdubai.com'>info@wabcomdubai.com</a>, Website: <a href='www.wabcom.ae'>www.wabcom.ae</a></p>
                                <div class='footer-img'>
                                <img src='{quotation.DomainUrl}/assets/images/wabcom/greythr.png' alt='GreyMatrix'>
                                <img src='{quotation.DomainUrl}/assets/images/wabcom/dummy-logo.png' alt='Tally'>
                                </div>
                                </div>
                            </div>
                            </div>
                        </div>
    
                    </body>
                    </html>");


            return pdfHtmlContent.ToString();
        }


        public async Task<string> GetQuotationTermsPdfHtmlContentAndData(int quotationID, int branchID, IDbTransaction? tran = null)
        {

            var quotation = await GetQuotationPdfDetailsModel(quotationID, branchID, tran);
            var pdfHtmlContent = new StringBuilder();
            pdfHtmlContent.Append($@"
                   <!DOCTYPE html>
                            <html>
                            <head>
                                <title>WABCOM</title>
                                <link rel='stylesheet' type='text/css' href='styles.css'>

                                <style>
                                  body {{
                                font-family: Arial, sans-serif;
                                margin: 0;
                                padding: 0;
                            }}

                            .container {{
                              width: 21cm;
                              height: 29.7cm;
                              page-break-after: always;
                              margin: 0 auto;
                              padding: 20px;
                            }}
                            .main{{
                              border: 2px solid #000;
                              padding: 30px 20px 10px 20px;
                              background-image: url('{quotation.DomainUrl}/assets/images/wabcom/logo-bg.png');
                              background-position: center;
                              background-repeat: no-repeat;
                              position: relative;
                              height: 28.7cm;

                            }}
                            h4{{
                              color: #000;
                            }}
                            .footer-img{{
                              display: flex;
                              justify-content: space-between;
                            }}


                            .header {{
                                display: flex;
                                justify-content: space-between;
                                margin-bottom: 20px;
                            }}

                            .header img {{
                                height: 50px;
                                margin: 0 10px;
                            }}

                            .content {{
                                line-height: 1.3;
                                text-align: justify;

                            }}
                            .content p{{
                              margin: 5px;
                              font-size: 15px;
                            }}

                            .content h2 {{
                                color: #000;
                                font-size: 18px;
                                text-transform: uppercase;
                                margin-bottom: 0;
                                font-weight: 600;
                            }}
                            h3{{
                              color: #000;
                            }}

                            .footer {{
                              position: absolute;
                              bottom: 10px;
                                text-align: center;
                                margin-top: 20px;
                                font-size: 12px;
                                color: #666;
                                left: 32%;
                                transform: translateX(-23%)
                            }}

                            .footer img {{
                                height: 50px;
                                margin: 0 5px;
                            }}
                            .style ul{{
                              padding: 0;
                              margin: 0;
                            }}
                            .style ul li{{
                              list-style: none;
                              line-height: 26px;
                            }}
                            table {{
                                        border-collapse: collapse;
                                        border-spacing: 0;
                                    }}

                            td, th {{
    		                                border: 1px solid #000;
    		                                text-align: left;
    		                                padding: 8px;
    		                            }}
                                    .second-table th{{
                                      background-color: #03aad4;
                                    }}
                                    table td p{{
                                      font-size: 14px !important;
                                    }}
                                    table li, li{{
                                      font-size: 14px;
                                    }}
                                    table ul{{
                                      padding: 0 27px;
                                margin:  0;
                                font-weight: 400;
                                    }}
                                    .custom-list {{
                                        list-style-type: none; 
                                        padding-left: 10px; 
                                        font-weight: normal;
                                    }}
                                    .custom-list li::before {{
                                        content: '\27A3'; 
                                        color: #000000; 
                                        margin-right: 5px; 
                                    }}
                                    .style-2 h4{{
                                      margin-bottom: 0;
                                    }}
                                    .style-2 th, .style-2 td{{
                                      font-size: 14px;
                                      padding: 3px;
                                    }}
                                </style>
                            </head>
                            <body>
   
                                <div class='container style'>
                                  <div class='main'>
                                    <div class='header'>
                                        <img src='{quotation.DomainUrl}/assets/images/wabcom/logo.png' alt='WABCOM'>
                                        <img src='{quotation.DomainUrl}/assets/images/wabcom/partner.png' alt='Gold Partner'>
                                    </div>
                                    <div class='content'>
           
                                       <h4>Terms and Conditions: </h4>  <ul class='custom-list'>");
            if(quotation.TermsList.Count>0)
            {
                foreach(var terms in quotation.TermsList)
                {
                    pdfHtmlContent.Append($@"
                                        <li>{terms}</li>");
                }
            }

            pdfHtmlContent.Append($@"</ul>
            
                                        <h4 style=""text-align: center;"">We thank you in advance and please call us if you have any questions. </h4>
                                        <p>Regards,
                                        </p>
                                        <p>{quotation.StaffName}
                                        </p>
                                        <p>Mobile No- {quotation.StaffPhoneNo}</p>
                                        <p>Phone No- +971 6 5343120</p>
                                        <p>Website- <a href='www.wabcom.ae'>www.wabcom.ae</a></p>
             
             
                                    </div>
                                    <div class='footer'>
                                      <div class='footer-top'>
                                        <h3><b>WAHAT AL BUSTAN COMPUTER TR. </b></h3>
                                        <p>#201,Team Business Center, Al Samar Street, P.O.Box: 83274, Sharjah, UAE, Tel:- +971 6 5343120</p>
                                        <p>mail to: <a href='mailto:info@wabcomdubai.com'>info@wabcomdubai.com</a>, Website: <a href='www.wabcom.ae'>www.wabcom.ae</a></p>
                                        <div class='footer-img'>
                                        <img src='{quotation.DomainUrl}/assets/images/wabcom/greythr.png' alt='GreyMatrix'>
                                        <img src='{quotation.DomainUrl}/assets/images/wabcom/dummy-logo.png' alt='Tally'>
                                      </div>
                                      </div>
                                    </div>
                                  </div>
                                </div>
                            </body>
                            </html>");


            return pdfHtmlContent.ToString();
        }
        #endregion

        public async Task<string> GenerateAllQuotationContent(int quotationID, int branchID, IDbTransaction? tran = null)
        {
            string string1 = await GetQuotationCoverPdfHtmlContentAndData(quotationID, branchID, tran);
            string string2 = await GetQuotationBasicPdfHtmlContentAndData(quotationID, branchID, tran);
            string string3 = await GetQuotationAwardPdfHtmlContentAndData(quotationID, branchID, tran);
            string string4 = await GetQuotationProductPdfHtmlContentAndData(quotationID, branchID, tran);
            string string5 = await GetQuotationItemPdfHtmlContentAndData(quotationID, branchID, tran);
            string string6 = await GetQuotationTermsPdfHtmlContentAndData(quotationID, branchID, tran);
            string mergedString = string1 + string2 + string3 + string4 + string5 + string6;

            return mergedString;
        }


    }
}
