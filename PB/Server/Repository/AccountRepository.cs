using PB.DatabaseFramework;
using PB.Model;
using PB.Shared.Helpers;
using System.Data;
using PB.Shared.Enum;
using PB.Shared.Models;
using PB.Shared.Tables.Accounts.AccountGroups;
using PB.Shared.Tables.Accounts.Ledgers;
using PB.Shared.Tables.Accounts.VoucherTypes;
using PB.Client.Pages.Settings;
using PB.Shared.Models.Accounts.Ledgers;
using PB.Shared.Tables;
using AutoMapper;
using Hangfire.Annotations;
using NPOI.OpenXmlFormats;
using PB.Client.Pages.Accounts.AccountGroups;
using PB.Shared.Models.Accounts;
using PB.Shared.Tables.Accounts.JournalMaster;
using PB.Model.Models;
using PB.Shared.Models.Common;
using PB.Client.Pages.SuperAdmin;
using PB.Shared.Models.Accounts.AccountGroups;
using PB.Client.Pages.Accounts.Ledgers;
using PB.Shared.Models.Accounts.VoucherTypes;
using PB.EntityFramework;
using PB.Client.Pages.SuperAdmin.Client;
using PB.Shared.Models.Reports;
using Microsoft.Extensions.Logging;
using PB.Shared.Tables.Tax;
using PB.Shared.Enum.Accounts;
using PB.Shared.Models.Inventory.Invoices;

namespace PB.Server.Repository
{
    public interface IAccountRepository
    {

        #region AccAccountGroup

        Task<int> InsertAccAccountGroup(AccountGroupModel accountGroupModel, int userID, IDbTransaction? tran = null);
        Task<AccountGroupModel> GetAccAccountGroup(int accountGroupID, int clientID, int branchID);

        #endregion

        #region AccLedger

        Task<int> InsertAccLedger(AccLedgerModel ledgerModel, int currentUserID, IDbTransaction? tran = null);
        Task<AccLedgerModel> GetAccLedger(int ledgerID, int clientID, int branchID, IDbTransaction? tran = null);
        Task<int> GetEntityLedgerID(int entityID, int clientID, IDbTransaction? tran = null);
        Task<bool> DeleteAccLedger(int ledgerID, int userID, IDbTransaction? tran = null);
        Task<string?> GenerateLedgerSearchWhereCondition(LedgerSearchModel searchModel, IDbTransaction? tran = null);

        #endregion

        #region AccVoucherType

        Task<int> InsertAccVoucherType(VoucherTypeModel voucherTypeModel, IDbTransaction? tran = null);

        #endregion

        #region AccVoucherTypeSetting



        #endregion

        #region Other Accounts Related Functions

        Task InsertClientDefaultAccountsRelatedEntries(int clientID, IDbTransaction? tran = null);
        Task<int> GetOpeningBalanceEntryJournalMasterID(int branchID, IDbTransaction? tran = null);
        Task<VoucherNumberModel> GetVoucherNumber(int voucherTypeId, int branchID, IDbTransaction? tran = null);

        #endregion

        #region Tax Category

        Task InsertDefaultTaxCategories(int clientID, Countries countryId, IDbTransaction? tran = null);
        Task<int?> GetTaxCategoryItemLedgerID(int taxCategoryItemID, int clientID, IDbTransaction? tran = null);

        #endregion

        #region Balance Sheet

        Task<BalancesheetReportModel> GetBalancesheet(BalanceSheetReportPostModel balanceSheetReportPostModel, int clientID);

        #endregion

    }

    public class AccountRepository : IAccountRepository
    {
        private readonly IDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IDbConnection _cn;
        public AccountRepository(IDbContext dbContext, IMapper mapper, IDbConnection cn)
        {
            this._dbContext = dbContext;
            this._mapper = mapper;
            this._cn = cn;
        }

        #region AccAccountGroup

        public async Task<int> InsertAccAccountGroup(AccountGroupModel accountGroupModel, int userID, IDbTransaction? tran = null)
        {
            var accountGroup = _mapper.Map<AccAccountGroup>(accountGroupModel);
            var logSummaryID = await _dbContext.InsertAddEditLogSummary(accountGroupModel.AccountGroupID, "Account Group : " + accountGroupModel.AccountGroupName + " added/updated by User " + userID, tran);
            accountGroup.AccountGroupID = await _dbContext.SaveAsync(accountGroup, tran, logSummaryID: logSummaryID);
            return accountGroup.AccountGroupID;
        }

        public async Task<AccountGroupModel> GetAccAccountGroup(int accountGroupID, int clientID, int branchID)
        {
            return await _dbContext.GetByQueryAsync<AccountGroupModel>($@"
                                                            Select A.*,P.AccountGroupName As ParentGroupName
                                                            From AccAccountGroup A
                                                            Left Join AccAccountGroup P on P.AccountGroupID=A.ParentID and P.IsDeleted=0
                                                            Where A.AccountGroupID={accountGroupID} and A.ClientID={clientID} and (A.BranchID={branchID} or A.BranchID is null) and A.IsDeleted=0
            ", null);
        }

        private async Task InsertDefaultAccountGroupEntries(int clientID, IDbTransaction? tran = null)
        {
            var accGroupCount = await _dbContext.GetByQueryAsync<int>($@"Select Count(*) From AccAccountGroup Where ClientID={clientID} and IsDeleted=0", null, tran);
            if (accGroupCount == 0)
            {
                List<AccAccountGroup> accountGroups = new()
                {
                    new()
                    {
                        AccountGroupCode="001",
                        AccountGroupName="Assets",
                        GroupTypeID=(int)AccountGroupTypes.Assets,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="002",
                        AccountGroupName="Liability",
                        GroupTypeID=(int)AccountGroupTypes.Liabilities,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="003",
                        AccountGroupName="Direct Expenses",
                        GroupTypeID=(int)AccountGroupTypes.DirectExpenses,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="004",
                        AccountGroupName="Direct Income",
                        GroupTypeID=(int)AccountGroupTypes.DirectIncome,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="005",
                        AccountGroupName="Indirect Expense",
                        GroupTypeID=(int)AccountGroupTypes.IndirectExpenses,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="006",
                        AccountGroupName="Indirect Income",
                        GroupTypeID=(int)AccountGroupTypes.IndirectIncome,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="007",
                        AccountGroupName="Purchase Account",
                        GroupTypeID=(int)AccountGroupTypes.PurchaseAccounts,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="008",
                        AccountGroupName="Sales Account",
                        GroupTypeID=(int)AccountGroupTypes.SalesAccounts,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="009",
                        AccountGroupName="Duties And Taxes",
                        GroupTypeID=(int)AccountGroupTypes.DutiesAndTaxes,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="010",
                        AccountGroupName="Bank Accounts",
                        GroupTypeID=(int)AccountGroupTypes.BankAccounts,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode = "011",
                        AccountGroupName = "Cash In Hand",
                        GroupTypeID=(int)AccountGroupTypes.CashInHand,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="012",
                        AccountGroupName="Capital Accounts",
                        GroupTypeID=(int)AccountGroupTypes.CapitalAccounts,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="013",
                        AccountGroupName="Current Assets",
                        GroupTypeID=(int)AccountGroupTypes.CurrentAssets,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="014",
                        AccountGroupName="Current Liabilities",
                        GroupTypeID=(int)AccountGroupTypes.CurrentLiabilities,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="015",
                        AccountGroupName="Sundry Creditors",
                        GroupTypeID=(int)AccountGroupTypes.SundryCreditors,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="016",
                        AccountGroupName="Sundry Debtors",
                        GroupTypeID=(int)AccountGroupTypes.SundryDebtors,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="017",
                        AccountGroupName="Loans And Advances",
                        GroupTypeID=(int)AccountGroupTypes.LoansAndAdvances,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="018",
                        AccountGroupName="Loan Liabilities",
                        GroupTypeID=(int)AccountGroupTypes.LoanLiabilities,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="019",
                        AccountGroupName="Fixed Assets",
                        GroupTypeID=(int)AccountGroupTypes.FixedAssets,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="020",
                        AccountGroupName="Investments",
                        GroupTypeID=(int)AccountGroupTypes.Investments,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="021",
                        AccountGroupName="Stock In Hand",
                        GroupTypeID=(int)AccountGroupTypes.StockInHand,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="022",
                        AccountGroupName="Gross Profit Or Loss",
                        GroupTypeID=(int)AccountGroupTypes.GrossProfitOrLoss,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="023",
                        AccountGroupName="Depreciation",
                        GroupTypeID=(int)AccountGroupTypes.Depreciation,
                        IsSuperParent=true,

                    },
                    new()
                    {
                        AccountGroupCode="024",
                        AccountGroupName="Net Profit Or Loss",
                        GroupTypeID=(int)AccountGroupTypes.NetProfitOrLoss,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="025",
                        AccountGroupName="Intangible Assests",
                        GroupTypeID=(int)AccountGroupTypes.IntangibleAssests,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="026",
                        AccountGroupName="Long Term Liabilities",
                        GroupTypeID=(int)AccountGroupTypes.LongTermLiabilities,
                        IsSuperParent=true,
                    },
                    new()
                    {
                        AccountGroupCode="027",
                        AccountGroupName="Revenue",
                        GroupTypeID=(int)AccountGroupTypes.Revenue,
                        IsSuperParent=true,
                    },
                };
                await _dbContext.SaveSubItemListAsync(accountGroups, "ClientID", clientID, tran);
            }
        }

        #endregion

        #region AccLedger

        private async Task InsertDefaultAccLedgerEntries(int clientID, IDbTransaction? tran = null)
        {
            var ledgerCount = await _dbContext.GetByQueryAsync<int>($@"Select Count(*) From AccLedger Where ClientID={clientID} and IsDeleted=0", null, tran);
            if (ledgerCount == 0)
            {
                var accountGroups = await _dbContext.GetListAsync<AccAccountGroup>($"ClientID={clientID}", null, tran);
                if (accountGroups != null)
                {
                    var cashInHand = accountGroups.Where(AG => AG.GroupTypeID == (int)AccountGroupTypes.CashInHand).FirstOrDefault();
                    if (cashInHand != null)
                    {
                        await _dbContext.SaveAsync(new AccLedger()
                        {
                            AccountGroupID = cashInHand.AccountGroupID,
                            Alias = "Cash in Hand",
                            LedgerName = "Cash in Hand",
                            ClientID = clientID,
                            IsBillToBill = false
                        }, tran);
                    }

                    var bankAccouts = accountGroups.Where(AG => AG.GroupTypeID == (int)AccountGroupTypes.BankAccounts).FirstOrDefault();
                    if (bankAccouts != null)
                    {
                        await _dbContext.SaveAsync(new AccLedger()
                        {
                            AccountGroupID = bankAccouts.AccountGroupID,
                            Alias = "Bank",
                            LedgerName = "Bank Account",
                            ClientID = clientID,
                            IsBillToBill = false
                        }, tran);
                    }

                    var directExpense = accountGroups.Where(AG => AG.GroupTypeID == (int)AccountGroupTypes.DirectExpenses).FirstOrDefault();
                    if (directExpense != null)
                    {
                        await _dbContext.SaveAsync(new AccLedger()
                        {
                            AccountGroupID = directExpense.AccountGroupID,
                            Alias = "Direct Expense",
                            LedgerName = "Direct Expense",
                            ClientID = clientID,
                            IsBillToBill = false
                        }, tran);
                    }

                    var indirectExpense = accountGroups.Where(AG => AG.GroupTypeID == (int)AccountGroupTypes.IndirectExpenses).FirstOrDefault();
                    if (indirectExpense != null)
                    {
                        await _dbContext.SaveAsync(new AccLedger()
                        {
                            AccountGroupID = indirectExpense.AccountGroupID,
                            Alias = "Indirect Expense",
                            LedgerName = "Indirect Expense",
                            ClientID = clientID,
                            IsBillToBill = false
                        }, tran);

                        await _dbContext.SaveAsync(new AccLedger()
                        {
                            AccountGroupID = indirectExpense.AccountGroupID,
                            Alias = "Discount Paid",
                            LedgerName = "Discount Paid",
                            ClientID = clientID,
                            IsBillToBill = false
                        }, tran);
                    }

                    var directIncome = accountGroups.Where(AG => AG.GroupTypeID == (int)AccountGroupTypes.DirectIncome).FirstOrDefault();
                    if (directIncome != null)
                    {
                        await _dbContext.SaveAsync(new AccLedger()
                        {
                            AccountGroupID = directIncome.AccountGroupID,
                            Alias = "Direct Income",
                            LedgerName = "Direct Income",
                            ClientID = clientID,
                            IsBillToBill = false
                        }, tran);
                    }

                    var indirectIncome = accountGroups.Where(AG => AG.GroupTypeID == (int)AccountGroupTypes.IndirectIncome).FirstOrDefault();
                    if (indirectIncome != null)
                    {
                        await _dbContext.SaveAsync(new AccLedger()
                        {
                            AccountGroupID = indirectIncome.AccountGroupID,
                            Alias = "Indirect Income",
                            LedgerName = "Indirect Income",
                            ClientID = clientID,
                            IsBillToBill = false
                        }, tran);

                        await _dbContext.SaveAsync(new AccLedger()
                        {
                            AccountGroupID = indirectIncome.AccountGroupID,
                            Alias = "Discount Recieved",
                            LedgerName = "Discount Recieved",
                            ClientID = clientID,
                            IsBillToBill = false
                        }, tran);
                    }

                    var purchase = accountGroups.Where(AG => AG.GroupTypeID == (int)AccountGroupTypes.PurchaseAccounts).FirstOrDefault();
                    if (purchase != null)
                    {
                        await _dbContext.SaveAsync(new AccLedger()
                        {
                            AccountGroupID = purchase.AccountGroupID,
                            Alias = "Purchase",
                            LedgerName = "Purchase",
                            ClientID = clientID,
                            IsBillToBill = false
                        }, tran);

                        await _dbContext.SaveAsync(new AccLedger()
                        {
                            AccountGroupID = purchase.AccountGroupID,
                            Alias = "Purchase Return",
                            LedgerName = "Purchase Return",
                            ClientID = clientID,
                            IsBillToBill = false
                        }, tran);
                    }

                    var sales = accountGroups.Where(AG => AG.GroupTypeID == (int)AccountGroupTypes.SalesAccounts).FirstOrDefault();
                    if (sales != null)
                    {
                        await _dbContext.SaveAsync(new AccLedger()
                        {
                            AccountGroupID = sales.AccountGroupID,
                            Alias = "Sales",
                            LedgerName = "Sales",
                            ClientID = clientID,
                            IsBillToBill = false
                        }, tran);

                        await _dbContext.SaveAsync(new AccLedger()
                        {
                            AccountGroupID = sales.AccountGroupID,
                            Alias = "Sales Return",
                            LedgerName = "Sales Return",
                            ClientID = clientID,
                            IsBillToBill = false
                        }, tran);
                    }
                }
            }
        }
        public async Task<int> InsertAccLedger(AccLedgerModel ledgerModel, int currentUserID, IDbTransaction? tran = null)
        {
            if (ledgerModel.GroupTypeID == (int)AccountGroupTypes.SundryCreditors || ledgerModel.GroupTypeID == (int)AccountGroupTypes.SundryDebtors)
            {
                //entity
                var ledgerEntiy = _mapper.Map<EntityCustom>(ledgerModel);
                ledgerEntiy.ClientID = ledgerModel.ClientID;
                ledgerEntiy.EntityTypeID = ledgerModel.GroupTypeID switch
                {
                    (int)AccountGroupTypes.SundryDebtors => (int)EntityType.Customer,
                    (int)AccountGroupTypes.SundryCreditors => (int)EntityType.Supplier,
                    _ => 0
                };                                   
                ledgerModel.EntityID = ledgerEntiy.EntityID = await _dbContext.SaveAsync(ledgerEntiy, tran);

                //entity personal info
                EntityPersonalInfo entityPersonalInfo = new()
                {
                    EntityPersonalInfoID = ledgerModel.EntityPersonalInfoID,
                    EntityID = ledgerEntiy.EntityID,
                    FirstName = ledgerModel.LedgerName
                };
                ledgerModel.EntityPersonalInfoID = entityPersonalInfo.EntityPersonalInfoID = await _dbContext.SaveAsync(entityPersonalInfo, tran);

                //entity address
                var entityAddress = _mapper.Map<EntityAddressCustom>(ledgerModel);
                entityAddress.EntityID = ledgerEntiy.EntityID;
                ledgerModel.AddressID = entityAddress.AddressID = await _dbContext.SaveAsync(entityAddress, tran);

                int logSummaryID = await _dbContext.InsertAddEditLogSummary(ledgerModel.EntityID.Value, "Details of Ledger : " + ledgerModel.LedgerName + " and Entity(EntityID : " + ledgerModel.EntityID.Value + ") added/updated by user " + currentUserID, tran);
                //Ledger
                var ledger = _mapper.Map<AccLedger>(ledgerModel);
                if (ledgerModel.LedgerID > 0)
                {
                    ledgerModel.LedgerID = ledger.LedgerID = await _dbContext.SaveAsync(ledger, tran);
                }
                else
                {
                    ledgerModel.LedgerID = ledger.LedgerID = await GetEntityLedgerID(ledgerModel.EntityID.Value, ledgerModel.ClientID.Value, tran);

                    if (ledger.LedgerID is not 0)
                        await _dbContext.ExecuteAsync($@"Update AccLedger Set LedgerCode=@LedgerCode,Alias=@Alias,IsBillToBill=@IsBillToBill Where LedgerID={ledger.LedgerID}",
                                                        new { LedgerCode = ledgerModel.LedgerCode, Alias = ledgerModel.Alias, IsBillToBill = ledgerModel.IsBillToBill }, tran);
                }
            }
            else
            {
                int logSummaryID = await _dbContext.InsertAddEditLogSummary(ledgerModel.LedgerID, "Details of Ledger : " + ledgerModel.LedgerName + " added/updated by user " + currentUserID, tran);
                var ledger = _mapper.Map<AccLedger>(ledgerModel);
                ledgerModel.LedgerID = ledger.LedgerID = await _dbContext.SaveAsync(ledger, tran, logSummaryID: logSummaryID);
            }

            //Adding/Updating Opening balance entry,
            if (ledgerModel.OpeningBalance > 0)
            {
                int journalMasterID = await GetOpeningBalanceEntryJournalMasterID(ledgerModel.BranchID.Value, tran);
                AccJournalEntry openingBalanceJournalEntry = new()
                {
                    JournalEntryID = ledgerModel.JournalEntryID,
                    JournalMasterID = journalMasterID,
                    LedgerID = ledgerModel.LedgerID
                };

                if (ledgerModel.DrorCr == (int)DebitOrCredit.Debit)
                {
                    openingBalanceJournalEntry.Debit = ledgerModel.OpeningBalance;
                    openingBalanceJournalEntry.Credit = 0;
                }

                if (ledgerModel.DrorCr == (int)DebitOrCredit.Credit)
                {
                    openingBalanceJournalEntry.Debit = 0;
                    openingBalanceJournalEntry.Credit = ledgerModel.OpeningBalance;
                }
                ledgerModel.JournalEntryID = openingBalanceJournalEntry.JournalEntryID = await _dbContext.SaveAsync(openingBalanceJournalEntry, tran);
            }

            //Adding bill to bill items
            if (ledgerModel.IsBillToBill)
            {
                ledgerModel.BillToBillItems.ForEach(billToBillItem => billToBillItem.ReferenceTypeID = (int)ReferenceTypes.OldReference);
                await InsertBillToBillEntries(ledgerModel.BillToBillItems, ledgerModel.LedgerID, ledgerModel.JournalEntryID, tran);
            }

            //Deleting opening balance entry that previously added
            if (ledgerModel.OpeningBalance == 0 && ledgerModel.JournalEntryID > 0)
            {
                int openingBalanceLogSummaryID = await _dbContext.InsertDeleteLogSummary(ledgerModel.JournalEntryID, "Opening balance for ledger " + ledgerModel.LedgerName + "(LedgerID : " + ledgerModel.LedgerID + ") deleted by user :" + currentUserID, tran);
                await _dbContext.DeleteAsync<AccJournalEntry>(ledgerModel.JournalEntryID, tran, openingBalanceLogSummaryID);
            }
            return ledgerModel.LedgerID;
        }
        public async Task<AccLedgerModel> GetAccLedger(int ledgerID, int clientID, int branchID, IDbTransaction? tran = null)
        {
            AccLedgerModel LedgerModel = new();
            LedgerModel = await _dbContext.GetByQueryAsync<AccLedgerModel>($@"
                                    Select L.*,AL.LedgerName AgentLedgerName,E.Phone,E.EmailAddress,EA.AddressID,EA.AddressLine1,EA.AddressLine2,EA.AddressLine3,
                                    EA.CityID,EA.StateID,EA.CountryID,CC.CityName,CS.StateName,C.CountryName,EP.EntityPersonalInfoID,AJ.JournalEntryID,AG.AccountGroupName,LT.GroupTypeName AS AccountTypeName,--LT.LedgerTypeName AS AccountTypeName,
                                    CASE
	                                    WHEN Debit>0 and Credit=0 THEN Debit 
	                                    WHEN Debit=0 and Credit>0 THEN Credit 
	                                    END AS OpeningBalance,
                                    CASE
	                                    WHEN Debit>0 and Credit=0 THEN 1 
	                                    WHEN Debit=0 and Credit>0 THEN 2 
	                                    END AS DrorCr
                                    From AccLedger L
                                    Left Join AccAccountGroup AG ON AG.AccountGroupID=L.AccountGroupID AND AG.IsDeleted=0
                                    Left Join AccLedger AL ON AL.LedgerID=L.AgentLedgerID AND AL.IsDeleted=0
                                    Left Join AccAccountGroupType LT ON LT.GroupTypeID=AG.GroupTypeID AND LT.IsDeleted=0
                                    Left Join viEntity E ON E.EntityID=L.EntityID 
                                    Left Join EntityAddress EA ON EA.EntityID=E.EntityID
                                    Left Join EntityPersonalInfo EP ON EP.EntityID=E.EntityID AND EP.IsDeleted=0
                                    Left Join Country C ON C.CountryID=EA.CountryID AND C.IsDeleted=0
                                    Left Join CountryState CS ON CS.StateID=EA.StateID AND CS.IsDeleted=0
                                    Left Join CountryCity CC ON CC.CityID=EA.CityID AND CC.IsDeleted=0
	                                Join AccVoucherType AT ON VoucherTypeNatureID={(int)VoucherTypeNatures.Journal} AND AT.ClientID={clientID} AND AT.VoucherTypeName like 'Journal' AND AT.IsDeleted=0
	                                Left Join AccJournalMaster AM ON AM.VoucherTypeID=AT.VoucherTypeID AND JournalNo=0 AND AM.BranchID={branchID} AND AM.IsDeleted=0
	                                Left Join AccJournalEntry AJ ON AJ.JournalMasterID=AM.JournalMasterID AND AJ.IsDeleted=0 AND AJ.LedgerID=L.LedgerID AND AJ.IsDeleted=0
                                    Where L.LedgerID={ledgerID} AND L.IsDeleted=0", null);

            if (LedgerModel.IsBillToBill)
            {
                LedgerModel.BillToBillItems = await _dbContext.GetListByQueryAsync<BillToBillModel>($@"SELECT B.*
                                                                                        FROM AccBilltoBill B
                                                                                        where B.LedgerID={ledgerID} and B.IsDeleted=0", null);
            }
            return LedgerModel;
        }
        public async Task<bool> DeleteAccLedger(int ledgerID, int userID, IDbTransaction? tran = null)
        {
            var LedgerDetails = await _dbContext.GetByQueryAsync<IdnValuePair>(@$"
                                                        Select EntityID AS ID,LedgerName AS Value
                                                        From AccLedger
                                                        Where LedgerID={ledgerID}
                    ", null, tran);

            int ledgerLogSummaryID = await _dbContext.InsertDeleteLogSummary(ledgerID, "Ledger " + LedgerDetails.Value + " deleted by User :" + userID, tran);
            int entityLogSummaryID = await _dbContext.InsertDeleteLogSummary(LedgerDetails.ID, "Entity " + LedgerDetails.Value + " deleted along with remove of ledger(" + ledgerID + ") by User :" + userID, tran);

            await _dbContext.DeleteAsync<AccLedger>(ledgerID, tran, ledgerLogSummaryID);
            await _dbContext.DeleteAsync<EntityCustom>(LedgerDetails.ID, tran, entityLogSummaryID);
            await _dbContext.ExecuteAsync($"Update EntityAddress Set IsDeleted=1 Where EntityID={LedgerDetails.ID}", null, tran);
            await _dbContext.ExecuteAsync($"Update EntityPersonalInfo Set IsDeleted=1 Where EntityID={LedgerDetails.ID}", null, tran);
            return true;
        }
        public async Task<int> GetEntityLedgerID(int entityID, int clientID, IDbTransaction? tran = null)
        {
            int ledgerID = await _dbContext.GetFieldsAsync<AccLedger, int>("LedgerID", $"EntityID={entityID} AND ClientID={clientID} AND IsDeleted=0", null, tran);
            if (ledgerID is 0)
            {
                AccLedger ledger = new();
                var entity = await _dbContext.GetAsync<ViEntity>(entityID, tran);
                if (entity is not null)
                {
                    ledger.LedgerName = entity.Name;
                    ledger.EntityID = entity.EntityID;
                    ledger.Alias = entity.Name;
                    switch (entity.EntityTypeID)
                    {
                        case (int)EntityType.Client:
                        case (int)EntityType.Customer:
                            ledger.AccountGroupID = await _dbContext.GetByQueryAsync<int?>("Select Top 1 AccountGroupID From AccAccountGroup Where ClientID=@ClientID And GroupTypeID=@GroupTypeID And IsDeleted=0", new { ClientID = clientID, GroupTypeID = (int)AccountGroupTypes.SundryCreditors }, tran);
                            ledger.LedgerCode = "SC-" + await _dbContext.GetByQueryAsync<int>(@$"Select Count(L.LedgerID)+1 
	                                                                                                From AccLedger L
	                                                                                                JOIN AccAccountGroup G ON G.AccountGroupID=L.AccountGroupID
	                                                                                                JOIN AccAccountGroupType T ON T.GroupTypeID=G.GroupTypeID
	                                                                                                Where L.ClientID={clientID} AND L.IsDeleted=0 And T.GroupTypeID={(int)AccountGroupTypes.SundryDebtors}", null, tran);
                            break;
                        case (int)EntityType.Supplier:
                            ledger.AccountGroupID = await _dbContext.GetByQueryAsync<int?>("Select Top 1 AccountGroupID From AccAccountGroup Where ClientID=@ClientID And GroupTypeID=@GroupTypeID And IsDeleted=0", new { ClientID = clientID, GroupTypeID = (int)AccountGroupTypes.SundryDebtors }, tran);
                            ledger.LedgerCode = "SD-" + await _dbContext.GetByQueryAsync<int>(@$"Select Count(L.LedgerID)+1 
	                                                                                                From AccLedger L
	                                                                                                JOIN AccAccountGroup G ON G.AccountGroupID=L.AccountGroupID
	                                                                                                JOIN AccAccountGroupType T ON T.GroupTypeID=G.GroupTypeID
	                                                                                                Where L.ClientID={clientID} AND L.IsDeleted=0 And T.GroupTypeID={(int)AccountGroupTypes.SundryCreditors}", null, tran);
                            break;
                    }
                    ledger.ClientID = clientID;
                    ledgerID = await _dbContext.SaveAsync(ledger, tran);
                }
            }
            return ledgerID;
        }
        public async Task<string?> GenerateLedgerSearchWhereCondition(LedgerSearchModel searchModel, IDbTransaction? tran = null)
        {
            string whereCondition = "";
            int VoucherTypeNatureID = await _dbContext.GetFieldsAsync<AccVoucherType, int>("VoucherTypeNatureID", $"IsDeleted=0 AND VoucherTypeID={searchModel.VoucherTypeID}", null);
            switch (VoucherTypeNatureID)
            {
                case (int)VoucherTypeNatures.Receipt:
                case (int)VoucherTypeNatures.Payment:
                    if (searchModel.DrOrCr == (int)DebitOrCredit.Debit)
                        whereCondition = $" And LedgerTypeID IN({(int)LedgerTypes.Cash},{(int)LedgerTypes.Bank})";
                    else if (searchModel.DrOrCr == (int)DebitOrCredit.Credit)
                        whereCondition = $" And LedgerTypeID NOT IN({(int)LedgerTypes.Cash},{(int)LedgerTypes.Bank})";
                    break;

                //Contra 
                //only cash and bank ledgers will come under this voucher type
                case (int)VoucherTypeNatures.Contra:
                    whereCondition = $" And LedgerTypeID IN ({(int)LedgerTypes.Cash,(int)LedgerTypes.Bank})";
                    break;

                //Journal 
                //All the other ledgers that not comes under cash or bank
                case (int)VoucherTypeNatures.Journal:
                    whereCondition = $" And LedgerTypeID NOT IN ({(int)LedgerTypes.Cash,(int)LedgerTypes.Bank})";
                    break;

                //for sales and sales return the posting ledger is assigned when creating the sales invoice type
                //so we need to find the customer,other or  normal ledger from the table
                case (int)VoucherTypeNatures.Sales:
                case (int)VoucherTypeNatures.SalesReturn:
                    whereCondition = $" And LedgerTypeID IN ({(int)LedgerTypes.Customer},{(int)LedgerTypes.Other},{(int)LedgerTypes.NormalLedger})";
                    break;

                //for purchase and sales puchase the posting ledger is assigned when creating the purchase invoice type
                //so we need to find the supplier ledger from the table
                case (int)VoucherTypeNatures.Purchase:
                case (int)VoucherTypeNatures.PurchaseReturn:
                    whereCondition = $" And LedgerTypeID={(int)LedgerTypes.Supplier}";
                    break;
            }

            if (!string.IsNullOrEmpty(searchModel.SearchString))
            {
                whereCondition += $" AND LedgerName LIKE '%{searchModel.SearchString}%'";
            }
            return whereCondition;
        }

        #endregion

        #region AccVoucherType

        private async Task InsertDefaultVoucherTypeEntries(int clientID, IDbTransaction? tran = null)
        {
            var cnt = await _dbContext.GetByQueryAsync<int>($@"Select Count(*) From AccVoucherType Where ClientID={clientID} and IsDeleted=0", null, tran);
            if (cnt == 0)
            {
                List<AccVoucherType> voucherTypes = new()
                {
                    new AccVoucherType()
                    {
                        VoucherTypeName = "Receipt",
                        VoucherTypeNatureID = (int)VoucherTypeNatures.Receipt,
                        ClientID = clientID,
                    },
                    new AccVoucherType()
                    {
                        VoucherTypeName="Payment",
                        VoucherTypeNatureID = (int)VoucherTypeNatures.Payment,
                        ClientID = clientID,
                    },
                    new AccVoucherType()
                    {
                        VoucherTypeName="Contra",
                        VoucherTypeNatureID = (int)VoucherTypeNatures.Contra,
                        ClientID = clientID,
                    },
                    new AccVoucherType()
                    {
                        VoucherTypeName="Journal",
                        VoucherTypeNatureID = (int)VoucherTypeNatures.Journal,
                        ClientID = clientID,
                    }
                };
                await _dbContext.SaveSubItemListAsync(voucherTypes, "ClientID", clientID, tran);
            }
        }
        private async Task InsertDefaultVoucherTypeSettingEntries(int clientID, IDbTransaction? tran = null)
        {
            var voucherTypesList = await _dbContext.GetListAsync<AccVoucherType>($"ClientID={clientID}", null, tran);
            if (voucherTypesList != null && voucherTypesList.Count > 0)
            {
                var branches = await _dbContext.GetListAsync<BranchCustom>($"ClientID={clientID}", null, tran);
                foreach (var branch in branches)
                {
                    var cnt = await _dbContext.GetByQueryAsync<int>($@"Select Count(*) From AccVoucherTypeSetting Where BranchID=@BranchID and IsDeleted=0", branch, tran);
                    if (cnt == 0)
                    {
                        List<AccVoucherTypeSetting> voucherTypeSettings = new()
                            {
                                new AccVoucherTypeSetting()
                                {
                                    PeriodicityID=(int)VoucherNumberingPeriodicity.Continuous,
                                    NumberingTypeID=(int)VoucherNumberingTypes.Automatic_Manual_Override,
                                    StartingNumber=1,
                                    Prefix="R-",
                                    HeaderText="Receipt",
                                    VoucherTypeID=voucherTypesList.Where(s=>s.VoucherTypeNatureID==(int)VoucherTypeNatures.Receipt).First().VoucherTypeID
                                },
                                new AccVoucherTypeSetting()
                                {
                                    PeriodicityID=(int)VoucherNumberingPeriodicity.Continuous,
                                    NumberingTypeID=(int)VoucherNumberingTypes.Automatic_Manual_Override,
                                    StartingNumber=1,
                                    Prefix="P-",
                                    HeaderText="Payment",
                                    VoucherTypeID=voucherTypesList.Where(s=>s.VoucherTypeNatureID==(int)VoucherTypeNatures.Payment).First().VoucherTypeID
                                },
                                new AccVoucherTypeSetting()
                                {
                                    PeriodicityID=(int)VoucherNumberingPeriodicity.Continuous,
                                    NumberingTypeID=(int)VoucherNumberingTypes.Automatic_Manual_Override,
                                    StartingNumber=1,
                                    Prefix="C-",
                                    HeaderText="Contra",
                                    VoucherTypeID=voucherTypesList.Where(s=>s.VoucherTypeNatureID==(int)VoucherTypeNatures.Contra).First().VoucherTypeID
                                },
                                new AccVoucherTypeSetting()
                                {
                                    PeriodicityID=(int)VoucherNumberingPeriodicity.Continuous,
                                    NumberingTypeID=(int)VoucherNumberingTypes.Automatic_Manual_Override,
                                    StartingNumber=1,
                                    Prefix="J-",
                                    HeaderText="Journal",
                                    VoucherTypeID=voucherTypesList.Where(s=>s.VoucherTypeNatureID==(int)VoucherTypeNatures.Journal).First().VoucherTypeID
                                },
                            };
                        await _dbContext.SaveSubItemListAsync(voucherTypeSettings, "BranchID", branch.BranchID, tran);
                    }
                }
            }
        }
        public async Task<int> InsertAccVoucherType(VoucherTypeModel voucherTypeModel, IDbTransaction? tran = null)
        {
            var voucherType = _mapper.Map<AccVoucherType>(voucherTypeModel);
            voucherType.VoucherTypeID = await _dbContext.SaveAsync(voucherType, tran);

            var voucherTypeSetting = _mapper.Map<AccVoucherTypeSetting>(voucherTypeModel);
            voucherTypeSetting.VoucherTypeID = voucherType.VoucherTypeID;
            voucherTypeSetting.Prefix = !string.IsNullOrEmpty(voucherTypeSetting.Prefix) ? voucherTypeSetting.Prefix.ToUpper() : "";
            await _dbContext.SaveAsync(voucherTypeSetting, tran);
            return voucherType.VoucherTypeID;
        }
        public async Task<VoucherNumberModel> GetVoucherNumber(int voucherTypeId, int branchID, IDbTransaction? tran = null)
        {
            return await _dbContext.GetByQueryAsync<VoucherNumberModel>($@"
                            Select ISNULL(MAX(A.JournalNo)+ 1,VS.StartingNumber) as JournalNo,ISNULL(VS.Prefix,'')As JournalNoPrefix
                            From AccvoucherTypeSetting VS
                            LEFT JOIN  AccJournalMaster A on VS.BranchID = A.BranchID and VS.VoucherTypeID=A.VoucherTypeID
                            Where VS.BranchID = @BranchID and VS.VoucherTypeID = @VoucherTypeID
                            Group by A.JournalNoPrefix,VS.Prefix,VS.StartingNumber,VS.BranchID", new { BranchID = branchID, VoucherTypeID = voucherTypeId }, tran);
        }

        #endregion
        
        #region Other Accounts Related Functions

        public async Task InsertClientDefaultAccountsRelatedEntries(int clientID, IDbTransaction? tran = null)
        {
            //Keep the order always same
            await InsertDefaultAccountGroupEntries(clientID, tran);
            await InsertDefaultAccLedgerEntries(clientID, tran);
            await InsertDefaultVoucherTypeEntries(clientID, tran);
            await InsertDefaultVoucherTypeSettingEntries(clientID, tran);
        }
        public async Task<int> GetOpeningBalanceEntryJournalMasterID(int branchID, IDbTransaction? tran = null)
        {
            var branch = await _dbContext.GetAsync<Model.Branch>(branchID, tran);
            //Journal voucher type Id
            int journalVoucherTypeID = await _dbContext.GetFieldsAsync<AccVoucherType, int>("VoucherTypeID", $"VoucherTypeName like 'Journal' and VoucherTypeNatureID={(int)VoucherTypeNatures.Journal} and ClientID={branch.ClientID.Value} and IsDeleted=0", null, tran);

            //JournalNo=0 is for opening balance
            var journalMaster = await _dbContext.GetAsync<AccJournalMaster>($"VoucherTypeID={journalVoucherTypeID} and JournalNo=0 and BranchID={branchID}", null, tran);
            if (journalMaster == null)
            {
                journalMaster = new()
                {
                    Date = DateTime.UtcNow,
                    BranchID = branchID,
                    VoucherTypeID = journalVoucherTypeID,
                    JournalNoPrefix = "",
                    //JournalNo=0 is for opening balance
                    JournalNo = 0
                };
                journalMaster.JournalMasterID = await _dbContext.SaveAsync(journalMaster, tran);
            }
            return journalMaster.JournalMasterID;
        }
        public async Task InsertBillToBillEntries(List<BillToBillModel> bilToBillList, int ledgerID, int journalEntryID, IDbTransaction? tran = null)
        {
            var billtobillItems = _mapper.Map<List<AccBillToBill>>(bilToBillList);

            if (billtobillItems.Count > 0)
            {
                await _dbContext.SaveSubItemListAsync(billtobillItems, "LedgerID", ledgerID, tran);
            }
        }

        #endregion

        #region Tax Category

        public async Task InsertDefaultTaxCategories(int clientID, Countries countryId, IDbTransaction? tran = null)
        {
            var taxCategoriesCount = await _dbContext.GetByQueryAsync<int>($"Select Count(*) From TaxCategory Where ClientID=@clientID and IsDeleted=0", new { clientID }, tran);
            if (taxCategoriesCount == 0)
            {
                await InsertClientDefaultAccountsRelatedEntries(clientID, tran);
                var taxAccountGroup = await _dbContext.GetByQueryAsync<AccAccountGroup>($"Select Top 1 * From AccAccountGroup Where ClientID={clientID} and GroupTypeID=@GroupTypeID And IsDeleted=0", new { GroupTypeID = (int)AccountGroupTypes.DutiesAndTaxes}, tran);
                if (taxAccountGroup == null)
                {
                    taxAccountGroup = new()
                    {
                        AccountGroupCode = "009",
                        AccountGroupName = "Duties And Taxes",
                        GroupTypeID = (int)AccountGroupTypes.DutiesAndTaxes
                    };
                    taxAccountGroup.AccountGroupID = await _dbContext.SaveAsync(taxAccountGroup, tran);
                }

                switch (countryId)
                {
                    case Countries.UAE:
                        await CreateArabianCategory(clientID, 5, taxAccountGroup.AccountGroupID, tran);
                        break;
                    case Countries.India:
                        await CreateIndianCategory(clientID, 5, 2.5, taxAccountGroup.AccountGroupID, tran);
                        await CreateIndianCategory(clientID, 12, 6, taxAccountGroup.AccountGroupID, tran);
                        await CreateIndianCategory(clientID, 18, 9, taxAccountGroup.AccountGroupID, tran);
                        await CreateIndianCategory(clientID, 28, 14, taxAccountGroup.AccountGroupID, tran);
                        break;
                }
            }
        }
        private async Task CreateIndianCategory(int clientID, int percentage, double percentageSpit, int accountGroupId, IDbTransaction tran)
        {
            TaxCategory gst = new()
            {
                ClientID = clientID,
                TaxCategoryName = $"GST {percentage}%"
            };
            gst.TaxCategoryID = await _dbContext.SaveAsync(gst, tran);

            #region SGST

            AccLedger ledger1 = new()
            {
                AccountGroupID = accountGroupId,
                ClientID = clientID,
                Alias = $"SGST {percentageSpit}%",
                LedgerName = $"SGST {percentageSpit}%",
            };
            ledger1.LedgerID = await _dbContext.SaveAsync(ledger1, tran);

            TaxCategoryItem taxItem1 = new()
            {
                LedgerID = ledger1.LedgerID,
                Percentage = Convert.ToDecimal(percentageSpit),
                TaxCategoryItemName = $"SGST {percentageSpit}%",
                TaxCategoryID = gst.TaxCategoryID
            };
            await _dbContext.SaveAsync(taxItem1, tran);

            #endregion

            #region CGST

            AccLedger ledger2 = new()
            {
                AccountGroupID = accountGroupId,
                ClientID = clientID,
                Alias = $"CGST {percentageSpit}%",
                LedgerName = $"CGST {percentageSpit}%",
            };
            ledger2.LedgerID = await _dbContext.SaveAsync(ledger2, tran);

            TaxCategoryItem taxItem2 = new()
            {
                LedgerID = ledger2.LedgerID,
                Percentage = Convert.ToDecimal(percentageSpit),
                TaxCategoryItemName = $"CGST {percentageSpit}%",
                TaxCategoryID = gst.TaxCategoryID
            };
            await _dbContext.SaveAsync(taxItem2, tran);

            #endregion
        }
        private async Task CreateArabianCategory(int clientID, int percentage, int accountGroupId, IDbTransaction tran)
        {
            TaxCategory vat = new()
            {
                ClientID = clientID,
                TaxCategoryName = $"VAT {percentage}%"
            };
            vat.TaxCategoryID = await _dbContext.SaveAsync(vat, tran);

            AccLedger ledger1 = new()
            {
                AccountGroupID = accountGroupId,
                ClientID = clientID,
                Alias = $"VAT {percentage}%",
                LedgerName = $"VAT {percentage}%",
            };
            ledger1.LedgerID = await _dbContext.SaveAsync(ledger1, tran);

            TaxCategoryItem taxItem1 = new()
            {
                LedgerID = ledger1.LedgerID,
                Percentage = Convert.ToDecimal(percentage),
                TaxCategoryItemName = $"VAT {percentage}%",
                TaxCategoryID = vat.TaxCategoryID
            };
            await _dbContext.SaveAsync(taxItem1, tran);

        }
        public async Task<int?> GetTaxCategoryItemLedgerID(int taxCategoryItemID, int clientID, IDbTransaction? tran = null)
        {
            int? ledgerID = await _dbContext.GetFieldsAsync<TaxCategoryItem, int?>("LedgerID", $"TaxCategoryItemID={taxCategoryItemID}", null, tran);
            if (ledgerID is null)
            {
                var taxCategoryItem = await _dbContext.GetAsync<TaxCategoryItem>(taxCategoryItemID, tran);
                if(taxCategoryItem is not null)
                {
                    AccLedger ledger = new()
                    {
                        LedgerName = taxCategoryItem.TaxCategoryItemName,
                        AccountGroupID = await _dbContext.GetByQueryAsync<int?>(@$"Select Top 1 G.AccountGroupID 
	                                                                            From AccAccountGroup G
	                                                                            JOIN AccAccountGroupType T ON T.GroupTypeID=G.GroupTypeID
	                                                                            Where G.ClientID={clientID} AND G.IsDeleted=0 And T.GroupTypeID={(int)AccountGroupTypes.DutiesAndTaxes}", null, tran),
                        ClientID = clientID
                    };

                    ledger.LedgerName = taxCategoryItem.TaxCategoryItemName;
                    if (!ledger.LedgerName.Contains($"{(int)Math.Floor(taxCategoryItem.Percentage)}"))
                        ledger.LedgerName += "-" + (int)Math.Floor(taxCategoryItem.Percentage);
                    ledger.LedgerCode = taxCategoryItem.TaxCategoryItemName.Replace(' ', '-');
                    if (!ledger.LedgerCode.Contains($"{(int)Math.Floor(taxCategoryItem.Percentage)}"))
                        ledger.LedgerCode += "-" + (int)Math.Floor(taxCategoryItem.Percentage);
                    ledgerID = await _dbContext.SaveAsync(ledger, tran);
                }
            }
            return ledgerID;
        }

        #endregion

        #region Balance Sheet

        private List<BalancesheetSPDataModel> spBalancesheetData = new();
        List<BalancesheetReportItemModel>[] balancesheetTemp;

        public async Task<BalancesheetReportModel> GetBalancesheet(BalanceSheetReportPostModel balanceSheetReportPostModel, int clientID)
        {

            BalancesheetGetModel model = new()
            {
                BranchID = balanceSheetReportPostModel.BranchID,
                ClientID = clientID,
                Date = balanceSheetReportPostModel.Date
            };

            var dataSet = await _dbContext.GetDataFromSP("sp_Balancesheet", model);
            spBalancesheetData = (await dataSet.ReadAsync<BalancesheetSPDataModel>()).ToList();
            balancesheetTemp = new List<BalancesheetReportItemModel>[6];
            for (int i = 0; i < 6; i++)
            {
                balancesheetTemp[i] = new List<BalancesheetReportItemModel>();
            }

            for (int natureId = 1; natureId <= 6; natureId++)
            {
                foreach (var item in spBalancesheetData.Where(s => s.ParentID < 7 && s.Nature == natureId))
                {
                    SetGroupTree(item, 0, item.Nature);
                }
            }

            var res = new BalancesheetReportModel()
            {
                DirectIncome = balancesheetTemp[0],
                DirectExpense = balancesheetTemp[1],
                IndirectIncome = balancesheetTemp[2],
                IndirectExpense = balancesheetTemp[3],
                Asset = balancesheetTemp[4],
                Liability = balancesheetTemp[5]
            };

            decimal gross = res.DirectIncome.Sum(s => s.Amount) - res.DirectExpense.Sum(s => s.Amount);
            if (gross > 0)
                res.GrossProfit = gross;
            else
                res.GrossLoss = gross * -1;

            decimal net = res.IndirectIncome.Sum(s => s.Amount) + res.GrossProfit - res.IndirectExpense.Sum(s => s.Amount) - (res.GrossLoss * -1);
            if (net > 0)
                res.NetProfit = net;
            else
                res.NetLoss = net * -1;

            res.TotalDirectExpense = res.DirectExpense.Sum(s => s.Amount) + res.GrossProfit;
            res.TotalDirectIncome = res.DirectIncome.Sum(s => s.Amount) + res.GrossLoss;
            res.TotalIndirectExpense = res.IndirectExpense.Sum(s => s.Amount) + res.GrossLoss + res.NetProfit;
            res.TotalIndirectIncome = res.IndirectIncome.Sum(s => s.Amount) + res.GrossProfit + res.NetLoss;
            res.TotalLiability = res.Liability.Sum(s => s.Amount) - res.NetLoss + res.NetProfit;
            res.TotalAsset = res.Asset.Sum(s => s.Amount);
            return res;
        }

        private void SetGroupTree(BalancesheetSPDataModel row, int nodelevel, int natureid)
        {
            row.Amount = Math.Round(row.Amount, 2);
            nodelevel = nodelevel + 1;
            if (row.AccountGorLType == 2)//If it is Ledger
            {
                balancesheetTemp[row.Nature - 1].Add(new()
                {
                    Amount = row.Amount,
                    ID = row.AccountGorLID,
                    Particulars = row.AccountGorLName,
                    NodeLevel = nodelevel,
                    IsLedger = true,
                    ParentID = row.ParentID
                });
            }
            else //If it is Group
            {
                var groupEntry = new BalancesheetReportItemModel()
                {
                    Amount = row.Amount,
                    ID = row.AccountGorLID,
                    Particulars = row.AccountGorLName,
                    NodeLevel = nodelevel,
                    ParentID = row.ParentID
                };
                balancesheetTemp[row.Nature - 1].Add(groupEntry);
                foreach (var item in spBalancesheetData.Where(s => s.ParentID == row.AccountGorLID && s.Nature == row.Nature))
                {
                    SetGroupTree(item, nodelevel, item.Nature);
                }

                groupEntry.TotalAmount = balancesheetTemp[row.Nature - 1].Where(s => s.ParentID == row.AccountGorLID).Select(s => s.Amount).Sum() +
                    balancesheetTemp[row.Nature - 1].Where(s => s.ParentID == row.AccountGorLID).Select(s => s.TotalAmount).Sum();
            }
        }

        #endregion

    }
}
