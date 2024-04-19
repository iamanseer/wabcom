using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Accounts.Ledgers
{
    public class LedgerListModel
    {
        public int LedgerID { get; set; }
        public string? LedgerName { get; set; }
        public string? Alias { get; set; }
        public string? LedgerCode { get; set; }
        public string? AccountGroupName { get; set; }
        public string? AccountGroupTypeName { get; set; }
    }
}
