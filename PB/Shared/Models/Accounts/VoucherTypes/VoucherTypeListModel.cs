using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Accounts.VoucherTypes
{
    public class VoucherTypeListModel
    {
        public int VoucherTypeID { get; set; }
        public string? VoucherTypeName { get; set; }
        public string? VoucherTypeNatureName { get; set; }
        public string? Prefix { get; set; }
    }
}
