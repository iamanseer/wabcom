using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Reports
{
    public class GeneralLedgerReportItemModel 
    {
        public int JournalMasterID { get; set; }
        public int JournalEntryID { get; set; } 
        public int LedgerID { get; set; }
        public string? LedgerName { get; set; }
        public DateTime Date { get; set; }
        public string? JournalNo { get; set; }
        public string? Particulars { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string? Balance { get; set; }
    }

    public class GeneralLedgerReportModel
    {
        public string? ClientName { get; set; }
        public List<GeneralLedgerReportGroupModel> Accounts { get; set; } = new(); 
    }

    public class GeneralLedgerReportGroupModel 
    {
        public int LedgerID { get; set; }
        public string? LedgerName { get; set; }
        public List<GeneralLedgerReportGroupItemModel> Items { get; set; } = new();

    }

    public class GeneralLedgerReportGroupItemModel
    {
        public int JournalMasterID { get; set; }
        public int JournalEntryID { get; set; }
        public DateTime Date { get; set; }
        public string? JournalNo { get; set; }
        public string? Particulars { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string? Balance { get; set; }
    }
}
