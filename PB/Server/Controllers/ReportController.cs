using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PB.Client.Pages.Accounts.Ledgers;
using PB.EntityFramework;
using PB.Model;
using PB.Server.Repository;
using PB.Shared.Enum;
using PB.Shared.Models.Reports;
using System.Diagnostics;
using System.Linq;

namespace PB.Server.Controllers
{
    public class ReportController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IAccountRepository _account;

        public ReportController(IDbContext dbContext, IAccountRepository account)
        {
            _dbContext = dbContext;
            _account = account;
        }

        #region Profit and Loss

        [HttpPost("profit-and-loss")]
        public async Task<IActionResult> GetProfitAndLossReport(ProfitAndLossReportPostModel profitAndLossReportPostModel)
        {
            var profitAndLossReportData = await _dbContext.GetListByQueryAsync<ProfitAndLossReportDataModel>(@$"Select Sum(E.Debit) Debit,Sum(E.Credit) Credit,G.Nature
                                                                                From AccJournalMaster M
                                                                                Join AccJournalEntry E ON E.JournalMasterID=M.JournalMasterID
                                                                                Join AccLedger L ON L.LedgerID=E.LedgerID
                                                                                Join AccAccountGroup G ON G.AccountGroupID=L.AccountGroupID
                                                                                Where M.BranchID=@BranchID And M.IsDeleted=0 And M.IsSuccess=1 And M.Date Between @FromDate And @ToDate
                                                                                Group By G.Nature", null);

            ProfitAndLossReportModel profitAndLossReportModel = new()
            {
                ClientName = await _dbContext.GetFieldsAsync<ViEntity, string?>("Name", $"EntityID={CurrentEntityID}", null),
                CurrencySymbol = await _dbContext.GetByQueryAsync<string>(@$"Select CR.Symbol
                                                                                From viBranch B
                                                                                Left Join CountryZone Z ON Z.ZoneID=B.ZoneID AND Z.IsDeleted=0
                                                                                Left Join Country C ON C.CountryID=Z.CountryID AND C.IsDeleted=0
                                                                                Left Join Currency CR ON CR.CurrencyID=C.CurrencyID AND CR.IsDeleted=0
                                                                                Where B.BranchID={CurrentBranchID} AND B.ClientID={CurrentClientID}", null)
            };

            return Ok(profitAndLossReportModel);
        }

        #endregion

        #region Balance Sheet

        [HttpPost("balance-sheet")]
        public async Task<IActionResult> GetBalanceSheetReport(BalanceSheetReportPostModel balanceSheetReportPostModel)
        {
            if (balanceSheetReportPostModel.BranchID is null && !balanceSheetReportPostModel.IsAdmin)
                balanceSheetReportPostModel.BranchID = CurrentBranchID;
            return Ok(await _account.GetBalancesheet(balanceSheetReportPostModel, CurrentClientID));
        }

        #endregion

        #region Trial Balance

        [HttpPost("trial-balance")]
        public async Task<IActionResult> GetTrialBalanceReport(TrialBalanceReportPostModel trialBalanceReportPostModel)
        {
            if (trialBalanceReportPostModel.BranchID is null && !trialBalanceReportPostModel.IsAdmin)
                trialBalanceReportPostModel.BranchID = CurrentBranchID;
            var trialBalanceWholeAccountItems = await _dbContext.GetListByQueryAsync<TrialBalanceReportItemModel>(@$"Select L.LedgerID,L.LedgerName,
                                                                        case when Sum(J.Debit)>Sum(J.Credit) then Sum(J.Debit)-Sum(J.Credit) else 0 end Debit ,
                                                                        case when Sum(J.Credit)>Sum(J.Debit) then Sum(J.Credit)-Sum(J.Debit) else 0 end Credit,
                                                                        AccountGroupName, L.AccountGroupID
                                                                        From AccJournalMaster JM
                                                                        join AccJournalEntry J on JM.JournalMasterID=J.JournalMasterID
                                                                        join AccLedger L on J.LedgerID=L.LedgerID
                                                                        LEFT JOIN AccAccountGroup G on G.AccountGroupID=L.AccountGroupID
                                                                        where JM.Date <=@Date AND J.IsDeleted = 0 and JM.IsDeleted=0 and JM.BranchID=@BranchID
                                                                        Group by L.LedgerName, AccountGroupName,L.AccountGroupID,L.LedgerID
                                                                        order by AccountGroupName,L.LedgerName", trialBalanceReportPostModel);

            TrialBalanceReportModel TrialBalanceReport = new()
            {
                Date = trialBalanceReportPostModel.Date,
                ClientName = await _dbContext.GetFieldsAsync<ViEntity, string?>("Name", $"EntityID={CurrentEntityID}", null),
                CurrencySymbol = await _dbContext.GetByQueryAsync<string>(@$"Select CR.Symbol
                                                                From viBranch B
                                                                Left Join CountryZone Z ON Z.ZoneID=B.ZoneID AND Z.IsDeleted=0
                                                                Left Join Country C ON C.CountryID=Z.CountryID AND C.IsDeleted=0
                                                                Left Join Currency CR ON CR.CurrencyID=C.CurrencyID AND CR.IsDeleted=0
                                                                Where B.BranchID={CurrentBranchID} AND B.ClientID={CurrentClientID}", null)
            };

            // Grouping by AccountGroupID
            var groupedByAccountGroupId = trialBalanceWholeAccountItems.GroupBy(item => item.AccountGroupID);

            // Filtering Account Groups
            TrialBalanceReport.Groups = groupedByAccountGroupId
                .Select(group =>
                    new TrialBalanceReportGroupModel()
                    {
                        AccountGroupID = group.Key, // Assuming ParentID cannot be null
                        Debit = group.Sum(item => item.Debit),
                        Credit = group.Sum(item => item.Credit),
                        AccountGroupName = group.First().AccountGroupName,
                        GroupItems = group
                            .Select(ledger => new TrialBalanceReportGroupItemModel()
                            {
                                LedgerID = ledger.LedgerID,
                                LedgerName = ledger.LedgerName,
                                AccountGroupID = ledger.AccountGroupID,
                                Debit = ledger.Debit,
                                Credit = ledger.Credit
                            })
                            .ToList()
                    })
                .ToList();

            return Ok(TrialBalanceReport);
        }

        #endregion

        #region General Ledger Report

        [HttpPost("general-ledger")]
        public async Task<IActionResult> GetGeneralLedgerReport(GeneralLedgerReportPostModel generalLedgerReportPostModel)
        {
            var genralLedgerReportItems = await _dbContext.GetListByQueryAsync<GeneralLedgerReportItemModel>($@"
                                            Select LedgerID,LedgerName,Date,JournalNo,Particulars,Debit,Credit,
                                            Case when Balance<0 then CONVERT(varchar(20),cast(Balance*-1 as decimal(18,2)))+' CR'
                                            else CONVERT(varchar(20),cast(Balance as decimal(18,2)))+' DR' end as Balance
                                            From(
                                                SELECT LedgerID,LedgerName,Date,JournalNo,Particulars,Debit,Credit,
                                                SUM(Debit - Credit) OVER(PARTITION BY LedgerID ORDER BY Date,JournalMasterID,JournalEntryID ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) 
                                                AS Balance
                                                From(
	                                                Select 0 as JournalMasterID,0 as JournalEntryID,L.LedgerID,Case When E.EntityID is null then LedgerName else E.Name end as LedgerName,@FromDate Date,'' as JournalNo,'Opening Balance' as Particulars,
	                                                case when Sum(J.Debit)>Sum(J.Credit) then Sum(J.Debit)-Sum(J.Credit) else 0 end Debit ,
	                                                case when Sum(J.Credit)>Sum(J.Debit) then Sum(J.Credit)-Sum(J.Debit) else 0 end Credit
	                                                From AccJournalMaster JM
	                                                JOIN AccJournalEntry J on JM.JournalMasterID=J.JournalMasterID
	                                                JOIN AccLedger L on J.LedgerID=L.LedgerID
	                                                LEFT JOIN viEntity E on E.EntityID=L.EntityID
	                                                where JM.Date <@FromDate AND JM.IsSuccess = 1 and L.IsDeleted=0 and JM.IsDeleted=0 and JM.BranchID={CurrentBranchID} and (L.LedgerID=@LedgerID or @LedgerID=0) 
	                                                Group by L.LedgerID,LedgerName,E.EntityID,E.Name

	                                                UNION

	                                                Select JM.JournalMasterID,JournalEntryID,L.LedgerID,Case When E.EntityID is null then LedgerName else E.Name end as LedgerName,Date,JournalNoPrefix+CONVERT(varchar, JournalNo) as JournalNo,JM.Particular,Debit,Credit
	                                                AS Balance
	                                                from AccJournalEntry J
	                                                JOIN AccJournalMaster JM on JM.JournalMasterID=J.JournalMasterID
	                                                JOIN AccLedger L on L.LedgerID=J.LedgerID and L.IsDeleted=0
	                                                LEFT JOIN viEntity E on E.EntityID=L.EntityID
	                                                Where JM.Date between @FromDate and @ToDate and JM.IsSuccess=1 AND L.IsDeleted=0 and JM.IsDeleted=0  and JM.BranchID={CurrentBranchID} and (L.LedgerID=@LedgerID or @LedgerID=0)
                                                )as A
                                            )As B", generalLedgerReportPostModel);

            var groupedByLedgerId = genralLedgerReportItems.GroupBy(item => item.LedgerID);
            GeneralLedgerReportModel generalReportModel = new()
            {
                ClientName = await _dbContext.GetFieldsAsync<ViEntity, string?>("Name", $"EntityID={CurrentEntityID}", null),
                Accounts = groupedByLedgerId
                .Select(group =>
                    new GeneralLedgerReportGroupModel()
                    {
                        LedgerID = group.Key, // Assuming ParentID cannot be null
                        LedgerName = group.First().LedgerName,
                        Items = group
                            .Select(item => new GeneralLedgerReportGroupItemModel()
                            {
                                JournalMasterID = item.JournalMasterID,
                                JournalEntryID = item.JournalEntryID,
                                Date = item.Date,
                                JournalNo = item.JournalNo,
                                Particulars = item.Particulars,
                                Debit = item.Debit,
                                Credit = item.Credit,
                                Balance = item.Balance,
                            })
                            .ToList()
                    })
                .ToList()
            };
            return Ok(generalReportModel);
        }

        #endregion

        #region Journal Report

        [HttpPost("journal-report")]
        public async Task<IActionResult> GetJournalReport(JournalReportPostModel journalReportPostModel)
        {
            var journalReportData = await _dbContext.GetListByQueryAsync<JournalReportDataModel>(@$"SELECT JM.JournalMasterID,JM.Date,JM.JournalNoPrefix,JM.JournalNo,JM.Particular,L.LedgerName,J.Debit,J.Credit
                                                                    FROM AccJournalMaster JM
                                                                    Left join AccJournalEntry J on JM.JournalMasterID=J.JournalMasterID
                                                                    Left join AccLedger L on J.LedgerID=L.LedgerID 
                                                                    Where JM.Date between getutcdate() and getutcdate() and JM.BranchID=5 and JM.IsDeleted=0 and J.IsDeleted=0
                                                                    Order by JM.JournalNo", journalReportPostModel);

            var groupedByJournalMasterId = journalReportData.GroupBy(item => item.JournalMasterID);
            JournalReportModel journalReport = new()
            {
                ClientName = await _dbContext.GetFieldsAsync<ViEntity, string?>("Name", $"EntityID={CurrentEntityID}", null),
                Entries = groupedByJournalMasterId
                .Select(group =>
                    new JournalReportGroupModel()
                    {
                        JournalMasterID = group.Key, // Assuming ParentID cannot be null
                        Date = group.First().Date,
                        JournalNoWithPrefix = group.First().JournalNoPrefix + " / " + group.First().JournalNo,
                        Particular = group.First().Particular,
                        Items = group
                            .Select(ledger => new JournalReportGroupItemModel()
                            {
                                Account = ledger.LedgerName,
                                Debit = ledger.Debit,
                                Credit = ledger.Credit
                            })
                            .ToList()
                    })
                .ToList()
            };
            return Ok(journalReport);
        }

        #endregion

        #region Reciept and Disbursement

        //[HttpPost("reciept-and-disbursement-report")]
        //public async Task<IActionResult> GetRnDReport(ReceiptAndDisbursementReportReportPostModel receiptAndDisbursementReportReportPostModel)
        //{
        //}

        #endregion

        #region Sales By Item

        [HttpPost("sales-by-item-report")]
        public async Task<IActionResult> SalesByItemReport(SalesByItemReportPostModel salesByItemReportPostModel)
        {
            SalesByItemReportModel salesByItemReportModel = new()
            { 
                ClientName = await _dbContext.GetFieldsAsync<ViEntity, string?>("Name", $"EntityID={CurrentEntityID}", null)
            };
            string query = @$"Select ITM.ItemID As ID,ITM.ItemName,Sum(II.Quantity) As QuantitySold,Sum(I.NetAmount) As Amount
                                From Invoice I
                                Join InvoiceItem II ON I.InvoiceID=II.InvoiceID And II.IsDeleted=0
                                Join InvoiceType IT ON IT.InvoiceTypeID=I.InvoiceTypeID
                                Join InvoiceTypeNature INN ON INN.InvoiceTypeNatureID=IT.InvoiceTypeNatureID
                                Join viItem vI ON vI.ItemVariantID=II.ItemVariantID
                                Join Item ITM ON ITM.ItemID=vI.ItemID
                                Where I.IsDeleted=0 And I.BranchID={CurrentBranchID} And INN.InvoiceTypeNatureID<>{(int)InvoiceTypeNatures.Stock_Adjustment} And INN.InvoiceTypeNatureID={(int)InvoiceTypeNatures.Sales} And I.InvoiceDate Between @FromDate And @ToDate
                                Group By ITM.ItemID,ITM.ItemName";
            if(!salesByItemReportPostModel.IsByItem)
            {
                query = @$"Select vI.ItemID As ID,vI.ItemName,Sum(II.Quantity) As QuantitySold,Sum(I.GrossAmount) As Amount
                                From Invoice I
                                Join InvoiceItem II ON I.InvoiceID=II.InvoiceID And II.IsDeleted=0
                                Join InvoiceType IT ON IT.InvoiceTypeID=I.InvoiceTypeID
                                Join InvoiceTypeNature INN ON INN.InvoiceTypeNatureID=IT.InvoiceTypeNatureID
                                Join viItem vI ON vI.ItemVariantID=II.ItemVariantID
                                Where I.IsDeleted=0 And I.BranchID={CurrentBranchID} And INN.InvoiceTypeNatureID<>{(int)InvoiceTypeNatures.Stock_Adjustment} And INN.InvoiceTypeNatureID={(int)InvoiceTypeNatures.Sales} And I.InvoiceDate Between @FromDate And @ToDate
                                Group By vI.ItemID,vI.ItemName";
            }
			salesByItemReportModel.Items = await _dbContext.GetListByQueryAsync<SalesByItemReportItemModel>(query, salesByItemReportPostModel);
            return Ok(salesByItemReportModel);
        }

        #endregion

        #region Sales By Customer

        [HttpPost("sales-by-customer-report")]
        public async Task<IActionResult> SalesByCustomerReport(SalesByCustomerReportPostModel salesByCustomerReportPostModel)  
        {
            SalesByCustomerReportModel salesByCustomerReportModel = new() 
            {
                ClientName = await _dbContext.GetFieldsAsync<ViEntity, string?>("Name", $"EntityID={CurrentEntityID}", null)
            };
            salesByCustomerReportModel.Items = await _dbContext.GetListByQueryAsync<SalesByCustomerItemModel>(@$"Select I.CustomerEntityID,E.Name As CustomerName,Sum(I.NetAmount) As SalesAmount,Sum(I.GrossAmount) As SalesWithTaxAmount
                                From Invoice I
                                Join InvoiceItem II ON I.InvoiceID=II.InvoiceID And II.IsDeleted=0
                                Join InvoiceType IT ON IT.InvoiceTypeID=I.InvoiceTypeID
                                Join InvoiceTypeNature INN ON INN.InvoiceTypeNatureID=IT.InvoiceTypeNatureID
								Join viEntity E ON E.EntityID=I.CustomerEntityID
                                Where I.IsDeleted=0 And I.BranchID={CurrentBranchID} And INN.InvoiceTypeNatureID<>{(int)InvoiceTypeNatures.Stock_Adjustment} And INN.InvoiceTypeNatureID={(int)InvoiceTypeNatures.Sales} And I.InvoiceDate Between @FromDate And @ToDate
                                Group By I.CustomerEntityID,E.Name", salesByCustomerReportPostModel);
            return Ok(salesByCustomerReportModel ?? new());
        }

        #endregion

        #region Sales By Staff

        [HttpPost("sales-by-staff-report")]
        public async Task<IActionResult> SalesByStaffReport(SalesByStaffReportPostModel salesByStaffReportPostModel)
        {
            SalesByStaffReportModel salesByStaffReportModel = new()
            {
                ClientName = await _dbContext.GetFieldsAsync<ViEntity, string?>("Name", $"EntityID={CurrentEntityID}", null)
            };
            salesByStaffReportModel.Items = await _dbContext.GetListByQueryAsync<SalesByStaffReportItemModel>(@$"Select I.UserEntityID,E.Name As StaffName,Sum(I.NetAmount) As SalesAmount,Sum(I.GrossAmount) As SalesWithTaxAmount
                                From Invoice I
                                Join InvoiceItem II ON I.InvoiceID=II.InvoiceID And II.IsDeleted=0
                                Join InvoiceType IT ON IT.InvoiceTypeID=I.InvoiceTypeID
                                Join InvoiceTypeNature INN ON INN.InvoiceTypeNatureID=IT.InvoiceTypeNatureID
								Join viEntity E ON E.EntityID=I.UserEntityID
                                Where I.IsDeleted=0 And I.BranchID={CurrentBranchID} And INN.InvoiceTypeNatureID<>{(int)InvoiceTypeNatures.Stock_Adjustment} And INN.InvoiceTypeNatureID={(int)InvoiceTypeNatures.Sales} And I.InvoiceDate Between @FromDate And @ToDate
                                Group By I.UserEntityID,E.Name", salesByStaffReportPostModel);
            return Ok(salesByStaffReportModel ?? new());
        }

        #endregion

    }
}
