using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Accounts.JournalMaster
{
    public class AccJournalEntry : Table
    {
        [PrimaryKey]
        public int JournalEntryID { get; set; }
        public int? JournalMasterID { get; set; }
        [Required(ErrorMessage = "Please choose a ledger for the entry")]
        public int? LedgerID { get; set; }
        public string? Particular { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
    }
}
