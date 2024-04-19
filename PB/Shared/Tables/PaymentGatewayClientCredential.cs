using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public class PaymentGatewayClientCredential:Table
    {
        [PrimaryKey]
        public int? CredentialID { get; set; }
        public int? ClientID { get; set; }
        [Required(ErrorMessage ="Please choose payment gateway")]
        [Range(1,int.MaxValue,ErrorMessage = "Please choose valid payment gateway")]
        public int? GatewayID { get; set; }
        public bool IsLive { get; set; }
        public string? MerchantID { get; set; }
        public string? AccessCode { get; set; }
        public string? WorkingKey { get; set; }
        public string? Salt { get; set; }
    }
}
