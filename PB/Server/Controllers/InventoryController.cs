using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using PB.DatabaseFramework;
using PB.Model;
using PB.Model.Models;
using PB.Server.Repository;
using System.Data;
using PB.Shared.Models.Accounts.VoucherTypes;
using PB.Shared.Models.Common;
using PB.Shared.Models.Inventory.Invoice;
using PB.Shared.Models;
using PB.Shared;
using PB.EntityFramework;
using PB.Shared.Tables.Inventory.InvoiceType;
using PB.Shared.Models.Inventory.Invoices;
using PB.Shared.Enum;
using PB.Shared.Tables;
using PB.Shared.Tables.Inventory.Invoices;
using PB.Shared.Models.Inventory.InvoiceType;
using PB.Shared.Tables.CRM;
using NPOI.SS.Formula.Functions;
using static NPOI.HSSF.Util.HSSFColor;
using PB.Shared.Views;
using PB.Shared.Models.Inventory.Item;
using PB.Shared.Models.CRM;
using PB.Shared.Models.CRM.Quotation;
using PB.Shared.Enum.CRM;
using System.Text.RegularExpressions;
using PB.CRM.Model.Enum;
using PB.Shared.Tables.Accounts.JournalMaster;
using PB.Shared.Tables.Inventory.Items;
using PB.Shared.Enum.Inventory;
using PB.Shared.Models.Inventory.Items;
using System.Diagnostics;

namespace PB.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class InventoryController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection _cn;
        private readonly IMapper _mapper;
        private readonly IAccountRepository _accounts;
        private readonly IInventoryRepository _inventory;
        private readonly ICommonRepository _common;
        private readonly ICRMRepository _crm;
        private readonly IItemRepository _item;
        private readonly IPDFRepository _pdf;
        private readonly ISuperAdminRepository _superAdmin;
        public InventoryController(IDbContext dbContext, IDbConnection cn, IMapper mapper, IAccountRepository accounts, IInventoryRepository inventory, ICommonRepository common, ICRMRepository crm, IItemRepository item, IPDFRepository pdf, ISuperAdminRepository superAdmin)
        {
            _dbContext = dbContext;
            _cn = cn;
            _mapper = mapper;
            _accounts = accounts;
            _inventory = inventory;
            _common = common;
            _crm = crm;
            _item = item;
            _pdf = pdf;
            _superAdmin = superAdmin;
        }

        #region Invoice Type

        [HttpPost("save-invoice-type")]
        public async Task<IActionResult> InsertInvoiceType(InvoiceTypeModel model)
        {
            var InvoiceTypeName = await _dbContext.GetByQueryAsync<string?>(@$"Select VoucherTypeName 
                                                                        from AccVoucherType
                                                                        where LOWER(VoucherTypeName) =LOWER(@InvoiceTypeName) and VoucherTypeID<>@VoucherTypeID and ClientID={CurrentClientID}  and IsDeleted=0", model);
            if (InvoiceTypeName != null)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invoice type name already exist",
                    ResponseMessage = "The invoice type name already exist,try different invoice type name"
                });
            }
            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                model.ClientID = CurrentClientID;
                //model.VoucherTypeName = model.InvoiceTypeName;
                model.BranchID = model.BranchID == null ? CurrentBranchID : model.BranchID;
                var voucherType = _mapper.Map<VoucherTypeModel>(model);

                model.VoucherTypeID = await _accounts.InsertAccVoucherType(voucherType, tran);
                model.InvoiceTypeID = await _inventory.InsertInvoicetypeEntry(model, tran);

                tran.Commit();
                InvoiceTypeSuccessModel res = new()
                {
                    InvoiceTypeID = model.InvoiceTypeID,
                    InvoiceTypeName = model.InvoiceTypeName,
                    ResponseTitle = "Success",
                    ResponseMessage = "InvoiceTypeSaved"

                };
                return Ok(res);
            }
        }

        [HttpGet("get-invoice-type/{Id}")]
        public async Task<IActionResult> GerInvoiceType(int Id)
        {
            var result = await _inventory.GetInvoiceType(Id, CurrentClientID, null);
            return Ok(result);
        }

        [HttpGet("delete-invoice-type/{Id}")]
        public async Task<IActionResult> DeleteInvoice(int Id)
        {
            await _inventory.DeleteInvoiceType(Id, CurrentClientID, null);
            return Ok(new Success());
        }

        [HttpPost("get-invoice-type-paged-list")]
        public async Task<IActionResult> GetInvoiceTypePagedList(PagedListPostModelWithFilter searchModel)
        {
            PagedListQueryModel searchData = searchModel;
            searchData.Select = $@"Select InvoiceTypeName,VoucherTypeName,InvoiceTypeNatureID,IT.InvoiceTypeID,VT.VoucherTypeNatureID,PeriodicityID,NumberingTypeID
                                From InvoiceType IT
                                Join AccVoucherType VT on VT.VoucherTypeID=IT.VoucherTypeID and VT.IsDeleted=0
                                Join AccVoucherTypeNature VN on VN.VoucherTypeNatureID=VT.VoucherTypeNatureID and VN.IsDeleted=0
                                Join AccVoucherTypeSetting VS on VS.VoucherTypeID=VT.VoucherTypeID and VS.IsDeleted=0";
            searchData.WhereCondition = $"IT.ClientID={CurrentClientID} and IT.IsDeleted=0";
            searchData.OrderByFieldName = searchModel.OrderByFieldName;
            searchData.WhereCondition += _common.GeneratePagedListFilterWhereCondition(searchModel, "VT.VoucherTypeName");
            var res = await _dbContext.GetPagedList<InvoiceTypeModel>(searchData, null);
            return Ok(res ?? new());
        }

        #region Invoice Charge

        [HttpGet("get-invoice-charge/{Id}")]
        public async Task<IActionResult> GerInvoiceCharge(int Id)
        {
            var result = await _dbContext.GetByQueryAsync<InvoiceTypeChargeModel>($@"Select IC.*,LedgerName From InvoiceTypeCharge IC 
                                                                                    Join AccLedger AL on AL.LedgerID=IC.LedgerID and AL.IsDeleted=0
                                                                                    Where IC.InvoiceTypeChargeID={Id} and IC.IsDeleted=0", null);
            return Ok(result);
        }

        [HttpGet("delete-invoice-charge/{Id}")]
        public async Task<IActionResult> DeleteInvoiceCharge(int Id)
        {
            await _dbContext.DeleteAsync<InvoiceTypeCharge>(Id);
            return Ok(new Success());
        }


        #endregion

        #endregion

        #region Invoice

        [HttpGet("create-invoice-from-quotation/{quotationID}")]
        public async Task<IActionResult> CreateInvoiceFromQuotation(int quotationID)
        {
            var quotation = await _crm.GetQuotationById(quotationID, CurrentBranchID);
            var invoice = _mapper.Map<InvoiceModel>(quotation);

            //Invoice type
            int invoiceTypeID = await _dbContext.GetByQueryAsync<int>("Select Top 1 InvoiceTypeID From InvoiceType Where ClientID=@ClientID And InvoiceTypeNatureID=@InvoiceTypeNatureID And InvoiceTypeName Like @InvoiceTypeName And IsDeleted=0", new { ClientID = CurrentClientID, InvoiceTypeNatureID = (int)InvoiceTypeNatures.Sales, InvoiceTypeName = "%Sales%" });
            if (invoiceTypeID != 0)
            {
                var invoiceTypeDetails = await _inventory.GetInvoiceTypeDetailsById(invoiceTypeID);
                invoice.InvoiceTypeID = invoiceTypeDetails.InvoiceTypeID;
                invoice.InvoiceTypeName = invoiceTypeDetails.InvoiceTypeName;
                invoice.Prefix = invoiceTypeDetails.Prefix;
                invoice.InvoiceTypeNatureID = invoiceTypeDetails.InvoiceTypeNatureID;
                invoice.InvoiceExtraChargeItems = invoiceTypeDetails.ExtraChargeItems;
            }

            //Invoice items
            for (int i = 0; i < quotation.QuotationItems.Count; i++)
            {
                var itemModel = await _item.GetItemModel(quotation.QuotationItems[i].ItemVariantID.Value);
                InvoiceItemModel invoiceItemModel = new()
                {
                    ItemVariantID = itemModel.ItemVariantID,
                    ItemName = itemModel.ItemModelName,
                    Quantity = quotation.QuotationItems[i].Quantity,
                    Rate = quotation.QuotationItems[i].Rate,
                    TaxPreferenceTypeID = itemModel.TaxPreferenceTypeID,
                    TaxPreferenceName = itemModel.TaxPreferenceName,
                    IsGoods = itemModel.IsGoods,
                    CurrentStock = itemModel.CurrentStock,
                };

                if (quotation.QuotationItems[i].TaxCategoryID is not null && itemModel.TaxPreferenceTypeID == (int)TaxPreferences.Taxable)
                {
                    var taxDetails = await _superAdmin.GetTaxCategoryDetailsById(quotation.QuotationItems[i].TaxCategoryID.Value);
                    if (taxDetails is not null)
                    {
                        invoiceItemModel.TaxCategoryID = taxDetails.TaxCategoryID;
                        invoiceItemModel.TaxCategoryName = taxDetails.TaxCategoryName;
                        if (taxDetails.TaxCategoryItems is not null)
                        {
                            invoiceItemModel.TaxPercentage = taxDetails.TaxCategoryItems.Sum(taxItem => taxItem.Percentage);
                            invoiceItemModel.TaxCategoryItems = taxDetails.TaxCategoryItems;
                        }
                    }
                }
                invoiceItemModel = _inventory.HandleInvoiceItemCalculation(invoiceItemModel);
                invoice.Items.Add(invoiceItemModel);
            }

            //Assignees
            invoice.InvoiceAssignees = _mapper.Map<List<InvoiceAssignee>>(quotation.QuotationAssignees);

            //Mail recipients
            invoice.InvoiceMailReceipients = _mapper.Map<List<InvoiceMailReceipient>>(quotation.MailReciepients);
            invoice.UserEntityID = null;
            invoice.QuotationID = quotationID;
            invoice.FileName = null;
            invoice.MediaID = null;
            invoice.CurrentFollowupNature = null;
            return Ok(invoice);
        }

        [HttpGet("get-invoice-type-details/{invoiceTypeID}")]
        public async Task<IActionResult> GetInvoiceTypeCharges(int invoiceTypeID)
        {
            return Ok(await _inventory.GetInvoiceTypeDetailsById(invoiceTypeID));
        }

        [HttpPost("get-invoice-item-details")]
        public async Task<IActionResult> GetInvoiceItem(ItemSelectedPostModel postModel)
        {
            var itemModel = await _item.GetItemModelDetails(postModel.ItemVariantID, CurrentBranchID, postModel.PlaceOfSupplyID);
            var invoiceItem = _mapper.Map<InvoiceItemModel>(itemModel);
            invoiceItem.Rate = itemModel.Price;
            invoiceItem = _inventory.HandleInvoiceItemCalculation(invoiceItem);
            return Ok(invoiceItem);
        }

        [HttpPost("get-updated-invoice-items")]
        public async Task<IActionResult> GetUpdatedInvoiceItems(UpdateItemVariantsPostRequestModel postModel)
        {
            List<InvoiceItemModel> invoiceItems = new();
            int placeOrSourceOfSupplyID = 0;
            if (postModel.PlaceOfSupplyID > 0)
                placeOrSourceOfSupplyID = postModel.PlaceOfSupplyID;
            if (postModel.SourceOfSupplyID > 0)
                placeOrSourceOfSupplyID = postModel.SourceOfSupplyID;

            foreach (var itemModelID in postModel.ItemVariantIDs)
            {
                ItemVariantDetail itemModel = await _item.GetItemModelDetails(itemModelID, CurrentBranchID, placeOrSourceOfSupplyID);
                var invoiceItem = _mapper.Map<InvoiceItemModel>(itemModel);
                invoiceItem.Rate = itemModel.Price;
                invoiceItem = _inventory.HandleInvoiceItemCalculation(invoiceItem);
                invoiceItems.Add(invoiceItem);
            }
            return Ok(invoiceItems);
        }

        [HttpGet("get-advance-details/{entityID}")]
        public async Task<IActionResult> GetAdvanceAmount(int entityID)
        {
            int ledgerID = await _accounts.GetEntityLedgerID(entityID, CurrentClientID);
            var adavnces = await _dbContext.GetListByQueryAsync<BillToBillAgainstReferenceModel>($@"Select B.BillID,B.ReferenceNo,B.Date,vB.Credit As BillAmount
                                                                    From AccBilltoBill B
                                                                    Join viBillToBillBalance vB ON B.BillID=vB.BillID
                                                                    Where B.LedgerID={ledgerID} And B.ReferenceTypeID={(int)ReferenceTypes.Advance}", null);
            return Ok(adavnces ?? new());
        }

        [HttpPost("save-invoice")]
        public async Task<IActionResult> SaveInvoice(InvoiceModel model)
        {
            int voucherTypeID = 0;
            VoucherNumberModel? voucherNumber = null;
            if (model.InvoiceID > 0 && model.InvoiceNumber == 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseMessage = "Not a valid invoice number"
                });
            }

            model.BranchID = CurrentBranchID;
            model.UserEntityID = model.UserEntityID == null ? CurrentEntityID : model.UserEntityID;
            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {

                InvoiceType invoiceType = await _dbContext.GetAsync<InvoiceType>(Convert.ToInt32(model.InvoiceTypeID), tran);
                voucherTypeID = Convert.ToInt32(invoiceType.VoucherTypeID);

                #region Converted Quotation Status Updation

                if (model.QuotationID != null)
                {
                    FollowUpModel followUpModel = new()
                    {
                        FollowUpStatusID = await _dbContext.GetByQueryAsync<int?>("Select Top 1 FollowUpStatusID From FollowUpStatus Where Nature=@Nature And Type=@Type And (ClientID=@ClientID Or IsNull(ClientID,0)=0) And IsDeleted=0", new { Nature = (int)FollowUpNatures.ClosedWon, Type = (int)FollowUpTypes.Quotation, ClientID = CurrentClientID }, tran),
                        EntityID = CurrentEntityID,
                        FollowUpType = (int)FollowUpTypes.Quotation,
                        QuotationID = model.QuotationID.Value,
                    };
                    await _crm.SaveFollowup(followUpModel, tran);
                }

                #endregion

                #region Invoice Number

                if (model.InvoiceID == 0 || model.InvoiceNumber == 0)
                {
                    voucherNumber = await _accounts.GetVoucherNumber(Convert.ToInt32(invoiceType.VoucherTypeID), CurrentBranchID, tran);
                    if (voucherNumber != null)
                    {
                        model.InvoiceNumber = voucherNumber.JournalNo;
                        model.Prefix = voucherNumber.JournalNoPrefix;
                    }
                }
                else
                {
                    voucherNumber = new();
                    voucherNumber.JournalNo = model.InvoiceNumber;
                    voucherNumber.JournalNoPrefix = model.Prefix;
                }

                #endregion

                #region Invoice

                var invoice = _mapper.Map<Invoice>(model);
                invoice.InvoiceID = await _dbContext.SaveAsync(invoice, tran);

                #endregion

                #region Invoice Items

                if (model.Items.Count > 0)
                {
                    var invoiceItems = _mapper.Map<List<InvoiceItem>>(model.Items);
                    await _dbContext.SaveSubItemListAsync(invoiceItems, "InvoiceID", invoice.InvoiceID, tran);
                }

                #endregion

                #region Invoice Tax Items

                List<InvoiceTaxItem>? taxItemsList = null;
                if (model.InvoiceTaxItems is not null && model.InvoiceTaxItems.Count > 0)
                {
                    taxItemsList = new(model.InvoiceTaxItems);
                    await _dbContext.SaveSubItemListAsync(model.InvoiceTaxItems, "InvoiceID", invoice.InvoiceID, tran);
                }

                #endregion

                #region Invoice Assignees

                if (model.InvoiceAssignees.Count > 0)
                    await _dbContext.SaveSubItemListAsync(model.InvoiceAssignees, "InvoiceID", invoice.InvoiceID, tran);

                #endregion

                #region Mail Recipients

                if (model.InvoiceMailReceipients.Count > 0)
                {
                    await _dbContext.SaveSubItemListAsync(model.InvoiceMailReceipients, "InvoiceID", invoice.InvoiceID, tran);
                }

                #endregion

                #region Accounts Entry

                #region Journal Master

                AccJournalMaster? journalMaster = null;
                if (model.JournalMasterID.HasValue)
                {
                    journalMaster = await _dbContext.GetAsync<AccJournalMaster>(model.JournalMasterID.Value, tran);
                }
                journalMaster ??= new();
                journalMaster.Date = model.AccountsDate;
                journalMaster.BranchID = CurrentBranchID;
                if (voucherTypeID > 0)
                    journalMaster.VoucherTypeID = voucherTypeID;
                if (voucherNumber != null)
                {
                    journalMaster.JournalNoPrefix = voucherNumber.JournalNoPrefix;
                    journalMaster.JournalNo = voucherNumber.JournalNo;
                }
                journalMaster.EntityID = model.CustomerEntityID;
                journalMaster.JournalMasterID = await _dbContext.SaveAsync(journalMaster, tran);
                await _dbContext.ExecuteAsync($@"UPDATE Invoice Set JournalMasterID={journalMaster.JournalMasterID} where InvoiceID={invoice.InvoiceID}", null, tran);

                #endregion

                #region Journal Entry

                List<AccJournalEntry> journalEntries = new();
                switch (model.InvoiceTypeNatureID)
                {
                    case (int)InvoiceTypeNatures.Sales:

                        //cash a/c dr
                        //bank a/c dr
                        //customer a/c dr
                        //Extra charge minus a/c dr
                        //   to Sales a/c
                        //   to Extra charge plus ac
                        //   to Tax Items

                        #region Debit 

                        //cash or bank acount dr
                        foreach (var ledger in model.InvoicePaymentsList.Where(item => item.Amount > 0))
                        {
                            journalEntries.Add(new()
                            {
                                Debit = ledger.Amount,
                                Credit = 0,
                                LedgerID = ledger.LedgerID
                            });
                        }

                        //customer acount dr
                        if (model.CreditAmount > 0)
                        {
                            journalEntries.Add(new()
                            {
                                Debit = model.CreditAmount,
                                Credit = 0,
                                LedgerID = await _accounts.GetEntityLedgerID(Convert.ToInt32(model.CustomerEntityID), CurrentClientID, tran)
                            });
                        }

                        //extra charge minus a/c dr
                        if (model.InvoiceExtraChargeItems != null && model.InvoiceExtraChargeItems.Count > 0)
                        {
                            foreach (var item in model.InvoiceExtraChargeItems.Where(s => s.Amount > 0 && s.ChargeOperation == (int)InvoiceTypeChargeOperations.Minus))
                            {
                                journalEntries.Add(new AccJournalEntry()
                                {
                                    LedgerID = item.LedgerID,
                                    Debit = item.Amount,
                                    Credit = 0,
                                });
                            }
                        }

                        #endregion

                        #region Credit

                        //to Sales ac
                        journalEntries.Add(new AccJournalEntry()
                        {
                            LedgerID = invoiceType.LedgerID,
                            Debit = 0,
                            Credit = model.TotalAmount,//confirm net amount or total amout should credit amount to sales account 
                        });

                        //extra charge plus a/c 
                        if (model.InvoiceExtraChargeItems != null && model.InvoiceExtraChargeItems.Count > 0)
                        {
                            foreach (var extraCharge in model.InvoiceExtraChargeItems.Where(s => s.Amount > 0 && s.ChargeOperation == (int)InvoiceTypeChargeOperations.Add))
                            {
                                journalEntries.Add(new AccJournalEntry()
                                {
                                    LedgerID = extraCharge.LedgerID,
                                    Debit = 0,
                                    Credit = extraCharge.Amount,
                                });
                            }
                        }

                        //tax items ac
                        if (taxItemsList != null && taxItemsList.Count > 0)
                        {
                            foreach (var taxItem in taxItemsList)
                            {
                                journalEntries.Add(new AccJournalEntry()
                                {
                                    LedgerID = await _accounts.GetTaxCategoryItemLedgerID(Convert.ToInt32(taxItem.TaxCategoryItemID), CurrentClientID, tran),
                                    Debit = 0,
                                    Credit = taxItem.TaxAmount,
                                });
                            }
                        }

                        #endregion

                        break;
                    case (int)InvoiceTypeNatures.Sales_Return:

                        //Sales Return a/c dr
                        //Extra charge plus a/c dr
                        //Tax Items
                        //   to Cash or Debtor a/c
                        //   to Extra charge minus a/c

                        #region Debit

                        //Sales Return a / c dr
                        journalEntries.Add(new AccJournalEntry()
                        {
                            LedgerID = invoiceType.LedgerID,
                            Debit = model.GrossAmount, //confirm the amount(net or gross amount)
                            Credit = 0,
                        });

                        //Extra charge plus ac dr
                        if (model.InvoiceExtraChargeItems != null && model.InvoiceExtraChargeItems.Count > 0)
                        {
                            foreach (var extraCharge in model.InvoiceExtraChargeItems.Where(s => s.Amount > 0 && s.ChargeOperation == (int)InvoiceTypeChargeOperations.Add))
                            {
                                journalEntries.Add(new AccJournalEntry()
                                {
                                    LedgerID = extraCharge.LedgerID,
                                    Debit = extraCharge.Amount,
                                    Credit = 0,
                                });
                            }
                        }

                        //Tax Items
                        if (taxItemsList is not null && taxItemsList.Count > 0)
                        {
                            foreach (var taxItem in taxItemsList)
                            {
                                journalEntries.Add(new AccJournalEntry()
                                {
                                    LedgerID = await _accounts.GetTaxCategoryItemLedgerID(Convert.ToInt32(taxItem.TaxCategoryItemID), CurrentClientID, tran),
                                    Debit = taxItem.TaxAmount,
                                    Credit = 0,
                                });
                            }
                        }

                        #endregion

                        #region Credit

                        //to cash or bank a/c
                        foreach (var ledger in model.InvoicePaymentsList.Where(item => item.Amount > 0))
                        {
                            journalEntries.Add(new()
                            {
                                Debit = ledger.Amount,
                                Credit = 0,
                                LedgerID = ledger.LedgerID
                            });
                        }

                        //to customer a/c
                        if (model.CreditAmount > 0)
                        {
                            journalEntries.Add(new()
                            {
                                Debit = model.CreditAmount,
                                Credit = 0,
                                LedgerID = await _accounts.GetEntityLedgerID(Convert.ToInt32(model.CustomerEntityID), CurrentClientID, tran)
                            });

                        }

                        //   to Extra charge minus a/c
                        if (model.InvoiceExtraChargeItems != null && model.InvoiceExtraChargeItems.Count > 0)
                        {
                            foreach (var item in model.InvoiceExtraChargeItems.Where(s => s.Amount > 0 && s.ChargeOperation == (int)InvoiceTypeChargeOperations.Minus))
                            {
                                journalEntries.Add(new AccJournalEntry()
                                {
                                    LedgerID = item.LedgerID,
                                    Debit = item.Amount,
                                    Credit = 0,
                                });
                            }
                        }

                        #endregion

                        break;
                    case (int)InvoiceTypeNatures.Purchase:

                        //Purchase a/c dr
                        //Extra charge plus a/c dr
                        //Tax Items
                        //   to cash,bank or supplier a/c
                        //   to Extra charge minus a/c

                        #region Debit

                        //Purchase a/c dr
                        journalEntries.Add(new AccJournalEntry()
                        {
                            LedgerID = invoiceType.LedgerID,
                            Debit = model.GrossAmount, //confirm the amount(net or gross amount)
                            Credit = 0,
                        });

                        //Extra charge plus ac dr
                        if (model.InvoiceExtraChargeItems != null && model.InvoiceExtraChargeItems.Count > 0)
                        {
                            foreach (var extraCharge in model.InvoiceExtraChargeItems.Where(s => s.Amount > 0 && s.ChargeOperation == (int)InvoiceTypeChargeOperations.Add))
                            {
                                journalEntries.Add(new AccJournalEntry()
                                {
                                    LedgerID = extraCharge.LedgerID,
                                    Debit = extraCharge.Amount,
                                    Credit = 0,
                                });
                            }
                        }

                        //Tax Items
                        if (taxItemsList is not null && taxItemsList.Count > 0)
                        {
                            foreach (var taxItem in taxItemsList)
                            {
                                journalEntries.Add(new AccJournalEntry()
                                {
                                    LedgerID = await _accounts.GetTaxCategoryItemLedgerID(Convert.ToInt32(taxItem.TaxCategoryItemID), CurrentClientID, tran),
                                    Debit = taxItem.TaxAmount,
                                    Credit = 0,
                                });
                            }
                        }

                        #endregion

                        #region Credit

                        //to cash,bank a/c
                        foreach (var ledger in model.InvoicePaymentsList.Where(item => item.Amount > 0))
                        {
                            journalEntries.Add(new()
                            {
                                Debit = ledger.Amount,
                                Credit = 0,
                                LedgerID = ledger.LedgerID
                            });
                        }

                        //supplier a/c
                        if (model.CreditAmount > 0)
                        {
                            journalEntries.Add(new()
                            {
                                Debit = model.CreditAmount,
                                Credit = 0,
                                LedgerID = await _accounts.GetEntityLedgerID(Convert.ToInt32(model.SupplierEntityID), CurrentClientID, tran)
                            });

                        }

                        #endregion

                        break;
                    case (int)InvoiceTypeNatures.Purchase_Return:

                        //cash, bank or supplier a/c dr
                        //Extra charge minus a/c dr
                        //   to Purchase return ac
                        //   to Extra charge plus ac
                        //   to Tax Items

                        #region Debit

                        //cash,bank a/c dr
                        foreach (var ledger in model.InvoicePaymentsList.Where(item => item.Amount > 0))
                        {
                            journalEntries.Add(new()
                            {
                                Debit = ledger.Amount,
                                Credit = 0,
                                LedgerID = ledger.LedgerID
                            });
                        }

                        //Supplier a/c dr
                        if (model.CreditAmount > 0)
                        {
                            journalEntries.Add(new()
                            {
                                Debit = model.CreditAmount,
                                Credit = 0,
                                LedgerID = await _accounts.GetEntityLedgerID(Convert.ToInt32(model.SupplierEntityID), CurrentClientID, tran)
                            });
                        }

                        //extra charge minus a/c dr
                        if (model.InvoiceExtraChargeItems != null && model.InvoiceExtraChargeItems.Count > 0)
                        {
                            foreach (var item in model.InvoiceExtraChargeItems.Where(s => s.Amount > 0 && s.ChargeOperation == (int)InvoiceTypeChargeOperations.Minus))
                            {
                                journalEntries.Add(new AccJournalEntry()
                                {
                                    LedgerID = item.LedgerID,
                                    Debit = item.Amount,
                                    Credit = 0,
                                });
                            }
                        }

                        #endregion

                        #region Credit

                        //to purchase return a/c
                        journalEntries.Add(new AccJournalEntry()
                        {
                            LedgerID = invoiceType.LedgerID,
                            Debit = 0,
                            Credit = model.TotalAmount,//confirm net amount or total amout should credit amount to sales account 
                        });

                        //extra charge plus a/c 
                        if (model.InvoiceExtraChargeItems != null && model.InvoiceExtraChargeItems.Count > 0)
                        {
                            foreach (var extraCharge in model.InvoiceExtraChargeItems.Where(s => s.Amount > 0 && s.ChargeOperation == (int)InvoiceTypeChargeOperations.Add))
                            {
                                journalEntries.Add(new AccJournalEntry()
                                {
                                    LedgerID = extraCharge.LedgerID,
                                    Debit = 0,
                                    Credit = extraCharge.Amount,
                                });
                            }
                        }

                        //to tax items a/c
                        if (taxItemsList != null && taxItemsList.Count > 0)
                        {
                            foreach (var taxItem in taxItemsList)
                            {
                                journalEntries.Add(new AccJournalEntry()
                                {
                                    LedgerID = await _accounts.GetTaxCategoryItemLedgerID(Convert.ToInt32(taxItem.TaxCategoryItemID), CurrentClientID, tran),
                                    Debit = 0,
                                    Credit = taxItem.TaxAmount,
                                });
                            }
                        }

                        #endregion

                        break;
                }
                await _dbContext.SaveSubItemListAsync(journalEntries, "JournalMasterID", journalMaster.JournalMasterID, tran);


                #endregion

                #endregion

                #region Item Stock

                foreach (var item in model.Items.Where(item => item.IsGoods))
                {
                    var itemStock = await _dbContext.GetByQueryAsync<ItemStock>("Select * From ItemStock Where ItemVariantID=@ItemVariantID And BranchID=@BranchID", new { ItemVariantID = item.ItemVariantID, BranchID = CurrentBranchID }, tran);
                    if (itemStock is null)
                    {
                        itemStock = new()
                        {
                            ItemVariantID = item.ItemVariantID,
                            BranchID = CurrentBranchID
                        };
                    }
                    switch (invoiceType.InvoiceTypeNatureID)
                    {
                        case (int)InvoiceTypeNatures.Sales:
                        case (int)InvoiceTypeNatures.Purchase_Return:
                            if (itemStock.Quantity > 0)
                                itemStock.Quantity = Convert.ToDecimal(itemStock.Quantity) - item.Quantity;
                            break;
                        case (int)InvoiceTypeNatures.Purchase:
                        case (int)InvoiceTypeNatures.Sales_Return:
                            itemStock.Quantity = Convert.ToDecimal(itemStock.Quantity) + item.Quantity;
                            break;
                    }
                    await _dbContext.SaveAsync(itemStock, tran);
                }

                #endregion

                #region Invoice Credit Payment

                if (model.CreditAmount > 0 && (model.InvoiceTypeNatureID == (int)InvoiceTypeNatures.Sales || model.InvoiceTypeNatureID == (int)InvoiceTypeNatures.Purchase))
                {
                    List<InvoicePaymentTermSlab> invoicePaymentSlabs = new();
                    List<AccBillToBill> accBillToBills = new();
                    int entityID = model.InvoiceTypeNatureID == (int)InvoiceTypeNatures.Sales ? model.CustomerEntityID.Value : model.SupplierEntityID.Value;
                    int ledgerID = await _accounts.GetEntityLedgerID(entityID, CurrentClientID, tran);
                    int journalEntryID = await _dbContext.GetByQueryAsync<int>($"Select JournalEntryID From AccJournalEntry Where JournalMasterID={journalMaster.JournalMasterID} And LedgerID={ledgerID}", null, tran);
                    foreach (var paymentTermSlabModel in model.PaymentSlabs)
                    {
                        #region BillToBill References
                        AccBillToBill accBillToBill = new()
                        {
                            Date = DateTime.UtcNow,
                            ReferenceTypeID = (int)ReferenceTypes.NewReference,
                            ReferenceNo = voucherNumber != null ? voucherNumber.JournalNoPrefix + '-' + voucherNumber.JournalNo : null,
                            LedgerID = ledgerID,
                            Debit = paymentTermSlabModel.Amount,
                            Credit = 0
                        };
                        accBillToBills.Add(accBillToBill);
                        #endregion

                        #region Invoice Payment TermSlab
                        var paymentTermSlab = new InvoicePaymentTermSlab()
                        {
                            ID = paymentTermSlabModel.ID,
                            SlabID = paymentTermSlabModel.SlabID,
                            Days = paymentTermSlabModel.Days,
                            Percentage = paymentTermSlabModel.Percentage,
                            Amount = paymentTermSlabModel.Amount,
                        };
                        invoicePaymentSlabs.Add(paymentTermSlab);
                        #endregion
                    }

                    await _dbContext.SaveSubItemListAsync(invoicePaymentSlabs, "InvoiceID", invoice.InvoiceID, tran);
                    await _dbContext.SaveSubItemListAsync(accBillToBills, "JournalEntryID", journalEntryID, tran);
                }

                #endregion

                #region Sales/Purchase return and Moving to advance

                if (model.MoveSalesOrPurchaseReturnInvoiceAmountToAdvance &&
                    model.CreditAmount > 0 &&
                    (model.InvoiceTypeNatureID == (int)InvoiceTypeNatures.Sales_Return ||
                    model.InvoiceTypeNatureID == (int)InvoiceTypeNatures.Purchase_Return))
                {
                    int entityID = model.InvoiceTypeNatureID == (int)InvoiceTypeNatures.Sales_Return ? model.CustomerEntityID.Value : model.SupplierEntityID.Value;
                    int ledgerID = await _accounts.GetEntityLedgerID(entityID, CurrentClientID, tran);
                    AccBillToBill accBillToBill = new()
                    {
                        Date = DateTime.UtcNow,
                        ReferenceTypeID = (int)ReferenceTypes.Advance,
                        ReferenceNo = voucherNumber != null ? voucherNumber.JournalNoPrefix + '-' + voucherNumber.JournalNo : null,
                        LedgerID = ledgerID,
                    };
                    //Advance from customer
                    if (model.InvoiceTypeNatureID == (int)InvoiceTypeNatures.Sales_Return)
                    {
                        accBillToBill.Debit = 0;
                        accBillToBill.Credit = model.CreditAmount;
                    }
                    //Advance to supplier
                    if (model.InvoiceTypeNatureID == (int)InvoiceTypeNatures.Purchase_Return)
                    {
                        accBillToBill.Debit = model.CreditAmount;
                        accBillToBill.Credit = 0;
                    }
                    accBillToBill.JournalEntryID = await _dbContext.GetByQueryAsync<int?>($"Select JournalEntryID From AccJournalEntry Where JournalMasterID={journalMaster.JournalMasterID} And LedgerID={ledgerID}", tran);
                    await _dbContext.SaveAsync(accBillToBill, tran);
                }

                #endregion

                #region Invoice payment from advance (AgainstReference in AccBillToBill)

                foreach (var advanceAgainstReference in model.InvoiceBillToBillAgainstReferences.Where(item => item.Amount > 0))
                {
                    int entityID = model.InvoiceTypeNatureID == (int)InvoiceTypeNatures.Sales ? model.CustomerEntityID.Value : model.SupplierEntityID.Value;
                    AccBillToBill accBillToBill = new()
                    {
                        Date = DateTime.UtcNow,
                        ReferenceTypeID = (int)ReferenceTypes.AgainstReference,
                        ReferenceNo = voucherNumber != null ? voucherNumber.JournalNoPrefix + '-' + voucherNumber.JournalNo : null,
                        LedgerID = await _accounts.GetEntityLedgerID(entityID, CurrentClientID, tran),
                        ParentBillID = advanceAgainstReference.BillID,
                    };
                    //Sales recipet from customer advance
                    if (model.InvoiceTypeNatureID == (int)InvoiceTypeNatures.Sales)
                    {
                        accBillToBill.Debit = advanceAgainstReference.Amount;
                        accBillToBill.Credit = 0;
                    }
                    //purchase payment to supplier from advance
                    if (model.InvoiceTypeNatureID == (int)InvoiceTypeNatures.Purchase)
                    {
                        accBillToBill.Debit = 0;
                        accBillToBill.Credit = advanceAgainstReference.Amount;
                    }
                    await _dbContext.SaveAsync(accBillToBill, tran);
                }

                #endregion

                //Ensure the amount for invoice is split into the accounts entry is correct or not

                tran.Commit();
                if (model.GenerateInvoicePdf)
                {
                    await _inventory.GenerateInvoicePdf(invoice.InvoiceID, CurrentBranchID, CurrentClientID);
                    MailDetailsModel quotationMailDetails = await _common.GetInvoicePdfMailDetails(invoice.InvoiceID, CurrentBranchID);
                    quotationMailDetails.ID = invoice.InvoiceID;
                    return Ok(quotationMailDetails);
                }
                else
                {
                    return Ok(new InvoiceSuccessModel() { InvoiceID = invoice.InvoiceID });
                }
            }
        }

        [HttpGet("get-invoice/{invoiceID}")]
        public async Task<IActionResult> GetInvoice(int invoiceID)
        {
            return Ok(await _inventory.GetInvoiceById(invoiceID));
        }

        [HttpGet("generate-invoice-pdf/{invoiceID}")]
        public async Task<IActionResult> GenerateInvoicePdf(int invoiceID)
        {
            PdfGeneratedResponseModel pdfGeneratedResponseModel = new()
            {
                MediaID = await _inventory.GenerateInvoicePdf(invoiceID, CurrentBranchID, CurrentClientID),
                MailDetails = await _common.GetInvoicePdfMailDetails(invoiceID, CurrentBranchID),
            };
            pdfGeneratedResponseModel.MailDetails.ID = invoiceID;
            return Ok(pdfGeneratedResponseModel);
        }

        [HttpGet("get-invoice-pdf/{invoiceID}")]
        public async Task<IActionResult> GetQuotationPDFDetails(int invoiceID)
        {
            StringModel htmlContent = new();
            htmlContent.Value = await _pdf.GetInvoicePdfHtmlContent(invoiceID, CurrentBranchID);
            return Ok(htmlContent);
        }

        [HttpGet("get-invoice-mail-details/{invoiceID}")]
        public async Task<IActionResult> GetInvoicePdfMailDetails(int invoiceID)
        {
            MailDetailsModel invoiceMailDetails = await _common.GetInvoicePdfMailDetails(invoiceID, CurrentBranchID);
            invoiceMailDetails.ID = invoiceID;
            return Ok(invoiceMailDetails);
        }

        [HttpPost("get-invoice-paged-list")]
        public async Task<IActionResult> GetInvoicePagedList(PagedListPostModelWithFilter searchModel)
        {
            PagedListQueryModel query = searchModel;
            query.Select = $@"Select I.InvoiceID,I.InvoiceDate,I.AccountsDate,I.InvoiceNumber,I.Prefix,IT.InvoiceTypeNatureID,vE.Name As Name,IT.InvoiceTypeName,uE.Name As Username
                                From Invoice I
                                Left Join viEntity vE ON vE.EntityID=I.CustomerEntityID
                                Left Join InvoiceType IT ON IT.InvoiceTypeID=I.InvoiceTypeID
                                Left Join viEntity uE ON uE.EntityID=
								Case
									When IT.InvoiceTypeNatureID={(int)InvoiceTypeNatures.Sales} OR IT.InvoiceTypeNatureID={(int)InvoiceTypeNatures.Sales_Return} Then I.CustomerEntityID
									When IT.InvoiceTypeNatureID={(int)InvoiceTypeNatures.Purchase} OR IT.InvoiceTypeNatureID={(int)InvoiceTypeNatures.Purchase_Return} Then I.SupplierEntityID
	                                End";
            query.WhereCondition = $"I.IsDeleted=0 and I.BranchID={CurrentBranchID} And IT.InvoiceTypeNatureID<>{(int)InvoiceTypeNatures.Stock_Adjustment}";
            query.OrderByFieldName = searchModel.OrderByFieldName;
            if (CurrentUserTypeID > (int)UserTypes.Client)
            {
                var invoiceIdList = await _inventory.GetAccessableInvoiceForUser(CurrentEntityID, CurrentBranchID);
                if (!string.IsNullOrEmpty(invoiceIdList))
                    query.WhereCondition += $" and I.InvoiceID IN ({invoiceIdList})";
            }
            query.WhereCondition += _common.GeneratePagedListFilterWhereCondition(searchModel, "vE.Name");
            return Ok(await _dbContext.GetPagedList<InvoiceListModel>(query, null));
        }

        [HttpGet("get-invoice-details/{invoiceID}")]
        public async Task<IActionResult> GetInvoiceDetails(int invoiceID)
        {
            var invoice = await _inventory.GetInvoiceById(invoiceID);
            var invoiceViewModel = _mapper.Map<InvoiceViewModel>(invoice);
            invoiceViewModel.Items = _mapper.Map<List<InvoiceViewItemModel>>(invoice.Items);
            invoiceViewModel.Items
                .ForEach(
                    viewItem => viewItem.Discount +=
                        invoice.Items
                        .Where(item => item.ItemVariantID == viewItem.ItemVariantID)
                        .First().ChargeDiscountDivided
                );
            invoiceViewModel.Username = await _dbContext.GetByQueryAsync<string>($"Select Name From viEntity Where EntityID={invoiceViewModel.UserEntityID}", null);
            return Ok(invoiceViewModel);
        }

        [HttpGet("get-invoice-menu-list")]
        public async Task<IActionResult> GetInvoiceMenuList()
        {
            string whereCondition = $"Where I.IsDeleted=0 and I.BranchID={CurrentBranchID}";
            if (CurrentUserTypeID > (int)UserTypes.Client)
            {
                string commaSeparatedIds = await _inventory.GetAccessableInvoiceForUser(CurrentEntityID, CurrentBranchID);
                if (!string.IsNullOrEmpty(commaSeparatedIds))
                    whereCondition += $" AND I.InvoiceID in ({commaSeparatedIds})";
            }

            var menuList = await _dbContext.GetListByQueryAsync<InvoiceMenuModel>(@$"Select I.InvoiceID,E.Name,I.Prefix,I.InvoiceNumber
                                                                                        From Invoice I
                                                                                        Join viEntity E ON I.CustomerEntityID=E.EntityID
                                                                                        {whereCondition} Order By 1 Desc", null);
            List<ViewPageMenuModel> result = new();
            foreach (var menu in menuList)
            {
                ViewPageMenuModel viewPageMenuModel = new()
                {
                    ID = menu.InvoiceID,
                    MenuName = menu.Name + " (" + menu.Prefix + " - " + menu.InvoiceNumber + ")"
                };

                if (!string.IsNullOrEmpty(viewPageMenuModel.MenuName) && !string.IsNullOrEmpty(menu.Name) && viewPageMenuModel.MenuName.Length > 20)
                    viewPageMenuModel.MenuName = menu.Name.Substring(0, 19) + "..(" + menu.Prefix + " - " + menu.InvoiceNumber + ")";

                result.Add(viewPageMenuModel);
            }
            return Ok(result);
        }

        #endregion

        #region Item Stock Adjustment

        [HttpPost("get-item-stock-paged-list")]
        public async Task<IActionResult> GetItemStockDetails(PagedListPostModel pagedListPostModel)
        {
            PagedListQueryModel queryModel = pagedListPostModel;
            queryModel.Select = @$"Select vI.ItemVariantID,vI.ItemName,SUM(IMS.Quantity) Quantity
                                                From viItem vI
                                                Left Join ItemStock IMS ON vI.ItemVariantID=IMS.ItemVariantID 
                                                Join Branch B ON B.ClientID=vI.ClientID";
            queryModel.WhereCondition = $"B.BranchID={CurrentBranchID} And vI.IsGoods=1";
            if (CurrentUserTypeID == (int)UserTypes.Client)
                queryModel.WhereCondition = $"B.ClientID={CurrentClientID} And vI.IsGoods=1";
            queryModel.GroupBy = "vI.ItemVariantID,vI.ItemName";
            queryModel.OrderByFieldName = "vI.ItemName";
            queryModel.SearchLikeColumnNames = new List<string>() { "vI.ItemName" };
            var itemStockDetails = await _dbContext.GetPagedList<ItemStockViewModel>(queryModel, null);
            return Ok(itemStockDetails);
        }

        [HttpGet("get-item-stock-details/{itemModelID}")]
        public async Task<IActionResult> GetItemStockDetails(int itemModelID)
        {
            var result = await _dbContext.GetByQueryAsync<ItemStockAdjustmentItemModel>(@$"Select I.ItemVariantID,I.ItemName,S.Quantity As SystemStock
                                                                                    From viItem I 
                                                                                    Left Join ItemStock S ON I.ItemVariantID=S.ItemVariantID
                                                                                    Where I.ItemVariantID={itemModelID}", null);
            return Ok(result ?? new());
        }

        [HttpGet("get-item-stock-adjustments-details")]
        public async Task<IActionResult> GetItemStockDetails()
        {
            ItemStockAdjustmentModel stockAdjustmentModel = new();
            //stockAdjustmentModel.InvoiceTypeID = await _dbContext.GetByQueryAsync<int>($"Select Top 1 InvoiceTypeID From InvoiceType Where ClientID={CurrentClientID} And InvoiceTypeNatureID={(int)InvoiceTypeNatures.Stock_Adjustment}", null);
            stockAdjustmentModel.Items = await _dbContext.GetListByQueryAsync<ItemStockAdjustmentItemModel>(@$"Select I.ItemVariantID,I.ItemName,IMS.Quantity As SystemStock
                                                                                From viItem I
                                                                                Left Join ItemStock IMS ON IMS.ItemVariantID=I.ItemVariantID
                                                                                Where I.ClientID={CurrentClientID}", null);
            return Ok(stockAdjustmentModel);
        }

        [HttpPost("save-item-stock-adjustments")]
        public async Task<IActionResult> SaveItemStockAdjustments(ItemStockAdjustmentModel stockAdjustmentModel)
        {
            int invoieTypeID = await _dbContext.GetByQueryAsync<int>($"Select Top 1 InvoiceTypeID From InvoiceType Where ClientID={CurrentClientID} And InvoiceTypeNatureID={(int)InvoiceTypeNatures.Stock_Adjustment}", null);
            //Invoice entry
            Invoice stockAdjustmentInvoice = new()
            {
                InvoiceDate = stockAdjustmentModel.Date,
                InvoiceTypeID = invoieTypeID,
                Prefix = await _dbContext.GetByQueryAsync<string>($"Select Prefix From InvoiceType Where InvoiceTypeID={invoieTypeID}", null),
                InvoiceNumber = await _dbContext.GetByQueryAsync<int>($"Select IsNull(Max(InvoiceNumber),0)+1 From Invoice Where InvoiceTypeID={invoieTypeID} And BranchID={CurrentBranchID}", null),
                BranchID = CurrentBranchID
            };
            stockAdjustmentInvoice.InvoiceID = await _dbContext.SaveAsync(stockAdjustmentInvoice);

            foreach (var item in stockAdjustmentModel.Items)
            {
                //Item stock difference in invoice item
                InvoiceItem stockAdjustmentInvoiceItem = new()
                {
                    InvoiceID = stockAdjustmentInvoice.InvoiceID,
                    ItemVariantID = item.ItemVariantID,
                };

                //opening stock case
                if (item.SystemStock == 0 && item.PhysicalStock > 0)
                    stockAdjustmentInvoiceItem.Quantity = item.PhysicalStock;
                //stock adjustment case
                if (item.SystemStock > 0)
                    stockAdjustmentInvoiceItem.Quantity = item.PhysicalStock - item.SystemStock;

                stockAdjustmentInvoiceItem.InvoiceItemID = await _dbContext.SaveAsync(stockAdjustmentInvoiceItem);

                //Item stock adjustment insrtion
                PB.Shared.Tables.Inventory.Items.ItemStockAdjustment itemStockAdjustment = new()
                {
                    InvoiceItemID = stockAdjustmentInvoiceItem.InvoiceItemID,
                    SystemStock = item.SystemStock,
                    PhysicalStock = item.PhysicalStock,
                };
                itemStockAdjustment.StockAdjustmentID = await _dbContext.SaveAsync(itemStockAdjustment);

                //Item Stock updation
                var itemStock = await _dbContext.GetByQueryAsync<ItemStock>($"Select * From ItemStock Where ItemVariantID={item.ItemVariantID} and BranchID={CurrentBranchID}", null);
                itemStock ??= new();
                itemStock.ItemVariantID = item.ItemVariantID;
                itemStock.BranchID = CurrentBranchID;
                //opening stock case
                if (item.SystemStock == 0 && item.PhysicalStock > 0)
                    itemStock.Quantity = item.PhysicalStock;
                //stock adjustment case
                if (item.SystemStock > 0)
                    itemStock.Quantity += stockAdjustmentInvoiceItem.Quantity;

                itemStock.StockID = await _dbContext.SaveAsync(itemStock);
            }

            return Ok(new StockAdjustmentSuccessModel()
            {
                InvoieID = stockAdjustmentInvoice.InvoiceID,
                ResponseTitle = "Success",
                ResponseMessage = "StockAdjustSuccess"
            });
        }

        #endregion

        #region Invoice payment term slab

        [HttpGet("get-invoice-payment-term-slabls/{paymentTermID}")]
        public async Task<IActionResult> GetInvoicePaymentSlab(int paymentTermID)
        {
            var res = await _dbContext.GetListByQueryAsync<InvoicePaymentTermSlabModel>($@"Select SlabID,PaymentTermID,Day as Days,Percentage,SlabName
                                                                                            From PaymentTermSlab
                                                                                            Where PaymentTermID={paymentTermID} and IsDeleted=0", null);
            return Ok(res ?? new());
        }



        #endregion
    }
}
