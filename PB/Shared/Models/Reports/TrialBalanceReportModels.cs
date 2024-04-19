using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Reports
{

    public class TrialBalanceReportItemModel
    {
        public int LedgerID { get; set; } 
        public string? LedgerName { get; set; }
        public int AccountGroupID { get; set; }
        public string? AccountGroupName { get; set; } 
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }

    }


    public class TrialBalanceReportModel 
    {
        public string? ClientName { get; set; }
        public DateTime? Date { get; set; } 
        public string? CurrencySymbol { get; set; }
        public List<TrialBalanceReportGroupModel> Groups { get; set; } = new(); 
    }

    public class TrialBalanceReportGroupModel
    {
        public int AccountGroupID { get; set; }
        public string? AccountGroupName { get; set; }
        public bool ShowItems { get; set; } = false;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public List<TrialBalanceReportGroupItemModel> GroupItems { get; set; } = new(); 
    }

    public class TrialBalanceReportGroupItemModel
    {
        public int LedgerID { get; set; }
        public string? LedgerName { get; set; }
        public int AccountGroupID { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }


}
