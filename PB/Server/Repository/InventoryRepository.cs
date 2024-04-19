using AutoMapper;
using NPOI.HPSF;
using PB.EntityFramework;
using PB.Model;
using PB.Shared.Enum;
using PB.Shared.Helpers;
using PB.Shared.Models.Inventory;
using PB.Shared.Models.Inventory.Invoice;
using PB.Shared.Models.Inventory.Invoices;
using PB.Shared.Models.Inventory.InvoiceType;
using PB.Shared.Tables;
using PB.Shared.Tables.Accounts.JournalMaster;
using PB.Shared.Tables.Accounts.Ledgers;
using PB.Shared.Tables.Accounts.VoucherTypes;
using PB.Shared.Tables.Inventory.Invoices;
using PB.Shared.Tables.Inventory.InvoiceType;
using PB.Shared.Tables.Inventory.Items;
using PB.Shared.Tables.Tax;
using System.Data;

namespace PB.Server.Repository
{
    public interface IInventoryRepository
    {
        #region InvoiceType
        Task<int> InsertInvoicetypeEntry(InvoiceTypeModel invoiceTpye, IDbTransaction? tran = null);
        Task<InvoiceTypeModel> GetInvoiceType(int invoiceTypeId, int ClientId, IDbTransaction? tran = null);
        Task<InvoiceTypeSelectedDetailsModel> GetInvoiceTypeDetailsById(int invoiceTypeID);
        Task<List<InvoiceTypeChargeModel>> GetInvoiceTypeCharges(int invoiceTypeId, IDbTransaction? tran = null);
        Task DeleteInvoiceType(int invoiceTypeId, int ClientId, IDbTransaction? tran = null);

        #endregion

        #region DefaultInvoiceType
        //Task InsertClientDefaultInvoiceTypeEntries(int clientID, IDbTransaction? tran = null);
        Task InsertClientInventoryDefaultEntries(int clientID, IDbTransaction? tran = null);
        Task<int> InsertSingleItemSaleEntry(int clientID, int customerEntityID, int itemModelID, bool isIntraTaxCategory = true, IDbTransaction? tran = null);

        #endregion

        #region Invoince
        Task<InvoiceModel> GetInvoiceById(int invoiceID);
        Task<InvoiceItemModel> GetInvoiceItemById(int invoiceItemID);
        InvoiceItemModel HandleInvoiceItemCalculation(InvoiceItemModel invoiceItem);
        Task<string> GetAccessableInvoiceForUser(int userEntityID, int currentBranchID);
        Task<int> GenerateInvoicePdf(int invoiceID, int branchID, int clientID);

        #endregion
    }
    public class InventoryRepository : IInventoryRepository
    {
        private readonly IDbContext _dbContext;
        private readonly IAccountRepository _accounts;
        private readonly IMapper _mapper;
        private readonly IDbConnection _cn;
        private readonly ISuperAdminRepository _supperAdmin;
        private readonly IPDFRepository _pdf;
        private readonly IHostEnvironment _env;

        public InventoryRepository(IDbContext dbContext, IAccountRepository accounts, IMapper mapper, IDbConnection cn, ISuperAdminRepository supperAdmin, IPDFRepository pdf, IHostEnvironment env)
        {
            this._dbContext = dbContext;
            this._accounts = accounts;
            this._mapper = mapper;
            this._cn = cn;
            this._supperAdmin = supperAdmin;
            this._pdf = pdf;
            this._env = env;
        }

        #region InvoiceType

        public async Task<int> InsertInvoicetypeEntry(InvoiceTypeModel invoiceType, IDbTransaction? tran = null)
        {

            InvoiceType invoiceTypes = new()
            {
                InvoiceTypeID = invoiceType.InvoiceTypeID,
                InvoiceTypeName = invoiceType.InvoiceTypeName,
                InvoiceTypeNatureID = invoiceType.InvoiceTypeNatureID,
                Prefix = invoiceType.Prefix,
                LedgerID = invoiceType.LedgerID,
                ClientID = invoiceType.ClientID,
                VoucherTypeID = invoiceType.VoucherTypeID
            };
            invoiceTypes.InvoiceTypeID = await _dbContext.SaveAsync(invoiceTypes, tran);

            List<InvoiceTypeCharge> ChargeList = new();
            if (invoiceType.InvoiceCharge != null)
            {
                foreach (var charge in invoiceType.InvoiceCharge)
                {
                    InvoiceTypeCharge charges = new()
                    {
                        InvoiceTypeChargeID = charge.InvoiceTypeChargeID,
                        ChargeName = charge.ChargeName,
                        OrderNumber = charge.OrderNumber,
                        ClientID = invoiceType.ClientID.Value,
                        InvoiceTypeID = invoiceTypes.InvoiceTypeID,
                        ChargeNature = charge.ChargeNature,
                        ChargeOperation = charge.ChargeOperation,
                        ChargeCalculation = charge.ChargeCalculation,
                        ChargeEffect = charge.ChargeEffect,
                        LedgerID = charge.LedgerID

                    };
                    ChargeList.Add(charges);
                }
                await _dbContext.SaveListAsync(ChargeList, "", tran);
            }


            return invoiceTypes.InvoiceTypeID;
        }
        public async Task<InvoiceTypeModel> GetInvoiceType(int invoiceTypeId, int ClientId, IDbTransaction? tran = null)
        {
            InvoiceTypeModel result = new();
            result = await _dbContext.GetByQueryAsync<InvoiceTypeModel>($@"Select * From InvoiceType IT
                                                                        Join AccVoucherType VT on VT.VoucherTypeID=IT.VoucherTypeID and VT.IsDeleted=0
                                                                        Join AccVoucherTypeNature VN on VN.VoucherTypeNatureID=VT.VoucherTypeNatureID and VN.IsDeleted=0
                                                                        Join AccVoucherTypeSetting VS on VS.VoucherTypeID=VT.VoucherTypeID and VS.IsDeleted=0
                                                                        Join AccLedger L on L.ledgerID=IT.LedgerID and L.IsDeleted=0
                                                                        Where IT.InvoiceTypeID={invoiceTypeId} and IT.ClientID={ClientId} and IT.isdeleted=0", null, tran);

            result.InvoiceCharge = await GetInvoiceTypeCharges(invoiceTypeId, tran);
            return result;
        }
        public async Task<InvoiceTypeSelectedDetailsModel> GetInvoiceTypeDetailsById(int invoiceTypeID)
        {
            InvoiceTypeSelectedDetailsModel details = await _dbContext.GetByQueryAsync<InvoiceTypeSelectedDetailsModel>(@$"Select InvoiceTypeID,InvoiceTypeName,Prefix,InvoiceTypeNatureID
                                                                                                                            From InvoiceType 
                                                                                                                            Where InvoiceTypeID={invoiceTypeID}", null);

            details.ExtraChargeItems = await _dbContext.GetListByQueryAsync<InvoiceChargeModel>(@$"Select ITC.* ,L.LedgerName
                                                                                                            From InvoiceTypeCharge ITC
                                                                                                            Left Join AccLedger L ON L.LedgerID=ITC.LedgerID And L.IsDeleted=0
                                                                                                            Where ITC.InvoiceTypeID={invoiceTypeID} And ITC.IsDeleted=0", null);
            return details;
        }
        public async Task<List<InvoiceTypeChargeModel>> GetInvoiceTypeCharges(int invoiceTypeId, IDbTransaction? tran = null)
        {
            return await _dbContext.GetListByQueryAsync<InvoiceTypeChargeModel>($@"Select IC.*,LedgerName From InvoiceType IT
                                                                                                    Join InvoiceTypeCharge IC on IC.InvoiceTypeID=IT.InvoiceTypeID and IC.IsDeleted=0
                                                                                                    Join AccLedger AL on AL.LedgerID=IC.LedgerID and AL.IsDeleted=0
                                                                                                    Where IT.InvoiceTypeID={invoiceTypeId} and IT.IsDeleted=0", null);
        }
        public async Task DeleteInvoiceType(int invoiceTypeId, int ClientId, IDbTransaction? tran = null)
        {
            var invoiceType = await _dbContext.GetAsync<InvoiceType>(invoiceTypeId);

            await _dbContext.DeleteAsync<AccVoucherTypeSetting>(invoiceType.VoucherTypeID.Value, tran);
            await _dbContext.DeleteAsync<AccVoucherType>(invoiceType.VoucherTypeID.Value, tran);
            await _dbContext.DeleteAsync<InvoiceType>(invoiceTypeId, tran);
            //await _dbContext.DeleteAsync<InvoiceTypeCharge>(invoiceTypeId, tran);
            await _dbContext.ExecuteAsync($@"UPDATE InvoiceTypeCharge
                                                SET IsDeleted=1
                                                Where InvoiceTypeID={invoiceTypeId}", null, tran);

        }

        #endregion

        #region DefaultinvoiceType

        public async Task InsertClientInventoryDefaultEntries(int clientID, IDbTransaction? tran = null)
        {
            //Invoice voucher types
            List<AccVoucherType> invoiceVoucherTypes = new()
            {
                new AccVoucherType()
                {
                    VoucherTypeName=VoucherTypeNatures.Sales.ToString(),
                    VoucherTypeNatureID=(int)VoucherTypeNatures.Sales,
                },
                new AccVoucherType()
                {
                    VoucherTypeName="Sales Return",
                    VoucherTypeNatureID=(int)VoucherTypeNatures.SalesReturn,
                },
                new AccVoucherType()
                {
                    VoucherTypeName=VoucherTypeNatures.Purchase.ToString(),
                    VoucherTypeNatureID=(int)VoucherTypeNatures.Purchase,
                },
                new AccVoucherType()
                {
                    VoucherTypeName="Purchase Return",
                    VoucherTypeNatureID=(int)VoucherTypeNatures.PurchaseReturn,
                },
            };
            await _dbContext.SaveSubItemListAsync(invoiceVoucherTypes, "ClientID", clientID, tran, false);

            var vouchers = await _dbContext.GetListByQueryAsync<AccVoucherType>($"Select * From AccVoucherType Where ClientID={clientID} And IsDeleted=0And VoucherTypeNatureID >= {(int)VoucherTypeNatures.Sales} ", null, tran);
            if (vouchers != null)
            {
                //Invoice voucher type settings
                var branches = await _dbContext.GetListAsync<BranchCustom>($"ClientID={clientID}", null, tran);
                foreach (var branch in branches)
                {
                    List<AccVoucherTypeSetting> invoiceVoucherTypeSettings = new()
                    {
                        new AccVoucherTypeSetting()
                        {
                            PeriodicityID=(int)VoucherNumberingPeriodicity.Continuous,
                            NumberingTypeID=(int)VoucherNumberingTypes.Automatic_Manual_Override,
                            StartingNumber=1,
                            Prefix="S",
                            VoucherTypeID=vouchers.Where(s=>s.VoucherTypeNatureID==(int)VoucherTypeNatures.Sales).First().VoucherTypeID
                        },
                        new AccVoucherTypeSetting()
                        {
                            PeriodicityID=(int)VoucherNumberingPeriodicity.Continuous,
                            NumberingTypeID=(int)VoucherNumberingTypes.Automatic_Manual_Override,
                            StartingNumber=1,
                            Prefix="SR",
                            VoucherTypeID=vouchers.Where(s=>s.VoucherTypeNatureID==(int)VoucherTypeNatures.SalesReturn).First().VoucherTypeID
                        },
                        new AccVoucherTypeSetting()
                        {
                            PeriodicityID=(int)VoucherNumberingPeriodicity.Continuous,
                            NumberingTypeID=(int)VoucherNumberingTypes.Automatic_Manual_Override,
                            StartingNumber=1,
                            Prefix="P",
                            VoucherTypeID=vouchers.Where(s=>s.VoucherTypeNatureID==(int)VoucherTypeNatures.Purchase).First().VoucherTypeID
                        },
                        new AccVoucherTypeSetting()
                        {
                            PeriodicityID=(int)VoucherNumberingPeriodicity.Continuous,
                            NumberingTypeID=(int)VoucherNumberingTypes.Automatic_Manual_Override,
                            StartingNumber=1,
                            Prefix="PR",
                            VoucherTypeID=vouchers.Where(s=>s.VoucherTypeNatureID==(int)VoucherTypeNatures.PurchaseReturn).First().VoucherTypeID
                        }
                    };
                    await _dbContext.SaveSubItemListAsync(invoiceVoucherTypeSettings, "BranchID", branch.BranchID, tran, false);
                }

                //Invoice type 
                var ledgers = await _dbContext.GetListAsync<AccLedger>($"ClientID={clientID} And IsDeleted=0", null, tran);
                if (ledgers != null)
                {
                    List<InvoiceType> invoiceTypes = new()
                    {
                        new()
                        {
                            InvoiceTypeName = vouchers.Where(v => v.VoucherTypeNatureID == (int)VoucherTypeNatures.Sales).First().VoucherTypeName,
                            InvoiceTypeNatureID = (int)InvoiceTypeNatures.Sales,
                            Prefix = "S",
                            VoucherTypeID = vouchers.Where(v => v.VoucherTypeNatureID == (int)VoucherTypeNatures.Sales).First().VoucherTypeID,
                        },
                        new()
                        {
                            InvoiceTypeName = vouchers.Where(v => v.VoucherTypeNatureID == (int)VoucherTypeNatures.SalesReturn).First().VoucherTypeName,
                            InvoiceTypeNatureID = (int)InvoiceTypeNatures.Sales_Return,
                            Prefix = "SR",
                            VoucherTypeID = vouchers.Where(v => v.VoucherTypeNatureID == (int)VoucherTypeNatures.SalesReturn).First().VoucherTypeID,
                        },
                        new()
                        {
                            InvoiceTypeName = vouchers.Where(v => v.VoucherTypeNatureID == (int)VoucherTypeNatures.Purchase).First().VoucherTypeName,
                            InvoiceTypeNatureID = (int)InvoiceTypeNatures.Purchase,
                            Prefix = "P",
                            VoucherTypeID = vouchers.Where(v => v.VoucherTypeNatureID == (int)VoucherTypeNatures.Purchase).First().VoucherTypeID,
                        },
                        new()
                        {
                            InvoiceTypeName = vouchers.Where(v => v.VoucherTypeNatureID == (int)VoucherTypeNatures.PurchaseReturn).First().VoucherTypeName,
                            InvoiceTypeNatureID = (int)InvoiceTypeNatures.Purchase_Return,
                            Prefix = "PR",
                            VoucherTypeID = vouchers.Where(v => v.VoucherTypeNatureID == (int)VoucherTypeNatures.PurchaseReturn).First().VoucherTypeID,
                        },
                        new()
                        {
                            InvoiceTypeName = InvoiceTypeNatures.Stock_Adjustment.ToString().Replace('_',' '),
                            InvoiceTypeNatureID = (int)InvoiceTypeNatures.Stock_Adjustment,
                            Prefix = "SA",
                            VoucherTypeID = null,
                            LedgerID = null
                        }
                    };
                    await _dbContext.SaveSubItemListAsync(invoiceTypes, "ClientID", clientID, tran, false);
                }
            }
        }
        public async Task<int> InsertSingleItemSaleEntry(int clientID, int customerEntityID, int itemModelID, bool isIntraTaxCategory = true, IDbTransaction? tran = null)
        {
            int branchID = await _dbContext.GetByQueryAsync<int>($"Select Top 1 BranchID from Branch Where ClientID={clientID} and IsDeleted=0", tran);
            var salesInvoiceType = await _dbContext.GetAsync<InvoiceType>($@"InvoiceTypeNatureID={InvoiceTypeNatures.Sales} and ClientID={clientID}", tran);
            var bookLedgerID = await _dbContext.GetByQueryAsync<int?>($@"Select LedgerID From AccLedger Where LedgerName={PDV.SalesLedgerName} and ClientID={clientID}", tran);
            var partyLedgerID = await _accounts.GetEntityLedgerID(customerEntityID, clientID, tran);
            var voucherNumber = await _accounts.GetVoucherNumber(salesInvoiceType.VoucherTypeID.Value, branchID, tran);
            var bankLedgerID = await _dbContext.GetByQueryAsync<int?>($@"Select LedgerID From AccLedger Where LedgerName={PDV.BankLedgerName} and ClientID={clientID}", tran);

            #region Amount

            ItemVariant itemModel = await _dbContext.GetAsync<ItemVariant>(itemModelID, tran);
            decimal totalAmount = Convert.ToDecimal(itemModel.Price) * itemModel.UMUnit;
            decimal netAmount = totalAmount; //becuase no discount is applying
            decimal taxAmount = 0;
            decimal grossAmount = 0;

            int ItemTaxCategoryType = isIntraTaxCategory ? 1 : 0;
            var taxData = await _dbContext.GetByQueryAsync<TaxDetailsModel>(@$"SELECT T.TaxCategoryID,T.TaxPercentage
                                                                                From viItem I
                                                                                Left Join viTaxCategory T ON T.TaxCategoryID=
                                                                                CASE
                                                                                    WHEN 1={ItemTaxCategoryType} THEN I.InterTaxcategoryID
                                                                                    WHEN 1={ItemTaxCategoryType} THEN I.InterTaxCategoryID
	                                                                                END
                                                                                WHERE I.ItemVariantID={itemModelID}", null, tran);

            if (taxData is not null)
            {
                taxData.TaxCategoryItems = await _dbContext.GetListByQueryAsync<TaxCategoryItem>(@$"SELECT * FROM TaxCategoryItem WHERE TaxCategoryID={taxData.TaxCategoryID}", null, tran);
                taxAmount = netAmount * taxData.TaxPercentage / 100;
                grossAmount = netAmount + taxAmount;
            }

            #endregion

            #region AccJournalMaster And Journal Entry

            AccJournalMaster journalMaster = new()
            {
                JournalMasterID = 0,
                Date = DateTime.Now.Date,
                BranchID = branchID,
                VoucherTypeID = salesInvoiceType.VoucherTypeID,
                JournalNoPrefix = voucherNumber.JournalNoPrefix,
                JournalNo = voucherNumber.JournalNo,
                EntityID = customerEntityID,
            };
            journalMaster.JournalMasterID = await _dbContext.SaveAsync(journalMaster, tran);

            List<AccJournalEntry> journalEntries = new();

            //Debit
            AccJournalEntry bankLedgerEntry = new()
            {
                JournalEntryID = 0,
                JournalMasterID = journalMaster.JournalMasterID,
                LedgerID = bankLedgerID,
                Debit = grossAmount
            };
            journalEntries.Add(bankLedgerEntry);

            //if there is discount then add discount entry in Debit section
            //--
            //--
            //--

            //Credit
            AccJournalEntry salesLedgerEntry = new()
            {
                JournalEntryID = 0,
                JournalMasterID = journalMaster.JournalMasterID,
                LedgerID = bookLedgerID,
                Credit = totalAmount
            };
            journalEntries.Add(salesLedgerEntry);

            if (taxData is not null && taxData.TaxCategoryItems is not null)
            {
                //Tax entries
                foreach (var taxItem in taxData.TaxCategoryItems)
                {
                    AccJournalEntry taxCategoryItem = new()
                    {
                        JournalEntryID = 0,
                        JournalMasterID = journalMaster.JournalMasterID,
                        LedgerID = taxItem.LedgerID,
                        Credit = netAmount * taxItem.Percentage / 100
                    };
                    journalEntries.Add(taxCategoryItem);
                }
            }

            if (journalEntries.Count > 0)
                await _dbContext.SaveSubItemListAsync(journalEntries, "JournalMasterID", journalMaster.JournalMasterID, tran);

            #endregion

            //#region Invoice

            //var invoice = new Invoice()
            //{
            //    InvoiceID = 0,
            //    InvoiceTypeID = salesInvoiceType.InvoiceTypeID,
            //    BillDate = DateTime.Now.Date,
            //    AccountDate = DateTime.Now.Date,
            //    InvoiceNo = voucherNumber.JournalNo,
            //    Prefix = voucherNumber.JournalNoPrefix,
            //    BookLedgerID = bookLedgerID,
            //    PartyLedgerID = partyLedgerID,
            //    BranchID = branchID,
            //    JournalMasterID = journalMaster.JournalMasterID,
            //};
            //invoice.InvoiceID = await _dbContext.SaveAsync(invoice, tran);

            //#endregion

            return journalMaster.JournalMasterID;

        }

        #endregion

        #region Invoince

        public async Task<InvoiceModel> GetInvoiceById(int invoiceID)
        {
            InvoiceModel invoiceModel = await _dbContext.GetByQueryAsync<InvoiceModel>(@$"Select I.*,IT.InvoiceTypeName,CE.Name As CustomerName,CE.TaxNumber,B.BranchName,IT.InvoiceTypeNatureID,
                                                                                            Concat(CR.CurrencyName,' (',CR.Symbol,')') As CurrencyName,Concat('[',CS.StateCode,'] - ',CS.StateName) As PlaceOfSupplyName,
                                                                                            M.FileName,PaymentTermName,B.ClientID
                                                                                            From Invoice I
                                                                                            Join InvoiceType IT ON IT.InvoiceTypeID=I.InvoiceTypeID and IT.IsDeleted=0
                                                                                            Join viEntity CE ON CE.EntityID=I.CustomerEntityID
                                                                                            Join viBranch B ON B.BranchID=I.BranchID
                                                                                            Join Currency CR ON CR.CurrencyID=I.CurrencyID and CR.IsDeleted=0
                                                                                            Join CountryState CS ON CS.StateID=I.PlaceOfSupplyID and CS.IsDeleted=0
                                                                                            Left Join PaymentTerm PT on PT.PaymentTermID=I.PaymentTermID and PT.IsDeleted=0
                                                                                            Left Join AccJournalMaster Acc on Acc.JournalMasterID=I.JournalMasterID and Acc.IsDeleted=0
                                                                                            Left Join Media M ON M.MediaID=I.MediaID and M.IsDeleted=0
                                                                                            Where I.InvoiceID={invoiceID} and I.IsDeleted=0", null);

            invoiceModel.Items = await _dbContext.GetListByQueryAsync<InvoiceItemModel>(@$"Select II.*,I.ItemName,I.TaxPreferenceTypeID,I.TaxPreferenceName,T.TaxCategoryName
                                                            From InvoiceItem II
                                                            Left Join viItem I ON I.ItemVariantID=II.ItemVariantID
                                                            Left Join viTaxCategory T ON T.TaxCategoryID=II.TaxCategoryID
                                                            Where II.InvoiceID={invoiceID} And II.IsDeleted=0", null);

            invoiceModel.InvoiceMailReceipients = await _dbContext.GetListAsync<InvoiceMailReceipient>($"InvoiceID={invoiceID} And IsDeleted=0", null);

            invoiceModel.InvoiceExtraChargeItems = await _dbContext.GetListByQueryAsync<InvoiceChargeModel>(@$"Select IC.*,ITC.ChargeName,ITC.OrderNumber,ITC.ChargeNature,ITC.ChargeOperation,ITC.ChargeCalculation,ITC.ChargeEffect,ITC.LedgerID,L.LedgerName
                                                            From InvoiceCharge IC
                                                            Join InvoiceTypeCharge ITC ON ITC.InvoiceTypeChargeID=IC.InvoiceTypeChargeID
                                                            Join AccLedger L ON L.LedgerID=ITC.LedgerID
                                                            Where IC.InvoiceID={invoiceID} And IC.IsDeleted=0", null);
            invoiceModel.PaymentSlabs = await _dbContext.GetListByQueryAsync<InvoicePaymentTermSlabModel>(@$"Select ID,IP.SlabID,InvoiceID,Days,IP.Percentage,IP.Amount,SlabName,PT.PaymentTermID
                                                                                                                From InvoicePaymentTermSlab IP
                                                                                                                Left Join PaymentTermSlab P on P.SlabID=IP.SlabID and P.IsDeleted=0
                                                                                                                Left Join PaymentTerm PT on PT.PaymentTermID=P.PaymentTermID and PT.IsDeleted=0
                                                                                                                Where InvoiceID={invoiceID} and IP.IsDeleted=0", null);
            invoiceModel.InvoicePaymentsList = await _dbContext.GetListByQueryAsync<InvoicePaymentsModel>($@"Select JournalEntryID,AE.LedgerID,LedgerName,AT.GroupTypeID as AccountGroupTypeID,Debit as Amount
                                                                                                        From AccJournalEntry AE
                                                                                                        Left Join AccLedger AL on AL.LedgerID=AE.LedgerID and AL.IsDeleted=0
                                                                                                        Left Join AccAccountGroup AG on AG.AccountGroupID=AL.AccountGroupID and AG.IsDeleted=0
                                                                                                        Left Join AccAccountGroup AT on AT.GroupTypeID=AG.GroupTypeID and AG.IsDeleted=0
                                                                                                        Where JournalMasterID=@JournalMasterID  and AT.ClientID={invoiceModel.ClientID} and AE.IsDeleted=0", new { JournalMasterID = invoiceModel.JournalMasterID });

            invoiceModel.InvoiceAssignees = await _dbContext.GetListAsync<InvoiceAssignee>($"InvoiceID={invoiceID} And IsDeleted=0", null);
            invoiceModel.NeedShippingAddress = invoiceModel.ShippingAddressID is not null ? true : false;
            return invoiceModel;
        }
        private async Task<List<InvoiceItemModel>> GetInvoiceItems(int invoiceID)
        {
            List<InvoiceItemModel> invoiceItems = new();
            List<int> invoiceItemIDs = await _dbContext.GetListFieldsAsync<InvoiceItem, int>("InvoiceItemID", $"InvoiceID={invoiceID} And IsDeleted=0", null);
            foreach (var invoiceItemID in invoiceItemIDs)
            {
                var invoiceItem = await GetInvoiceItemById(invoiceItemID);
                invoiceItem = HandleInvoiceItemCalculation(invoiceItem);
                invoiceItems.Add(invoiceItem);
            }
            return invoiceItems;
        }
        private async Task<List<InvoiceMailReceipient>> GetInvoiceMailReceipients(int invoiceID)
        {
            return await _dbContext.GetListAsync<InvoiceMailReceipient>($"InvoiceID={invoiceID} And IsDeleted=0", null);
        }
        public async Task<InvoiceItemModel> GetInvoiceItemById(int invoiceItemID)
        {
            return await _dbContext.GetByQueryAsync<InvoiceItemModel>($@"Select II.*,I.ItemName,I.TaxPreferenceTypeID,I.TaxPreferenceName,T.TaxCategoryName
                                                            From InvoiceItem II
                                                            Join viItem I ON I.ItemVariantID=II.ItemVariantID
                                                            Join viTaxCategory T ON T.TaxCategoryID=II.TaxCategoryID
                                                            Where II.InvoiceItemID={invoiceItemID} And II.IsDeleted=0", null);
        }
        public InvoiceItemModel HandleInvoiceItemCalculation(InvoiceItemModel invoiceItem)
        {
            invoiceItem.TotalAmount = Convert.ToDecimal(invoiceItem.Quantity) * invoiceItem.Rate;
            invoiceItem.NetAmount = invoiceItem.TotalAmount - invoiceItem.Discount;
            invoiceItem.TaxAmount = invoiceItem.NetAmount * (invoiceItem.TaxPercentage / 100);
            invoiceItem.GrossAmount = invoiceItem.NetAmount + invoiceItem.TaxAmount;
            if (invoiceItem.TaxCategoryItems.Count > 0)
            {
                invoiceItem.TaxCategoryItems
                                .ForEach(taxCategoryItem => taxCategoryItem.TaxAmount = Math.Round((invoiceItem.NetAmount * (taxCategoryItem.Percentage / 100)), 2));
            }
            return invoiceItem;
        }
        public async Task<string> GetAccessableInvoiceForUser(int userEntityID, int currentBranchID)
        {
            List<int> accessibleInvoices = await _dbContext.GetListByQueryAsync<int>($@"SELECT I.InvoiceID
                                                                        FROM Invoice I 
                                                                        LEFT JOIN InvoiceAssignee IA ON I.InvoiceID = IA.InvoiceID AND IA.IsDeleted=0 
                                                                        WHERE (I.InvoiceID IS NULL OR IA.EntityID={userEntityID}) AND I.BranchID={currentBranchID} AND I.IsDeleted=0
                ", null);
            if (accessibleInvoices.Count > 0)
                return (string.Join(',', accessibleInvoices));
            else
                return "";
        }
        public async Task<int> GenerateInvoicePdf(int invoiceID, int branchID, int clientID)
        {
            Random random = new Random();
            int randomNumber = random.Next(0, 100);
            var invoiceNumber = await _dbContext.GetByQueryAsync<IdnValuePair>($"Select InvoiceNumber As ID,Prefix As Value From Invoice Where InvoiceID={invoiceID}", null);
            string pdfName = "pdf_invoice_" + branchID + '_' + invoiceNumber.Value + '_' + invoiceNumber.ID + '_' + randomNumber;
            int invoiceMediaID = await _pdf.CreateInvoicePdf(invoiceID, branchID, clientID, pdfName);
            await RemoveInvoicePdfFiles(invoiceID, branchID, invoiceNumber);
            return invoiceMediaID;
        }
        private async Task RemoveInvoicePdfFiles(int invoiceID, int branchID, IdnValuePair invoiceNumber)
        {
            string actualFolderPath = Path.Combine(_env.ContentRootPath, "wwwroot", "gallery", "invoice");
            string searchLikePattern = "pdf_invoice_" + branchID + '_' + invoiceNumber.Value + '_' + invoiceNumber.ID + "_*";
            string currentFileName = await _dbContext.GetByQueryAsync<string>(@$"Select FileName
                                                                                From Invoice I
                                                                                Left Join Media M ON I.MediaID=M.MediaID
                                                                                Where I.InvoiceID={invoiceID}", null);
            if (!string.IsNullOrEmpty(currentFileName))
            {
                var filesToRemove = Directory.GetFiles(actualFolderPath, searchLikePattern).ToList();
                if (filesToRemove is not null && filesToRemove.Count > 0)
                {
                    int lastIndexOfSeparator = currentFileName.LastIndexOf('/');
                    if (lastIndexOfSeparator != -1)
                        currentFileName = currentFileName.Substring(lastIndexOfSeparator + 1);
                    currentFileName = Path.Combine(_env.ContentRootPath, "wwwroot", "gallery", "invoice", currentFileName);
                    filesToRemove.Remove(filesToRemove.Where(fileName => fileName == currentFileName).First());
                    if (filesToRemove.Count > 0)
                    {
                        foreach (var fileToRemove in filesToRemove)
                        {
                            File.Delete(fileToRemove);
                        }
                    }
                }
            }
        }

        #endregion


    }
}
