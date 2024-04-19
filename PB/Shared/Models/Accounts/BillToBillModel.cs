using PB.Model;
using PB.Shared.Enum;
using PB.Shared.Enum.Accounts;
using PB.Shared.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Accounts
{
    public class BillToBillModel
    {
        public int BillID { get; set; }

        [Required(ErrorMessage = "Please provide a date")]
        public DateTime? Date { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Please choose a Reference type")]
        public int? ReferenceTypeID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Reference number")]
        public string? ReferenceNo { get; set; }
        public int? JournalEntryID { get; set; }
        [Helpers.RequiredIfMultiple(nameof(ReferenceTypeID), new int[] { (int)ReferenceTypes.OldReference, (int)ReferenceTypes.OldReference }, ErrorMessage = "Please provide mobile number for the account")]
        public int? Days { get; set; }
        public int? LedgerID { get; set; }

        [RequiredIf(nameof(IsDebit),true, ErrorMessage = "Please provide debit amount")]
        public decimal? Debit { get; set; }

        [RequiredIf(nameof(IsDebit), false, ErrorMessage = "Please provide credit amount")]
        public decimal? Credit { get; set; }
        public int? ParentBillID { get; set; }
        public string? LedgerName { get; set; }
        public bool IsDebit { get; set; }
        public decimal MaximumAmount { get; set; }
    }
}
