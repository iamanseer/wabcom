using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Enum.Accounts
{
    public class GeneralLedgerReportSearchModel
    {
        [Required]
        public int LedgerID { get; set; }
        [Required]
        public DateTime? FromDate { get; set; } = DateTime.Now;
        [Required]
        public DateTime? ToDate { get; set; } = DateTime.Now;
    }

    public class GeneralLedgerReportResultModel
    {
        public int LedgerID { get; set; }
        public string? LedgerName { get; set; }
        public DateTime Date { get; set; }
        public string? JournalNo { get; set; }
        public string? Particulars { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string? Balance { get; set; }
    }
}
