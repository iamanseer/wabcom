using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Common
{
    public class LedgerSearchModel
    {
        public int VoucherTypeID { get; set; }
        public string? SearchString { get; set; }
        public bool ReadDataOnSearch { get; set; } 
        public int DropdownMode { get; set; }
        public int DrOrCr { get; set; } 
    }
}
