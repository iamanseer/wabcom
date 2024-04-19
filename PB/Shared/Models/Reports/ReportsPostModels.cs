using PB.Model;
using PB.Shared.Enum;
using PB.Shared.Enum.Reports;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Reports
{
    public class ProfitAndLossReportPostModel
    {
        [Required(ErrorMessage = "Please provide a start date for the report")]
        public DateTime? FromDate { get; set; }
        [Required(ErrorMessage = "Please provide a end date for the report")]
        public DateTime? ToDate { get; set; }
    }

    public class BalanceSheetReportPostModel
    {
        [Required(ErrorMessage = "Please provide a date for the report")]
        public DateTime? Date { get; set; }
        public bool IsAdmin { get; set; } = false;
        [RequiredIf(nameof(IsAdmin), true, ErrorMessage = "Please choose a branch")]
        public int? BranchID { get; set; }
    }

    public class TrialBalanceReportPostModel
    {
        [Required(ErrorMessage = "Please provide a date for the report")]
        public DateTime? Date { get; set; }
        public bool IsAdmin { get; set; } = false;
        [RequiredIf(nameof(IsAdmin), true, ErrorMessage = "Please choose a branch")]
        public int? BranchID { get; set; }
    }

    public class GeneralLedgerReportPostModel
    {
        public bool IsAdmin { get; set; } = false;
        [Required(ErrorMessage = "Please choose a ledger to get the report")]
        public int? LedgerID { get; set; }
        public string? LedgerName { get; set; }
        [Required(ErrorMessage = "Please provide a start date to get the report")]
        public DateTime? FromDate { get; set; }
        [Required(ErrorMessage = "Please provide a end date to get the report")]
        public DateTime? ToDate { get; set; }
        [RequiredIf(nameof(IsAdmin), true, ErrorMessage = "Please choose a branch for the report")]
        public int? BranchID { get; set; }
    }

    public class JournalReportPostModel
    {
        public bool IsAdmin { get; set; } = false;
        [Required(ErrorMessage = "Please provide a start date to get the report")]
        public DateTime? FromDate { get; set; }
        [Required(ErrorMessage = "Please provide a end date to get the report")]
        public DateTime? ToDate { get; set; }
        [RequiredIf(nameof(IsAdmin), true, ErrorMessage = "Please choose a branch for the report")]
        public int? BranchID { get; set; } 
    }

    public class ReceiptAndDisbursementReportPostModel 
    {
    }

    public class SalesByItemReportPostModel
    {
        public bool IsAdmin { get; set; } = false;
        [Required(ErrorMessage = "Please provide a start date to get the report")]
        public DateTime? FromDate { get; set; }
        [Required(ErrorMessage = "Please provide a end date to get the report")]
        public DateTime? ToDate { get; set; }
        [RequiredIf(nameof(IsAdmin), true, ErrorMessage = "Please choose a branch for the report")]
        public int? BranchID { get; set; }
        public bool IsByItem { get; set; } = false;
    }

    public class SalesByCustomerReportPostModel
    {
        public bool IsAdmin { get; set; } = false;
        [Required(ErrorMessage = "Please provide a start date to get the report")]
        public DateTime? FromDate { get; set; }
        [Required(ErrorMessage = "Please provide a end date to get the report")]
        public DateTime? ToDate { get; set; }
        [RequiredIf(nameof(IsAdmin), true, ErrorMessage = "Please choose a branch for the report")]
        public int? BranchID { get; set; }
    }

    public class SalesByStaffReportPostModel 
    {
        public bool IsAdmin { get; set; } = false;
        [Required(ErrorMessage = "Please provide a start date to get the report")]
        public DateTime? FromDate { get; set; }
        [Required(ErrorMessage = "Please provide a end date to get the report")]
        public DateTime? ToDate { get; set; }
        [RequiredIf(nameof(IsAdmin), true, ErrorMessage = "Please choose a branch for the report")]
        public int? BranchID { get; set; }
    }
} 
