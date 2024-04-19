using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class WhatsappConversation : Table
    {
        [PrimaryKey]
        public int ConversationID { get; set; }
        public DateTime? Date { get; set; }
        public string? WCID { get; set; }
        public bool IsUserInitiated { get; set; }
        public bool IsBillable { get; set; }
        public string? PricingModel { get; set; }
        public int? WhatsappAccountID { get; set; }
        public int? ContactID { get; set; }
    }
}
