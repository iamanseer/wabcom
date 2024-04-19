using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.eCommerce.Customers
{
    public class EC_CustomerVerificationViewModel
    {
        public int EntityID { get; set; }
        public string? Phone { get; set; }
        public string? OTP { get; set; }
        public DateTime? AddedOn { get; set; }
        public int CustomerID { get; set; }
    }
}
