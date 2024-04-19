using PB.CRM.Model.Enum;
using PB.Model;
using PB.Shared.Enum;
using PB.Shared.Enum.CRM;

using PB.Shared.Models.WhatsaApp;
using PB.Shared.Tables;

using PB.Shared.Tables.Whatsapp;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class WhatsappTemplateModel
    {
        public int TemplateID { get; set; }
        [MaxLength(512)]
        [Required(ErrorMessage = "Please enter template name")]
        [RegularExpression(@"^\S+$", ErrorMessage = "Spaces are not allowed in the Template Name.")]
        public string? TemplateName { get; set; }
        public int? WhatsappAccountID { get; set; }
        public int? LanguageID { get; set; }
        public int? CategoryID { get; set; }
        public int? HeaderTypeID { get; set; } //WhatsappHeaderType
        [MaxLength(60)]
        public string? HeaderText { get; set; }
        public List<WhatsappTemplateVariable>? HeaderVariables { get; set; } = new();
        public int? HeaderMediaTypeID { get; set; }///WhatsappHeaderMediaType
        public int? HeaderMediaID { get; set; }
        public string? HeaderMediaHandle { get; set; }
        [MaxLength(1024, ErrorMessage = "Body length cannot exceed 1024")]
        public string? Body { get; set; }
        public List<WhatsappTemplateVariable>? BodyVariables { get; set; } = new();
        [MaxLength(60)]
        public string? Footer { get; set; }
        public int? FooterButtonTypeID { get; set; }//WhatsappFooterButtonType
        public bool HasCallButton { get; set; }
        [MaxLength(25)]
        public string? CallButtonText { get; set; }
        public int? CallCountryID { get; set; }
        public string? CallCountryName { get; set; }
        [MaxLength(20)]
        public string? CallPhoneNo { get; set; }
        public bool HasVisitButton { get; set; }
        [MaxLength(25)]
        public string? VisitButton1Text { get; set; }
        public int? VisitButton1TypeID { get; set; }//WhatsappVisitLinkType
        public string? VisitButton1URL { get; set; }
        public string? VisitButton2Text { get; set; }
        public int? VisitButton2TypeID { get; set; }//WhatsappVisitLinkType 
        public string? VisitButton2URL { get; set; }
        public List<WhatsappTemplateVariable>? ButtonVariables { get; set; } = new(); 
        public string? WhatsappTemplateID { get; set; }
        public string? WhatsappTemplateStatus { get; set; }
        public string? QuickButton1 { get; set; }
        public string? QuickButton2 { get; set; }
        public string? QuickButton3 { get; set; }
        public string? Name { get; set; }
        public string? LanguageName { get; set; }
    }

}
