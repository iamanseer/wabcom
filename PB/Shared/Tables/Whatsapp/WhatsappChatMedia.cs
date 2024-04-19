using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class WhatsappChatMedia : Table
    {
        [PrimaryKey]
        public int ChatMediaID { get; set; }
        public int? ChatID { get; set; }
        public string? Caption { get; set; }
        public string? MimeType { get; set; }
        public string? Sha256 { get; set; }
        public string? WMediaID { get; set; }
        public string? WMediaURL { get; set; }
        public int? MediaID { get; set; }
        public bool IsVoice { get; set; }
        public bool IsAnimated { get; set; }
        public string? DocumentName { get; set; }
    }
}
