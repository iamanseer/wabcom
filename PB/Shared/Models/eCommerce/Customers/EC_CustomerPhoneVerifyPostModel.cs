using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.eCommerce.Customers
{
    public class EC_CustomerPhoneVerifyPostModel
    {
        [Required(ErrorMessage ="Please enter phone number")]
        public string? PhoneNo { get; set; }
    }

    public class OtpViewModel
    {
        public string? OTP { get; set; }
        public DateTime? OtpGeneratedOn { get; set; }
    }
}
