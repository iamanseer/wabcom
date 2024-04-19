using PB.Shared.Tables.Accounts.JournalMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Accounts.VoucherEntry
{
    public class VoucherEntryModel : AccJournalMaster
    {
        //public string? BranchName { get; set; }
        public string? EntityName { get; set; }   
        public string? VoucherTypeName { get; set; }
        public string? VerifiedByName { get; set; } 
        public List<AccJournalEntryModel> JournalEntries { get; set; } = new();
        public bool IsAdmin { get; set; } = false;
        public int VoucherTypeNatureID { get; set; } 
    }
}
