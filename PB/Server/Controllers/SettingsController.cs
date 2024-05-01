using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PB.Shared.Models;
using PB.DatabaseFramework;
using PB.Model;
using PB.Server.Repository;
using PB.Shared;
using PB.Shared.Enum;
using PB.Shared.Helpers;
using PB.Shared.Models.Common;
using PB.Shared.Tables;
using PB.Shared.Tables.Accounts.Ledgers;
using System.Data;
using PB.Model.Models;
using PB.Shared.Tables.CRM;
using PB.EntityFramework;
using PB.Shared.Views;
using PB.Shared.Models.SuperAdmin.Client;
using PB.Shared.Tables.Tax;
using PB.Shared.Enum.Accounts;
using PB.Shared.Tables.Common;
using PB.Shared.Models.Inventory.Items;
using PB.Shared.Models.CRM.Quotations;

namespace PB.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SettingsController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection _cn;
        private readonly IMapper _mapper;
        private readonly ICRMRepository _crm;
        private readonly ICommonRepository _common;

        public SettingsController(IDbContext dbContext, IDbConnection cn, IMapper mapper, ICRMRepository crm, ICommonRepository common)
        {
            _dbContext = dbContext;
            _cn = cn;
            _mapper = mapper;
            _crm = crm;
            _common = common;
        }

        #region Lead Through

        [HttpPost("save-lead-through")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SaveLead(LeadThroughModel model)
        {
            var Count = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) 
                                                                    from LeadThrough
                                                                    where LOWER(TRIM(Name))=LOWER(TRIM(@Name)) and LeadThroughID<>@LeadThroughID and IsDeleted=0 and (ClientID IS NULL OR ClientID={CurrentClientID})", model);
            if (Count != 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "LeadThrough already exist",
                    ResponseMessage = "The Lead Trough already exist,try different one"
                });
            }
            try
            {
                model.ClientID = CurrentClientID;
                var lead = _mapper.Map<LeadThrough>(model);
                lead.LeadThroughID = await _dbContext.SaveAsync(lead);
                LeadThroughAddResultModel returnObject = new() { LeadThroughID = lead.LeadThroughID, LeadThroughName = lead.Name };
                return Ok(returnObject);
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

        [HttpPost("get-lead-through-paged-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetLeadsList(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;

            query.Select = $@"Select LeadThroughID,Name,ClientID
								From LeadThrough";

            query.WhereCondition = $" IsDeleted=0 and (ClientID IS NULL OR ClientID={CurrentClientID})";
            query.OrderByFieldName = model.OrderByFieldName;
            if (!model.SearchString.IsNullOrEmpty())
            {
                query.WhereCondition += $" and Name like '%{model.SearchString}%'";
            }
            var res = await _dbContext.GetPagedList<LeadThroughModel>(query, null);
            return Ok(res);
        }

        [HttpGet("get-lead-through/{leadThroughID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetLead(int leadThroughID)
        {
            var res = await _dbContext.GetByQueryAsync<LeadThroughModel>($@"Select *
                                                                                From LeadThrough
                                                                                Where LeadThroughID={leadThroughID} and IsDeleted=0", null);
            return Ok(res ?? new());
        }

        [HttpGet("delete-lead-through")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteLead(int Id)
        {
            int enquiryCount = await _dbContext.GetByQueryAsync<int>($@"SELECT Count(*) AS DataCount 
                                                                           from  Enquiry 
                                                                            WHERE IsDeleted =0 and LeadThroughID={Id}
                                                                            GROUP BY LeadThroughID", null);

            if (enquiryCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid operation",
                    ResponseMessage = "Enquiry exist  with the LeadThrough you are trying to remove"
                });
            }

            await _dbContext.DeleteAsync<LeadThrough>(Id);
            return Ok(true);
        }

        #endregion

        #region Followup Status

        [HttpGet("delete-followup-status")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteFollowupStatus(int Id)
        {
            int enquiryCount = await _dbContext.GetByQueryAsync<int>($@"SELECT Count(*) AS DataCount 
                                                                           from FollowUp F
                                                                            LEFT JOIN Enquiry E ON E.EnquiryID=F.EnquiryID
                                                                            WHERE E.IsDeleted =0 and F.IsDeleted=0 and FollowUpStatusID={Id}
                                                                            GROUP BY FollowUpStatusID", null);

            if (enquiryCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid operation",
                    ResponseMessage = "Enquiry exist  with the FollowupStatus you are trying to remove"
                });
            }

            int quotationCount = await _dbContext.GetByQueryAsync<int>($@"SELECT Count(*) AS DataCount 
                                                                           from FollowUp F
                                                                            LEFT JOIN Quotation E ON E.QuotationID=F.QuotationID
                                                                            WHERE E.IsDeleted =0 and F.IsDeleted=0 and FollowUpStatusID={Id}
                                                                            GROUP BY FollowUpStatusID", null);

            if (quotationCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid operation",
                    ResponseMessage = "Quotation entry exist with the FollowupStatus you are trying to remove"
                });
            }
            await _dbContext.DeleteAsync<FollowupStatus>(Id);
            return Ok(true);
        }

        #endregion

        #region Quotation And Invoice Preferences

        [HttpPost("save-setting")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SaveQuotationInvoiceSetting(ClientSettingModel model)
        {
            model.ClientID = CurrentClientID;
            var feature = _mapper.Map<ClientSetting>(model);
            await _dbContext.SaveAsync(feature);
            return Ok(new Success());

        }

        [HttpGet("get-quotation-default-settings")]
        public async Task<IActionResult> GetQuotationDefaultSettings()
        {
            var quotationSettings = await _dbContext.GetByQueryAsync<QuotationInvoiceDefaultSettingsModel>(@$"
																				Select B.BranchID,CR.CurrencyID,Concat(CR.CurrencyName,' ( ',CR.Symbol,' )') AS CurrencyName,CS.QuotationSubject As Subject,CS.QuotationCustomerNote As CustomerNote,CS.QuotationTermsAndCondition As TermsAndCondition,CS.QuotationNeedShippingAddress As NeedShippingAddress,C.CountryID,C.CountryName,C.ISDCode,B.StateID As PlaceOfSupplyID,CST.StateName As PlaceOfSupplyName
                                                                                From viBranch B
                                                                                Left Join CountryZone Z ON Z.ZoneID=B.ZoneID AND Z.IsDeleted=0
                                                                                Left Join Country C ON C.CountryID=Z.CountryID AND C.IsDeleted=0
                                                                                Left Join Currency CR ON CR.CurrencyID=C.CurrencyID AND CR.IsDeleted=0
                                                                                Left Join ClientSetting CS ON CS.ClientID=B.ClientID AND CS.IsDeleted=0
                                                                                Left Join CountryState CST ON CST.StateID=B.StateID AND CST.IsDeleted=0
                                                                                Where B.BranchID={CurrentBranchID} AND B.ClientID={CurrentClientID}                                                       
            ", null);
            return Ok(quotationSettings ?? new());
        }

        [HttpGet("get-invoice-default-settings")]
        public async Task<IActionResult> GetInvoiceDefaultSettings()
        {
            var invoiceSettings = await _dbContext.GetByQueryAsync<QuotationInvoiceDefaultSettingsModel>(@$"
																				Select B.BranchID,CR.CurrencyID,Concat(CR.CurrencyName,' ( ',CR.Symbol,' )') AS CurrencyName,CS.InvoiceSubject As Subject,CS.InvoiceCustomerNote As CustomerNote,CS.InvoiceTermsAndCondition As TermsAndCondition,CS.InvoiceNeedShippingAddress As NeedShippingAddress,C.CountryID,C.CountryName,C.ISDCode,B.StateID As PlaceOfSupplyID,CST.StateName As PlaceOfSupplyName
																				From viBranch B
                                                                                Left Join CountryZone Z ON Z.ZoneID=B.ZoneID AND Z.IsDeleted=0
                                                                                Left Join Country C ON C.CountryID=Z.CountryID AND C.IsDeleted=0
                                                                                Left Join Currency CR ON CR.CurrencyID=C.CurrencyID AND CR.IsDeleted=0
																				Left Join ClientSetting CS ON CS.ClientID={CurrentClientID}
                                                                                Left Join CountryState CST ON CST.StateID=B.StateID AND CST.IsDeleted=0
                                                                                Where B.BranchID={CurrentBranchID} AND B.ClientID={CurrentClientID}", null);
            return Ok(invoiceSettings ?? new());
        }

        [HttpGet("get-invoice-setting")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetInvoiceSetting()
        {
            var res = await _dbContext.GetByQueryAsync<ClientSetting>($@"Select *
                                                                                From ClientSetting
                                                                                Where ClientID={CurrentClientID} and IsDeleted=0 and Type={(int)ClientSettingsType.Invoice}", null);
            return Ok(res ?? new());
        }

        #endregion

        #region Tax Categories

        [HttpPost("save-tax-category")]
        public async Task<IActionResult> SaveTaxCategory(TaxCategoryModel model)
        {

            var taxCategory = await _dbContext.GetFieldsAsync<TaxCategory, string>(@$"TaxCategoryName", $"LOWER(TRIM(TaxCategoryName))=LOWER(TRIM(@TaxCategoryName)) and TaxCategoryID<>@TaxCategoryID and ClientID={CurrentClientID} and IsDeleted=0", new { TaxCategoryName = model.TaxCategoryName, TaxCategoryID = model.TaxCategoryID });
            if (!string.IsNullOrEmpty(taxCategory))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseTitle = "Duplicate Tax Category",
                    ResponseMessage = "The Tax category name is already exist, try different name"
                });
            }

            int logSummaryID = await _dbContext.InsertAddEditLogSummary(model.TaxCategoryID, "Tax category added or edited by User : " + CurrentUserID);

            var TaxCategory = _mapper.Map<TaxCategory>(model);
            TaxCategory.ClientID = CurrentClientID;
            TaxCategory.TaxCategoryID = await _dbContext.SaveAsync(TaxCategory, null, logSummaryID);

            List<TaxCategoryItem> TaxCategoryItems = _mapper.Map<List<TaxCategoryItem>>(model.TaxCategoryItems);
            if (TaxCategoryItems.Count > 0)
            {
                //int? accountGroupID = await _dbContext.GetByQueryAsync<int?>(@$"
                //                                        Select Top 1 AccountGroupID 
                //                                        From AccAccountGroup
                //                                        Where GroupTypeID=@GroupTypeID AND ClientID=@ClientID", new { GroupTypeID = (int)AccountGroupTypes.DutiesAndTaxes, ClientID = CurrentClientID });

                foreach (var item in TaxCategoryItems)
                {
                    //AccLedger taxCategoryLedger = new()
                    //{
                    //    LedgerID = item.LedgerID.Value,
                    //    LedgerName = item.TaxCategoryItemName + " " + item.Percentage.ToString(),
                    //    AccountGroupID = accountGroupID,
                    //    ClientID = CurrentClientID,
                    //    BranchID = CurrentBranchID
                    //};
                    //taxCategoryLedger.LedgerName = item.TaxCategoryItemName;
                    //if (!taxCategoryLedger.LedgerName.Contains($"{(int)Math.Floor(item.Percentage)}"))
                    //    taxCategoryLedger.LedgerName += "-" + (int)Math.Floor(item.Percentage);
                    //taxCategoryLedger.LedgerCode = item.TaxCategoryItemName.Replace(' ', '-');
                    //if (!taxCategoryLedger.LedgerCode.Contains($"{(int)Math.Floor(item.Percentage)}"))
                    //    taxCategoryLedger.LedgerCode += "-" + (int)Math.Floor(item.Percentage);
                    
                    //taxCategoryLedger.LedgerID = await _dbContext.SaveAsync(taxCategoryLedger);
                    TaxCategoryItem TaxCategoryItem = _mapper.Map<TaxCategoryItem>(item);
                    //TaxCategoryItem.LedgerID = taxCategoryLedger.LedgerID;
                    TaxCategoryItem.TaxCategoryID = TaxCategory.TaxCategoryID;
                    await _dbContext.SaveAsync(TaxCategoryItem);
                }
            }
            var returnModel = new TaxCategoryAddResultModel() { TaxCategoryID = TaxCategory.TaxCategoryID, TaxCategoryName = TaxCategory.TaxCategoryName };
            return Ok(returnModel);
        }

        [HttpGet("get-tax-category/{taxCategoryID}")]
        public async Task<IActionResult> GetTaxCategory(int taxCategoryID)
        {
            TaxCategory Result = await _dbContext.GetAsync<TaxCategory>(taxCategoryID);

            TaxCategoryModel TaxCategory = new()
            {
                TaxCategoryID = Result.TaxCategoryID,
                TaxCategoryName = Result.TaxCategoryName,
            };

            TaxCategory.TaxCategoryItems = await _dbContext.GetListByQueryAsync<TaxCategoryItemModel>(@$"Select TI.*,L.LedgerName
                                                                                                            From TaxCategoryItem TI 
                                                                                                            Left Join AccLedger L ON L.LedgerID=TI.LedgerID AND L.IsDeleted=0
                                                                                                            Where TI.IsDeleted=0 AND TI.TaxCategoryID={taxCategoryID}", null);
            return Ok(TaxCategory ?? new());
        }

        [HttpGet("delete-tax-category")]
        public async Task<IActionResult> DeleteTaxCategory(int Id)
        {
            int count = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) 
                                                                    From QuotationItem QI 
                                                                    Left Join Quotation Q ON Q.QuotationID=QI.QuotationID And Q.IsDeleted=0
                                                                    Where QI.TaxCategoryID={Id} And QI.IsDeleted=0 And Q.BranchID={CurrentBranchID}", null);

            if (count > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseTitle = "Invalid Action",
                    ResponseMessage = "The Tax category you are trying to delete is already in Quotation"
                });
            }


            int ItemTax = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) 
                                                            From Item 
                                                            Where InterTaxCategoryID={Id} or IntraTaxCategoryID={Id} and IsDeleted=0", null);

            if (ItemTax > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseTitle = "Invalid Action",
                    ResponseMessage = "The Tax category you are trying to delete is already in Item"
                });
            }

            int MembershipPackageTax = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) 
                                                                            From MembershipPackage 
                                                                            Where TaxCategoryID={Id} and {CurrentClientID}={PDV.ProgbizClientID} and IsDeleted=0", null);

            if (MembershipPackageTax > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseTitle = "Invalid Action",
                    ResponseMessage = "The Tax category you are trying to delete is already in membership package"
                });
            }



            int logSummaryID = await _dbContext.InsertDeleteLogSummary(Id, "Tax category deleted by User :" + CurrentUserID);

            List<TaxCategoryItemDeleteModel> TaxCategoryItemDeleteList = await _dbContext.GetListByQueryAsync<TaxCategoryItemDeleteModel>(@$"
                                                Select TaxCategoryItemID,LedgerID
                                                From TaxCategoryItem
                                                Where TaxCategoryID={Id}
            ", null);

            if (TaxCategoryItemDeleteList.Count > 0)
            {
                foreach (var item in TaxCategoryItemDeleteList)
                {
                    await _dbContext.ExecuteAsync($"Update TaxCategoryItem Set IsDeleted=1 Where TaxCategoryID={Id}");
                    await _dbContext.ExecuteAsync($"Update AccLedger Set IsDeleted=1 Where LedgerID={item.LedgerID}");
                }
            }

            await _dbContext.DeleteAsync<TaxCategory>(Id, null, logSummaryID);
            return Ok(true);
        }

        [HttpPost("get-tax-category-paged-list")]
        public async Task<IActionResult> GetTaxCategoryPagedList(PagedListPostModelWithFilter searchModel)
        {
            PagedListQueryModel query = searchModel;
            query.Select = $"Select TaxCategoryID,TaxCategoryName From TaxCategory";
            query.WhereCondition = $"ClientID={CurrentClientID} And IsDeleted=0";
            query.OrderByFieldName = searchModel.OrderByFieldName;
            if (!searchModel.SearchString.IsNullOrEmpty())
            {
                query.WhereCondition += $" and TaxCategoryName like '%{searchModel.SearchString}%'";
            }
            var result = await _dbContext.GetPagedList<TaxCategoryListModel>(query, null);

            return Ok(result);
        }

        [HttpGet("get-tax-category-details/{taxCategoryID}")]
        public async Task<IActionResult> GetTaxCategoryDetails(int taxCategoryID)
        {
            var result = await _dbContext.GetByQueryAsync<TaxCategorySelectedGetModel>(@$"
                                                                            Select * 
                                                                            From viTaxCategory 
                                                                            Where TaxCategoryID={taxCategoryID}", null);

            result.TaxCategoryItems = await _dbContext.GetListByQueryAsync<QuotationItemTaxCategoryItemsModel>($@"
                                                                            Select TaxCategoryItemID,Concat(TaxCategoryItemName,'   [ ',Convert(smallint,Percentage),' ]') AS TaxCategoryItemName,Percentage,0 As Amount
                                                                            From TaxCategoryItem T
                                                                            Where T.TaxCategoryID={result.TaxCategoryID} AND T.IsDeleted=0", null);

            return Ok(result ?? new());
        }

        [HttpPost("get-tax-category-details-by-id/{taxCategoryID}")]
        public async Task<IActionResult> GetTaxCategoryDetailsWithTaxItems(int taxCategoryID)
        {
            var taxCategory = await _dbContext.GetByQueryAsync<TaxCategoryDetailsModel>($"Select * From viTaxCategory Where TaxCategoryID={taxCategoryID}", null);
            taxCategory.TaxCategoryItems = await _dbContext.GetListByQueryAsync<TaxCategoryItemModel>($@"
                                                                            Select TaxCategoryItemID,Concat(TaxCategoryItemName,'   [ ',Convert(smallint,Percentage),' ]') AS TaxCategoryItemName,Percentage,L.LedgerName,L.LedgerID
                                                                            From TaxCategoryItem T
                                                                            Left Join AccLedger L ON L.LedgerID=T.LedgerID And L.IsDeleted=0
                                                                            Where T.TaxCategoryID={taxCategoryID} AND T.IsDeleted=0", null);
            return Ok(taxCategory);
        }

        #endregion

        #region Client Invoice

        [HttpPost("get-client-payment-details")]
        public async Task<IActionResult> GetAllClientPaymentDetails(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;

            query.Select = $@"Select PackageName,GrossFee as Fee,PaidStatus,DisconnectionDate,C.ClientID,InvoiceID,InvoiceNo,InvoiceDate
                                From Client C
                                Left Join ClientInvoice CI on C.ClientID=CI.ClientID and CI.IsDeleted=0 
                                Left Join MembershipPackage MP on CI.packageID=MP.PackageID and MP.IsDeleted=0";

            query.WhereCondition = $"C.Isdeleted=0 and C.ClientID={CurrentClientID}";
            query.OrderByFieldName = "InvoiceID desc";
            if (!model.SearchString.IsNullOrEmpty())
            {
                query.WhereCondition += $" and Name like '%{model.SearchString}%'";
            }
            query.WhereCondition += _common.GeneratePagedListFilterWhereCondition(model, "PackageName");
            var res = await _dbContext.GetPagedList<ClientInvoiceListModel>(query, null);
            return Ok(res);
        }


        #endregion

        #region Item Qr Code

        [HttpGet("get-client-qr-code-state")]
        public async Task<IActionResult> GetClientItemQrCodeState()
        {
            var qrCodeStateModel = await _dbContext.GetByQueryAsync<QrCodeStateModel>("Select ItemQrCodeState,ItemGroupQrCodeState From ClientSetting Where ClientID=@ClientID And IsDeleted=0", new { ClientID = CurrentClientID }, null);
            return Ok(qrCodeStateModel ?? new());
        }

        #endregion

        #region Client Setting

        [HttpPost("save-client-setting")]
        public async Task<IActionResult> SaveClientSetting(ClientSettingModel clientSettingModel)
        {
            var clientSetting = _mapper.Map<ClientSetting>(clientSettingModel);
            string commaSeparatedNames = string.Join(", ", clientSettingModel.TaxCategoyItems.Select(item => item.TaxCategoyItemName));
            clientSetting.TaxCategoryItemNames = commaSeparatedNames;
            clientSetting.ClientID = CurrentClientID;
            await _dbContext.SaveAsync(clientSetting);
            return Ok(new Success());
        }

        [HttpGet("get-client-setting")]
        public async Task<IActionResult> GetClientSetting()
        {
            var clientSetting = await _dbContext.GetByQueryAsync<ClientSetting>($"Select * From ClientSetting Where ClientID={CurrentClientID} And IsDeleted=0", null);
            var clientSettingModel = _mapper.Map<ClientSettingModel>(clientSetting);
            clientSettingModel = clientSettingModel ?? new();
            if (clientSetting is not null && clientSetting.TaxCategoryItemNames is not null)
            {
                string[] names = clientSetting.TaxCategoryItemNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                List<ClientSettingTaxCategoryItemModel> taxCategoryItems = names.Select(name => new ClientSettingTaxCategoryItemModel
                {
                    TaxCategoyItemName = name.Trim()
                }).ToList();
                clientSettingModel.TaxCategoyItems = taxCategoryItems;
            }
            return Ok(clientSettingModel ?? new());
        }

        #endregion

        #region Payment Term
        [HttpPost("save-payment-term")]
        public async Task<IActionResult> SavePaymentTerm(PaymentTermModel model)
        {

            var paymentTernName = await _dbContext.GetFieldsAsync<PaymentTerm, string>(@$"PaymentTermName", $"LOWER(TRIM(PaymentTermName))=LOWER(TRIM(@PaymentTermName)) and PaymentTermID<>@PaymentTermID and ClientID={CurrentClientID} and IsDeleted=0", new { PaymentTermName = model.PaymentTermName, PaymentTermID = model.PaymentTermID });
            if (!string.IsNullOrEmpty(paymentTernName))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseTitle = "Duplicate Payment Term",
                    ResponseMessage = "The Payment term name is already exist, try different name"
                });
            }

            int logSummaryID = await _dbContext.InsertAddEditLogSummary(model.PaymentTermID, "Payment term added or edited by User : " + CurrentUserID);

            var PaymentTerm = _mapper.Map<PaymentTerm>(model);
            PaymentTerm.ClientID = CurrentClientID;
            PaymentTerm.PaymentTermID = await _dbContext.SaveAsync(PaymentTerm, null, logSummaryID);

            List<PaymentTermSlab> PaymentTermSlabs = _mapper.Map<List<PaymentTermSlab>>(model.PaymentTermSlabs);

            var tempPaymentslabList = await _dbContext.GetListByQueryAsync<PaymentTermSlabModel>($@"Select * From PaymentTermSlab
                                                                                                        Where PaymentTermID={model.PaymentTermID} and IsDeleted=0 ", null);

            foreach (var delt in tempPaymentslabList)
            {
                var slabToAdd = model.PaymentTermSlabs
                                .Where(S => S.SlabID == delt.SlabID && S.SlabID != 0).FirstOrDefault();

                if (slabToAdd is null)
                {
                    await _dbContext.ExecuteAsync($@"Update PaymentTermSlab SET IsDeleted=1 Where SlabID={delt.SlabID}", null);
                }

            }


            if (PaymentTermSlabs.Count > 0)
            {
               foreach (var item in PaymentTermSlabs)
                {
                  
                    PaymentTermSlab PaymentTermSlab = _mapper.Map<PaymentTermSlab>(item);
                    PaymentTermSlab.PaymentTermID = PaymentTerm.PaymentTermID;
                    await _dbContext.SaveAsync(PaymentTermSlab);
                }
            }
            var returnModel = new PaymentTermAddResultModel() { PaymentTermID = PaymentTerm.PaymentTermID, PaymentTermName = PaymentTerm.PaymentTermName };
            return Ok(returnModel);
        }

        [HttpGet("get-payment-term/{paymentTermID}")]
        public async Task<IActionResult> GetPaymentTerm(int paymentTermID)
        {
            PaymentTerm Result = await _dbContext.GetAsync<PaymentTerm>(paymentTermID);

            PaymentTermModel PaymentTerm = new()
            {
                PaymentTermID = Result.PaymentTermID,
                PaymentTermName = Result.PaymentTermName,
                Description=Result.Description,
            };

            PaymentTerm.PaymentTermSlabs = await _dbContext.GetListByQueryAsync<PaymentTermSlabModel>(@$"Select * From PaymentTermSlab
                                                                                                    Where IsDeleted=0 and PaymentTermID={paymentTermID}", null);
            return Ok(PaymentTerm ?? new());
        }

        [HttpGet("delete-payment-term")]
        public async Task<IActionResult> DeletePaymentTerm(int Id)
        {
            int logSummaryID = await _dbContext.InsertDeleteLogSummary(Id, "Payment term deleted by User :" + CurrentUserID);

            await _dbContext.ExecuteAsync($"Update PaymentTermSlab Set IsDeleted=1 Where PaymentTermID={Id}");
              
            await _dbContext.DeleteAsync<PaymentTerm>(Id, null, logSummaryID);
            return Ok(true);
        }



        [HttpPost("get-payment-term-paged-list")]
        public async Task<IActionResult> GetPaymentTermPagedList(PagedListPostModelWithFilter searchModel)
        {
            PagedListQueryModel query = searchModel;
            query.Select = $"Select * From PaymentTerm";
            query.WhereCondition = $"ClientID={CurrentClientID} And IsDeleted=0";
            query.OrderByFieldName = searchModel.OrderByFieldName;
            if (!searchModel.SearchString.IsNullOrEmpty())
            {
                query.WhereCondition += $" and PaymentTermName like '%{searchModel.SearchString}%'";
            }
            var result = await _dbContext.GetPagedList<PaymentTermListModel>(query, null);

            return Ok(result);
        }


        [HttpGet("remove-payment-term/{Id}")]
        public async Task<IActionResult> DeleteAllPaymentTerm(int Id)
        {
            int logSummaryID = await _dbContext.InsertDeleteLogSummary(Id, "Payment term deleted by User :" + CurrentUserID);

            await _dbContext.ExecuteAsync($"Update PaymentTermSlab Set IsDeleted=1 Where PaymentTermID={Id}");

            await _dbContext.DeleteAsync<PaymentTerm>(Id, null, logSummaryID);
            return Ok(true);
        }

        #endregion

        #region Promotion

        [HttpGet("get-promotions/{promotionID}")]
        public async Task<IActionResult> GetPromotion(int promotionID)
        {
            Promotion Result = await _dbContext.GetAsync<Promotion>(promotionID);

            PromotionModel promotion = new()
            {
                PromotionID = Result.PromotionID,
                PromotionName = Result.PromotionName,
                StartDate = Result.StartDate,
                EndDate=Result.EndDate,
                ClientID=Result.ClientID
            };

            promotion.PromotionItems = await _dbContext.GetListByQueryAsync<PromotionItemListViewModel>(@$"Select PromotionItemID,DiscountTypeID,Percentage,Amount,PromotionID,
		                                                                                                            Case When PR.ItemID is not null then ItemName else ItemModelName end as ItemName,PR.ItemID,PR.ItemVariantID
                                                                                                            From PromotionItem PR
                                                                                                            Left Join Item I on I.ItemID=PR.ItemID and I.IsDeleted=0
                                                                                                            Left Join (Select ItemName as ItemModelName,IM.ItemVariantID From ItemVariant IM 
			                                                                                                            Left Join Item IT on IT.ItemID=IM.ItemID  and IT.IsDeleted=0
			                                                                                                            Where IM.IsDeleted=0)PM on PM.ItemVariantID=PR.ItemVariantID
                                                                                                            Where PR.PromotionID={promotionID} and PR.IsDeleted=0 ", null);
            return Ok(promotion ?? new());
        }


        [HttpPost("save-promotion")]
        public async Task<IActionResult> SavePromotion(PromotionModel model)
        {

            var promotionName = await _dbContext.GetFieldsAsync<Promotion, string>(@$"PromotionName", $"LOWER(TRIM(PromotionName))=LOWER(TRIM(@PromotionName)) and PromotionID<>@PromotionID and ClientID={CurrentClientID} and IsDeleted=0", new { PromotionName = model.PromotionName, PromotionID = model.PromotionID });
            if (!string.IsNullOrEmpty(promotionName))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseTitle = "Duplicate Promotion",
                    ResponseMessage = "The promotion name is already exist, try different name"
                });
            }

            int logSummaryID = await _dbContext.InsertAddEditLogSummary(model.PromotionID, "promotion name added or edited by User : " + CurrentUserID);

            var promotion = _mapper.Map<Promotion>(model);
            promotion.ClientID = CurrentClientID;
            promotion.PromotionID = await _dbContext.SaveAsync(promotion, null, logSummaryID);

            List<PromotionItem> promotionItem = _mapper.Map<List<PromotionItem>>(model.PromotionItems);

            var tempPromotionItemList = await _dbContext.GetListByQueryAsync<PromotionItemListViewModel>($@"Select * From PromotionItem
                                                                                                        Where PromotionID={model.PromotionID} and IsDeleted=0 ", null);

            foreach (var delt in tempPromotionItemList)
            {
                var slabToAdd = model.PromotionItems
                                .Where(S => S.PromotionItemID == delt.PromotionItemID && S.PromotionItemID != 0).FirstOrDefault();

                if (slabToAdd is null)
                {
                    await _dbContext.ExecuteAsync($@"Update PromotionItem SET IsDeleted=1 Where PromotionItemID={delt.PromotionItemID}", null);
                }

            }


            if (promotionItem.Count > 0)
            {
                foreach (var item in promotionItem)
                {

                    PromotionItem PromotionItem = _mapper.Map<PromotionItem>(item);
                    PromotionItem.PromotionID = promotion.PromotionID;

                    await _dbContext.SaveAsync(PromotionItem);
                }
            }
            var returnModel = new PromotionAddResultModel() { PromotionID = promotion.PromotionID, PromotionName = promotion.PromotionName };
            return Ok(returnModel);
        }


        [HttpGet("delete-promotion")]
        public async Task<IActionResult> DeletePromotion(int Id)
        {
            int logSummaryID = await _dbContext.InsertDeleteLogSummary(Id, "Promotion deleted by User :" + CurrentUserID);

            await _dbContext.ExecuteAsync($"Update PromotionItem Set IsDeleted=1 Where PromotionItemID={Id}");

            await _dbContext.DeleteAsync<Promotion>(Id, null, logSummaryID);
            return Ok(true);
        }

        [HttpPost("get-promotion-paged-list")]
        public async Task<IActionResult> GetPromotionPagedList(PagedListPostModelWithFilter searchModel)
        {
            PagedListQueryModel query = searchModel;
            query.Select = $"Select * From Promotion";
            query.WhereCondition = $"ClientID={CurrentClientID} And IsDeleted=0";
            query.OrderByFieldName = searchModel.OrderByFieldName;
            if (!searchModel.SearchString.IsNullOrEmpty())
            {
                query.WhereCondition += $" and PromotionName like '%{searchModel.SearchString}%'";
            }
            var result = await _dbContext.GetPagedList<PromotionListModel>(query, null);

            return Ok(result);
        }

        [HttpGet("remove-promotion/{Id}")]
        public async Task<IActionResult> RemovePromotion(int Id)
        {
            int logSummaryID = await _dbContext.InsertDeleteLogSummary(Id, "Promotion deleted by User :" + CurrentUserID);

            await _dbContext.ExecuteAsync($"Update PromotionItem Set IsDeleted=1 Where PromotionID={Id}");

            await _dbContext.DeleteAsync<Promotion>(Id, null, logSummaryID);
            return Ok(true);
        }
        #endregion



        #region Business Type

        [HttpGet("delete-business-type")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ClientSetting")]
        public async Task<IActionResult> DeleteBusinessType(int Id)
        {


            int quotationCount = await _dbContext.GetByQueryAsync<int>($@"SELECT Count(*) AS DataCount 
                                                                           from BusinessType F
                                                                            LEFT JOIN Quotation E ON E.BusinessTypeID=F.BusinessTypeID
                                                                            WHERE E.IsDeleted =0 and F.IsDeleted=0 and BusinessTypeID={Id}
                                                                            GROUP BY BusinessTypeID", null);

            if (quotationCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid operation",
                    ResponseMessage = "Quotation entry exist with the business type you are trying to remove"
                });
            }
            await _dbContext.ExecuteAsync($"UPDATE BusinessType SET IsDeleted=1 Where BusinessTypeID={Id}");
            return Ok(true);
        }

        [HttpPost("get-business-type-paged-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ClientSetting")]
        public async Task<IActionResult> GetBusinessList(PagedListPostModel model)
        {
            PagedListQueryModel query = model;
            query.Select = $@"Select *
								From BusinessType";
            query.WhereCondition = $"IsDeleted=0 and (ClientID={CurrentClientID} OR ClientID IS NULL)";
            query.OrderByFieldName = model.OrderByFieldName;
            query.SearchString = model.SearchString;
            query.SearchLikeColumnNames = new() { "BusinessTypeName" };
            var res = await _dbContext.GetPagedList<BusinessTypeModel>(query, null);
            return Ok(res);
        }

        #endregion

    }
}
