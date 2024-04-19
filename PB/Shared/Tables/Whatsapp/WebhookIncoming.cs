using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class WebhookIncoming : Table
    {
        [PrimaryKey]
        public int ID { get; set; }
        public string? Message { get; set; }
        public DateTime? AddedOn { get; set; }
        //public string? MessageFrom { get; set; }
        //public string? MessageTo { get; set; }
        public int? WhatsappAccountID { get; set; }
        public int? ContactID { get; set; }
        public int MessageType { get; set; }//1.Message, 2.Status
    }
}
