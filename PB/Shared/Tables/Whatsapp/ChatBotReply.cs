using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class ChatbotReply : Table
    {
        [PrimaryKey]
        public int ReplyID { get; set; }
        public int ReplyTypeID { get; set; }
        public int? ParentReplyID { get; set; }
        [Required]
        public int Code { get; set; }
        [Required]
        public string? Short { get; set; }
        public string? SmallDescription { get; set; }
        public string? Header { get; set; }
        public string? Footer { get; set; }
        public string? Detailed { get; set; }
        public int? WhatsappAccountID { get; set; }
        public string? ListButtonText { get; set; }
        public bool NeedConfirmation { get; set; }
        public int? ItemVariantID { get; set; }
        public string? SearchKey { get; set; }
        public bool NotifyUser { get; set; }
    }
}
