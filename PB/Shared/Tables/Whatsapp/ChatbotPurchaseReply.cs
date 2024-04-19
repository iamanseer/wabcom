using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class ChatbotPurchaseReply:Table
    {
        [PrimaryKey]
        public int PurchaseReplyID { get; set; }
        public int? ReplyID { get; set; }
        public string? QuantityLabel { get; set; }
        public string? PaymentLabel { get; set; }
        public string? ValidationFailedMessage { get; set; }
        public string? PaymentSuccessMessage { get; set; }
        public string? PaymentFailedMessage { get; set; }
    }
}
