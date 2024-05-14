using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.CRM.Customer
{
    public class CustomerSubscriptionModel
    {
        public int SubscriptionID { get; set; }
        [Required (ErrorMessage = "Please enter customer subscription name")]
        [StringLength(maximumLength: 30, MinimumLength = 2, ErrorMessage = "Length must be between 2 and 30 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Customer subscription name field should only contain alphabetic characters and spaces")]
        public string? SubscriptionName { get; set; }
        public int? ClientID { get; set; }
    }
}
