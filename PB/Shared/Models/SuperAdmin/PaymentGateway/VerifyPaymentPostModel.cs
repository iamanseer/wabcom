using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.SuperAdmin.PaymentGateway
{
    public class VerifyPaymentPostModel
    {
        public int? BankTypeID { get; set; }
        public int? JournalMasterID { get; set; }
        public string? ReferenceNo { get; set; }
    }
}
