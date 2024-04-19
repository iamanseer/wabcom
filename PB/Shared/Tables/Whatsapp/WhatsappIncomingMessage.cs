using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class WhatsappIncomingMessage : Table
    {
        [PrimaryKey]
        public int IncomingMessageID { get; set; }
        public string? Phone { get; set; }
        public string? Message { get; set; }
        public DateTime? AddedOn { get; set; } = DateTime.UtcNow;
        public string? DisplayPhoneNumber { get; set; }
        public string? PhoneNumberId { get; set; }
        public string? EntryID { get; set; }
        public string? WaId { get; set; }
        public string? MessageID { get; set; }
        public string? MessageType { get; set; }
    }
}
