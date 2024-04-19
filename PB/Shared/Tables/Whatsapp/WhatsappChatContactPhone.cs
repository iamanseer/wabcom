using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class WhatsappChatContactPhone : Table
    {
        [PrimaryKey]
        public int WhatsappChatPhoneID { get; set; }
        public int? WhatsappChatContactID { get; set; }
        public string? Phone { get; set; }
        public string? WAID { get; set; }
        public string? Type { get; set; }
    }
}
