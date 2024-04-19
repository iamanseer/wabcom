using PB.Model;
using PB.Shared.Enum;
using PB.Shared.Tables.Accounts.JournalMaster;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Accounts.VoucherEntry
{
    public class AccJournalEntryModel 
    {
        [Required(ErrorMessage = "Please choose a ledger for the entry")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a valid ledger for the entry")]
        public int? LedgerID { get; set; }
        public string? LedgerName { get; set; }
        public string? Particular { get; set; }
        [RequiredIf(nameof(DrOrCr),(int)DebitOrCredit.Debit,ErrorMessage = "Please provide debit amount")]
        public decimal? Debit { get; set; }
        [RequiredIf(nameof(DrOrCr), (int)DebitOrCredit.Credit, ErrorMessage = "Please provide credit amount")]
        public decimal? Credit { get; set; }
        [Required(ErrorMessage = "Please choose debit or credit")]
        [Range(1,2,ErrorMessage ="Please choose a valid entry type")]
        public int DrOrCr {get; set; }
        public bool RowEditMode { get; set; }
        public int GroupTypeID { get; set; }    
        public List<BillToBillModel>? BillToBillReferences { get; set; }
    }
}
