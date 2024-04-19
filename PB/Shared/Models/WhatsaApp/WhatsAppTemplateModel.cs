using PB.Model;
using PB.Shared.Enum;
using PB.Shared.Enum.WhatsApp;
using PB.Shared.Tables;
using PB.Shared.Tables.Whatsapp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.WhatsaApp
{
    public class WhatsAppTemplateModel : WhatsappTemplate
    {
        public string? AccountName { get; set; }
        public string? LanguageName { get; set; }
        public List<WhatsappTemplateVariableModel> Variables { get; set; } = new();
        public List<WhatsappTemplateButtonModel> Buttons { get; set; } = new();
        //private bool _HasHeaderVariable; 
        public bool HasHeaderVariable { get; set; }
        //{
        //    get { return Variables.Any(item => item.Section == (int)WhatsappTemplateVariableSection.Header); }
        //    set { _HasHeaderVariable = value; } 
        //}

        //private bool _HasBodyVariable;
        public bool HasBodyVariable { get; set; }
        //{
        //    get { return Variables.Any(item => item.Section == (int)WhatsappTemplateVariableSection.Body); }
        //    set { _HasBodyVariable = value; }
        //}

        //private bool _HasButtonVariable; 
        public bool HasButtonVariable { get; set; }
        //{
        //    get { return Variables.Any(item => item.Section == (int)WhatsappTemplateVariableSection.Button); }
        //    set { _HasButtonVariable = value; }
        //}

        //private string? _PointerEventStyle;
        public string? PointerEventStyle { get; set; }
        //{
        //    get { return TemplateID > 0 ? "pointer-events:none;" : null; }
        //    set { _PointerEventStyle = value; }
        //}
    }

    public class TemplateBasicDetailsModel
    {
        public string? Name { get; set; }
        public string? LanguageName { get; set; }
        public string? CountryName { get; set; }
    }

    public class TemplateDetailsModel
    {
        public int? HeaderMediaID { get; set; }
        public int? HeaderTypeID { get; set; }
        public int? HeaderMediaTypeID { get; set; }
        public List<IdnValuePair> Variables { get; set; } = new();

    }

    public class WhatsappTemplateListModel
    {
        public int TemplateID { get; set; }
        public string? TemplateName { get; set; }
        public string? LanguageName { get; set; }
        public WhatsappTemplateCategory CategoryID { get; set; }
        public string? Name { get; set; }
        public int? StatusID { get; set; }

        public string? StatusBadgeClass
        {
            get
            {
                switch (StatusID)
                {
                    case (int)WhatsappTemplateStatus.APPROVED: return "badge bg-success";
                    case (int)WhatsappTemplateStatus.PENDING: return "badge bg-info";
                    case (int)WhatsappTemplateStatus.REJECTED: return "badge bg-danger";
                    case (int)WhatsappTemplateStatus.PAUSED: return "badge bg-warning";
                    case (int)WhatsappTemplateStatus.PENDING_DELETION: return "badge bg-dark";
                }
                return null;
            }
        }

public string? StatusReason { get; set; }
    }

    public class TemplateVariableListModel
    {
        public int? HeaderMediaID { get; set; }
        public int? HeaderTypeID { get; set; }
        public int? HeaderMediaTypeID { get; set; }
        public List<IdnValuePair> Variables { get; set; } = new();
    }

    public class KeyValueModel
    {
        public string? Key { get; set; }
        public string? Value { get; set; }
    }

    public class WhatsappTemplateVariableModel : WhatsappTemplateVariable
    {
        private string? _VariableNameClass;
        public string? VariableNameClass
        {
            get { return string.IsNullOrEmpty(VariableName) ? "input-invalid-border form-control" : "form-control"; }
            set { _VariableNameClass = value; }
        }

        private string? _SampleValueClass;
        public string? SampleValueClass
        {
            get { return string.IsNullOrEmpty(SampleValue) ? "input-invalid-border form-control" : "form-control"; }
            set { _SampleValueClass = value; }
        }
    }

    public class WhatsappTemplateButtonModel : WhatsappTemplateButton
    {
        public WhatsappTemplateVariableModel? ButtonVariable { get; set; } = null;
        public string? CountryName { get; set; }

        private string? _ButtonTypeClass;
        public string? ButtonTypeClass
        {
            get { return Type is null || Type is 0 ? "input-invalid-border form-control form-select" : "form-control form-select"; }
            set { _ButtonTypeClass = value; }
        }

        public string? ButtonUrlClass
        {
            get { return Type is not null && Type == (int)WhatsappTemplateButtonType.URL && string.IsNullOrEmpty(Url) ? "input-invalid-border form-control" : "form-control"; }
            set { ButtonUrlClass = value; }
        }

        private string? _ButtonUrlTypeClass;
        public string? ButtonUrlTypeClass
        {
            get { return Type is not null && Type == (int)WhatsappTemplateButtonType.URL && UrlType == 0 || UrlType == null ? "input-invalid-border form-control form-select" : "form-control form-select"; }
            set { _ButtonUrlTypeClass = value; }
        }

        private string? _ButtonTextClass;
        public string? ButtonTextClass
        {
            get { return string.IsNullOrEmpty(Text) ? "input-invalid-border form-control" : "form-control"; }
            set { _ButtonTextClass = value; }
        }

        private string? _CountryClass;
        public string? CountryClass
        {
            get { return Type != null && Type == (int)WhatsappTemplateButtonType.PHONE_NUMBER && CountryID == null ? "input-invalid-border pb-select form-control" : "pb-select form-control"; }
            set { _CountryClass = value; }
        }

        private string? _PhoneClass;
        public string? PhoneClass
        {
            get { return Type != null && Type == (int)WhatsappTemplateButtonType.PHONE_NUMBER && Phone == null ? "input-invalid-border form-control" : " form-control"; }
            set { _PhoneClass = value; }
        }
    }


}
