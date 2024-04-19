using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.eCommerce.Common
{
    public class PhoneNoVerificationResultModel:Success
    {
        public DateTime? OtpAddedOn { get; set; }
    }


    public class EC_ItemCategorySuccessModel : Success
    {
        public int CategoryID { get; set; }
        public string? CategoryName { get; set; }
    }
}
