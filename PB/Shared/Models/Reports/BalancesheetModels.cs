using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Reports
{

    public class BalancesheetGetModel
    {
        public int ClientID { get; set; }
        public DateTime? Date { get; set; } = System.DateTime.Now.Date;
        public int? BranchID { get; set; }
    }
    public class BalancesheetReportModel 
    {
        public List<BalancesheetReportItemModel> DirectIncome { get; set; } = new();
        public List<BalancesheetReportItemModel> IndirectIncome { get; set; } = new();
        public List<BalancesheetReportItemModel> DirectExpense { get; set; } = new();
        public List<BalancesheetReportItemModel> IndirectExpense { get; set; } = new();
        public List<BalancesheetReportItemModel> Asset { get; set; } = new();
        public List<BalancesheetReportItemModel> Liability { get; set; } = new();

        public decimal TotalDirectIncome { get; set; }
        public decimal TotalIndirectIncome { get; set; }
        public decimal TotalDirectExpense { get; set; }
        public decimal TotalIndirectExpense { get; set; }
        public decimal TotalAsset { get; set; }
        public decimal TotalLiability { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal GrossLoss { get; set; }
        public decimal NetProfit { get; set; }
        public decimal NetLoss { get; set; }
        public string? ClientName { get; set; }
        public string? CurrencySymbol { get; set; } 
    }
    public class BalancesheetSPDataModel
    {
        public int AccountGorLID { get; set; }
        public int AccountGorLType { get; set; }
        public string? AccountGorLName { get; set; }
        public int ParentID { get; set; }
        public int Nature { get; set; }
        public decimal Amount { get; set; }
    }
    public class BalancesheetReportItemModel
    {
        public int ID { get; set; }
        public string? Particulars { get; set; }
        public decimal Amount { get; set; }
        public decimal TotalAmount { get; set; }
        public int NodeLevel { get; set; }
        public bool IsLedger { get; set; }

        private string? _LeftMarginStyle;

        public string? LeftMarginStyle
        {
            get
            {
                switch (NodeLevel)
                {
                    case 1:
                        _LeftMarginStyle = "td-p-12";
                        break;
                    case 2:
                        _LeftMarginStyle = "td-p-24";
                        break;
                    case 3:
                        _LeftMarginStyle = "td-p-36";
                        break;
                }
                return _LeftMarginStyle;
            }
        }

        public int ParentID { get; set; }
    }
}
