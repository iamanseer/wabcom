using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.SuperAdmin.PaymentGateway
{
    public class TransactionDetailsViewModel
    {
        public string? mihpayid { get; set; }
        public string? status { get; set; }
        public string? mode { get; set; }
        public string? bank_ref_num { get; set; }
        public string? error_Message { get; set; }
        public string? JSONConversionErrorMessage { get; set; }
        public string? JSONConversionErrorTitle { get; set; }
        public string? tracking_id { get; set; }
        public bool? payment_status { get; set; }

    }
}
