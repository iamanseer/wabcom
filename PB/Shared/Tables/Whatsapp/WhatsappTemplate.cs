using PB.Model;
using PB.Shared.Enum;
using PB.Shared.Enum.WhatsApp;
using PB.Shared.Helpers;
using System.ComponentModel.DataAnnotations;

namespace PB.Shared.Tables.Whatsapp
{
    public class WhatsappTemplate : Table
    {
        [PrimaryKey]
        public int TemplateID { get; set; }

        [MaxLength(512)]
        [Required(ErrorMessage = "Please prvide message template name")]
        [RegularExpression("^[a-z][a-z0-9_]*$", ErrorMessage = "The message template name can only have lowercase letters, underscores and numbers")]
        public string? TemplateName { get; set; }
        public int? ClientID { get; set; }
        [Required(ErrorMessage = "Please choose a whatsapp account")]
        public int? WhatsappAccountID { get; set; }
        [Required(ErrorMessage = "Please choose a category for template")]
        public int? CategoryID { get; set; }
        [Required(ErrorMessage = "Please choose a language for template")]
        public int? LanguageID { get; set; }
        public int? HeaderTypeID { get; set; } = (int)WhatsappTemplateHeaderType.NONE;

        [MaxLength(60, ErrorMessage = "The header content length should be less than 60 charecters")]
        [RequiredIf("HeaderTypeID", (int)WhatsappTemplateHeaderType.TEXT, ErrorMessage = "Please provide header content")]
        public string? HeaderText { get; set; }

        [RequiredIf("HeaderTypeID", (int)WhatsappTemplateHeaderType.MEDIA, ErrorMessage = "Please choose a media type for the header")]
        public int? HeaderMediaTypeID { get; set; }///WhatsappHeaderMediaType
        public int? HeaderMediaID { get; set; }
        public string? HeaderMediaHandle { get; set; }

        [Required(ErrorMessage = "Please provide content for body")]
        [MaxLength(1024, ErrorMessage = "The body content length shouldn't exceed than 1024 charecters")]
        public string? Body { get; set; }
        [MaxLength(60, ErrorMessage = "The body content length shouldn't exceed than 60 charecters")]
        public string? Footer { get; set; }
        public string? WhatsappTemplateID { get; set; }
        public string? StatusReason { get; set; }
        public int? StatusID { get; set; } 
    }
}
