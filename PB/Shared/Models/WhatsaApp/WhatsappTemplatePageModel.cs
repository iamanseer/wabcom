using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.WhatsaApp
{
    using PB.Model;
    using PB.Shared.Enum.WhatsApp;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class TemplateVariablePageModel 
    {
        public string? VariableNameClass { get; set; }
        public string? SampleValueClass { get; set; }
        public int VariableID { get; set; }
        [Required(ErrorMessage = "Please provide variable name")]
        public string? VariableName { get; set; }
        [Required(ErrorMessage = "Please provide a data type of variable")]
        public int DataType { get; set; }
        public int TemplateID { get; set; }
        public int Section { get; set; }
        [Required(ErrorMessage = "Please provide sample value for the variable")]
        public string? SampleValue { get; set; }
        public int OrderNo { get; set; }
    }

    public class TemplateButtonPageModel 
    {
        public TemplateVariablePageModel? ButtonVariable { get; set; }
        public string? CountryName { get; set; }
        public string? ButtonTypeClass { get; set; }
        public string? ButtonUrlClass { get; set; }
        public string? ButtonUrlTypeClass { get; set; }
        public string? ButtonTextClass { get; set; }
        public string? CountryClass { get; set; }
        public string? PhoneClass { get; set; }
        public int ButtonID { get; set; }
        [Required(ErrorMessage = "Please provide button text")]
        public string? Text { get; set; }
        public int? TemplateID { get; set; }
        [Required(ErrorMessage = "Please choose a button type")]
        public int? Type { get; set; } = 0;
        [RequiredIf("Type", (int)WhatsappTemplateButtonType.URL, ErrorMessage = "Please choose a button url type")]
        public int? UrlType { get; set; }
        [RequiredIf("Type", (int)WhatsappTemplateButtonType.PHONE_NUMBER, ErrorMessage = "Please choose a phone country")]
        public int? CountryID { get; set; }
        [RequiredIf("Type", (int)WhatsappTemplateButtonType.URL, ErrorMessage = "Please provide a button url")]
        [RegularExpression("^https:\\/\\/[A-Za-z0-9.-]+(\\.[A-Za-z]+)+$", ErrorMessage = "Please provide a valid url start with https://")]
        public string? Url { get; set; }
        [RequiredIf("Type", (int)WhatsappTemplateButtonType.PHONE_NUMBER, ErrorMessage = "Please provide a phone number")]
        [RegularExpression("^\\d+$", ErrorMessage = "Phone number must be digits")]
        public string? Phone { get; set; }
        public int? VariableID { get; set; }
    }

    public class WhatsappTemplatePageModel
    {
        public string? AccountName { get; set; }
        public string? LanguageName { get; set; }
        public List<TemplateVariablePageModel>? Variables { get; set; }
        public List<TemplateButtonPageModel>? Buttons { get; set; }
        public bool HasHeaderVariable { get; set; }
        public bool HasBodyVariable { get; set; }
        public bool HasButtonVariable { get; set; }
        public string? PointerEventStyle { get; set; }
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
        public int? HeaderMediaTypeID { get; set; }
        public int? HeaderMediaID { get; set; }
        public string? HeaderMediaHandle { get; set; }

        [Required(ErrorMessage = "Please provide content for body")]
        [MaxLength(1024, ErrorMessage = "The body content length shouldn't exceed than 1024 charecters")]
        public string? Body { get; set; }
        [MaxLength(60, ErrorMessage = "The body content length shouldn't exceed than 60 charecters")]
        public string? Footer { get; set; }
        public string? WhatsappTemplateID { get; set; }
        public int? StatusID { get; set; } 
        public string? StatusReason { get; set; }
        public string? FileName { get; set; } 
    }

}
