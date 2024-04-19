using PB.Shared.Tables.Accounts.VoucherTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Accounts.VoucherTypes
{
    public class VoucherTypeModel : AccVoucherType
    {
        public int ID { get; set; }
        public int? BranchID { get; set; }
        [Required(ErrorMessage = "Please choose a periodicity for the voucher type")]
        public int PeriodicityID { get; set; }
        [Required(ErrorMessage = "Please choose a numbering type for the voucher type")]
        public int NumberingTypeID { get; set; }
        [Required(ErrorMessage = "Please provide a prefix for the voucher type")]
        public string? Prefix { get; set; }
        [Range(1,int.MaxValue, ErrorMessage = "Please provide a minimum of 1")]
        [Required(ErrorMessage = "Please provide a starting number for the voucher type")]
        public int StartingNumber { get; set; } = 1;
        public string? HeaderText { get; set; }
        public string? FooterText { get; set; }
        public string? Remarks { get; set; }
    }
}
