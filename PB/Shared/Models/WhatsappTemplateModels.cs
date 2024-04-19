using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class TemplateSendParameters
    {
        public List<TemplateComponent>? Components { get; set; }
        public List<TemplateButton>? Buttons { get; set; } 
    }

    public class TemplateComponent
    {
        public string? type { get; set; }
        public List<TemplateParameter>? parameters { get; set; }
        public string? sub_type { get; set; }
        public int index { get; set; }
    }

    public class TemplateButton
    {
        public string? type { get; set; }
        public string? sub_type { get; set; }
        public int index { get; set; }
        public string? url { get; set; } 
    }

    public class TemplateParameter
    {
        public string? type { get; set; }
        public TemplateImage? image { get; set; }
        public TemplateDocument? document { get; set; }
        public TemplateImage? video { get; set; }
        public string? text { get; set; }
        public TemplateCurrency? currency { get; set; }
        public TemplateDateTime? date_time { get; set; }
        public string? payload { get; set; }
    }

    public class TemplateImage
    {
        public string? link { get; set; }
    }

    public class TemplateDocument
    {
        public string? link { get; set; }
        public string? id { get; set; }
        public string? filename { get; set; }
    }

    public class TemplateCurrency
    {
        public string? fallback_value { get; set; }
        public string? code { get; set; }
        public int amount_1000 { get; set; }
    }

    public class TemplateDateTime
    {
        public string? fallback_value { get; set; }
    }


    public class SessionResponse
    {
        public string? id { get; set; }
    }

    public class MediaUploadSessionResponse
    {
        public string? h { get; set; }
    }
}
