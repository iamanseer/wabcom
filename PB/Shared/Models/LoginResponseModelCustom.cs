using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class LoginResponseModelCustom : LoginResponseModel
    {
        public int? PaymentStatus { get; set; }
        public int ClientID { get; set; }
        public int? RenewalStatus { get; set; }
        public int UserTypeID { get; set; }

    }
}
