using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class WhatsappChatSession : Table
    {
        [PrimaryKey]
        public int SessionID { get; set; }
        public int? WhatsappAccountID { get; set; }
        public int? ContactID { get; set; }
        public DateTime? LastMessageOn { get; set; }
        public bool IsFinished { get; set; }
        public bool HasSupportChat { get; set; }
        public int? EnquiryID { get; set; }
    }
}
