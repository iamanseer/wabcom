using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class ChatbotMultiReply : Table
    {
        [PrimaryKey]
        public int MultiReplyID { get; set; }
        public int? ReplyID { get; set; }
        public int? MediaTypeID { get; set; }
        public int SlNo { get; set; }
        public int? MediaID { get; set; }
        public string? Caption { get; set; }
        public string? WMediaID { get; set; }
        public string? DocumentName { get; set; }
        public bool IsFirst { get; set; }
    }
}
