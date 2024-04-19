using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Accounts.VoucherTypes
{
    public class AccVoucherType : Table
    {
        [PrimaryKey] 
        public int VoucherTypeID { get; set; }
        [Required(ErrorMessage = "Please enter Voucher Type Name")]
        public string? VoucherTypeName { get; set; }
        [Required(ErrorMessage = "Please choose nature for the voucher type")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose nature for the voucher type")]
        public int? VoucherTypeNatureID { get; set; }
        public int? ClientID { get; set; }
    }
}
