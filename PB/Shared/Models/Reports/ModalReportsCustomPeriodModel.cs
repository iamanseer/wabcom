using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Reports
{
    public class ModalReportsCustomPeriodModel
    {
        public bool IsAdmin { get; set; } = false;
        public int ReportTypeID { get; set; } 
        public int PeriodTypeID { get; set; }  
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [RequiredIf(nameof(IsAdmin), true, ErrorMessage = "Please choose a branch for the report")]
        public int? BranchID { get; set; }
        [RequiredIf(nameof(ReportTypeID), (int)PB.Shared.Enum.Reports.Reports.GeneralLedger, ErrorMessage = "Please choose a ledger for the report")]
        public int? LedgerID { get; set; } 
        public string? LedgerName { get; set; }
    }
}
