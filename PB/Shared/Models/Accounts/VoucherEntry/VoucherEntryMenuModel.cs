using PB.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Accounts.VoucherEntry
{
    public class VoucherEntryMenuModel : ViewPageMenuModel
    {
        public string? JournalNoPrefix { get; set; }
        public int JournalNo { get; set; } 
    }
}
