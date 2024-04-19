using PB.Shared.Tables.Whatsapp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.WhatsaApp
{
    public class TemplateDeleteDetaisModel : WhatsappAccount
    {
        public string? TemplateName { get; set; }
        public string? WhatsappTemplateID { get; set; } 
    }
}
