using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Enum.Reports
{
    public enum ReportTypes
    {
        BusinessOverview = 1,
        Sales,
        Inventory
    }
    public enum Reports
    {
        //Accounts
        PnLReport = 1,
        BalanceSheet,
        TrialBalance,
        GeneralLedger,
        JournalReport,
        ReceiptAndDisbursementReport,


        //Sales
        SalesByCustomer = 20,
        SalesByItem,
        SalesByStaff,


        //Inventory
        InventorySummary = 40,
        CommittedStockDetails,


    }
}
