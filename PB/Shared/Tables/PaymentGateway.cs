using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public class PaymentGateway:Table
    {
        [PrimaryKey]
        public int? GatewayID { get; set; }
        [Required(ErrorMessage ="Please enter payment gateway name")]
        [StringLength(50,ErrorMessage ="Gateway Name should be at most 50 characters")]
        public string? GatewayName { get; set; }
        [Required(ErrorMessage = "Please enter test link")]
        public string? PaymentInitiationTestLink { get; set; }
        [Required(ErrorMessage ="Please enter live link")]
        public string? PaymentInitiationLiveLink { get; set; }
        [Required(ErrorMessage = "Please enter test link")]
        public string? PaymentVerificationTestLink { get; set; }
        [Required(ErrorMessage = "Please enter live link")]
        public string? PaymentVerificationLiveLink { get; set; }
        [Required(ErrorMessage = "Please choose gateway type")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose valid gateway type")]
        public int GatewayType { get; set; }
        public bool ShowMerchantID { get; set; }
        public bool ShowAccessCode { get; set; }
        public bool ShowWorkingKey { get; set; }
        public bool ShowSalt { get; set; }
       
    }
}
