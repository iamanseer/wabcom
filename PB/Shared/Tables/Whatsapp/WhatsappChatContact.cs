using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class WhatsappChatContact : Table
    {
        [PrimaryKey]
        public int WhatsappChatContactID { get; set; }
        public int? ChatID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FormattedName { get; set; }
    }
}
