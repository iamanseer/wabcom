using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class WhatsappChat : Table
    {
        [PrimaryKey]
        public int ChatID { get; set; }
        public int? WhatsappAccountID { get; set; }
        public int? ContactID { get; set; }
        public int? MessageTypeID { get; set; }
        public string? Message { get; set; }
        public bool IsIncoming { get; set; }
        public string? MessageID { get; set; }
        public int? MessageStatus { get; set; }
        public DateTime? AddedOn { get; set; }
        public DateTime? SeenOn { get; set; }
        public DateTime? DeliveredOn { get; set; }
        public DateTime? FailedOn { get; set; }
        public string? FailedReason { get; set; }
        public int? ReplyID { get; set; }
        public int? SessionID { get; set; }
        public int? ConversationID { get; set; }
        public int? ReceipientID { get; set; }
        public int? AttachedMediaID { get; set; }
    }
}
