using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Accounts.VoucherTypes
{
    public class AccVoucherTypeSetting : Table
    {
        [PrimaryKey]
        public int ID { get; set; }
        public int? VoucherTypeID { get; set; }
        public int? BranchID { get; set; }
        public int? PeriodicityID { get; set; }
        public int? NumberingTypeID { get; set; }
        public string? Prefix { get; set; }
        public int StartingNumber { get; set; }
        public string? HeaderText { get; set; }
        public string? FooterText { get; set; }
        public string? Remarks { get; set; }
    }
}
