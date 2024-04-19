using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.SuperAdmin.PaymentGateway
{
    public class PaymentGatewayClientCredentialsModel
    {
        public string? MerchantID { get; set; }
        public string? AccessCode { get; set; }
        public string? WorkingKey { get; set; }
        public string? Salt { get; set; }
        public int GatewayType { get; set; }
        public string? PaymentInitiationLink { get; set; }
        public string? PaymentVerificationLink { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? EmailAddress { get; set; }
        public string? Phone { get; set; }
        public string? Amount { get; set; }//Added to use same model for encryption also
        public string? OrderID { get; set; }
        public string? JournalNo { get; set; }
    }
}
