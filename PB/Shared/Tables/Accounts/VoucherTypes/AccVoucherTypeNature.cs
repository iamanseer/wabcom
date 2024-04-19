using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Accounts.VoucherTypes
{
    public class AccVoucherTypeNature : Table
    {
        [PrimaryKey]
        public int? VoucherTypeNatureID { get; set; }
        public string? VoucherTypeNatureName { get; set; }
        public bool ShowInList { get; set; }
        public int? SlNo { get; set; }
    }
}
