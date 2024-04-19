using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.SuperAdmin.PaymentGateway
{
    public class PaymentInitiationModel:PaymentGatewayClientCredentialsModel
    {
        public string? ResponseAPIURL { get; set; }
        public string? Hash { get; set; }
        public string? ProductInfo { get; set; }
    }
}
