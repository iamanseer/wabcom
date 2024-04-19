using PB.Model;
using PB.Shared.Enum;
using PB.Shared.Enum.WhatsApp;
using PB.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class WhatsappTemplateButton : Table
    {
        [PrimaryKey]
        public int ButtonID { get; set; }
        public string? Text { get; set; }
        public int? TemplateID { get; set; }
        public int? Type { get; set; }
        public string? Url { get; set; }
        public int? UrlType { get; set; }
        public string? Phone { get; set; }
        public int? CountryID { get; set; }
        public int? VariableID { get; set;}
    }
}
