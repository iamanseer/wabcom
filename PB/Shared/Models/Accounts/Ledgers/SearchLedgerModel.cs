using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Accounts.Ledgers
{
    public class SearchLedgerModel
    {
        public string? SearchString { get; set; }
        public bool ReadDataOnSearch { get; set; }
        public List<int> GroupTypeIdsIn { get; set; } = new();
        public List<int> GroupTypeIdsNotIn { get; set; } = new(); 
    }
}
