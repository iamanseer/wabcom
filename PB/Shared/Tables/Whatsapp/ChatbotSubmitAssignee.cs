using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class ChatbotSubmitAssignee : Table
    {
        [PrimaryKey]
        public int AssigneeID { get; set; }
        public int? ReplyID { get; set; }
        public int? EntityID { get; set; }
    }
}
