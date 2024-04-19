using PB.Shared.Enum;
using PB.Shared.Enum.WhatsApp;
using PB.Shared.Tables.Whatsapp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.WhatsaApp
{
    public class WhatsappTemplateTableModel : WhatsappTemplate
    {
        public string? AccountName { get; set; }
        public string? LanguageName { get; set; }
        public string? FileName { get; set; } 
        public List<WhatsappTemplateVariableModel>? Variables { get; set; } = null;
        public List<WhatsappTemplateButtonModel>? Buttons { get; set; } = null;
        private bool _HasHeaderVariable; 
        public bool HasHeaderVariable
        {
            get{return Variables is not null && Variables.Any(item => item.Section == (int)WhatsappTemplateVariableSection.HEADER);}
            set { _HasHeaderVariable = value; }
        }

        private bool _HasBodyVariable;
        public bool HasBodyVariable
        {
            get { return Variables is not null && Variables.Any(item => item.Section == (int) WhatsappTemplateVariableSection.BODY);}
            set { _HasBodyVariable = value; }
        }

        private bool _HasButtonVariable; 
        public bool HasButtonVariable
        {
            get { return Variables is not null && Variables.Any(item => item.Section == (int)WhatsappTemplateVariableSection.BUTTON); }
            set { _HasButtonVariable = value; }
        }

        private string? _PointerEventStyle;
        public string? PointerEventStyle
        {
            get { return TemplateID > 0 ? "pointer-events:none;" : null; }
            set { _PointerEventStyle = value; }
        }
    }
}
