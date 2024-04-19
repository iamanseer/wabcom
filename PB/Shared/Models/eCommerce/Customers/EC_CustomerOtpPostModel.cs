using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.eCommerce.Customers
{
    public class EC_CustomerOtpPostModel
    {
        [Required(ErrorMessage ="Please enter otp")]
        public string? Otp { get; set; }
        [Required(ErrorMessage = "Please enter phone number")]
        public string? PhoneNo { get; set; }
    }
}
