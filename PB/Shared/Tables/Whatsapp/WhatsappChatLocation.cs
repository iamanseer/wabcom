using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class WhatsappChatLocation : Table
    {
        [PrimaryKey]
        public int ChatLocationID { get; set; }
        public int? ChatID { get; set; }
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Name { get; set; }
        public string? Url { get; set; }
    }
}
