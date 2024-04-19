using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class CloudAPITemplateModel
    {
        public string? name { get; set; }
        public string? language { get; set; }
        public string? category { get; set; }
        public List<TemplateComponentModel> components { get; set; } = new();
    }

    public class TemplateComponentModel
    {
        public string? type { get; set; }
        public string? format { get; set; }
        public string? text { get; set; }
        public TemplateBodyTextModel example { get; set; } = new();
        public List<TemplateButtonModel>? buttons { get; set; }
    }
    public class TemplateBodyTextModel
    {
        public List<List<string>>? body_text { get; set; }
        public List<string>? header_text { get; set; }
        public List<string>? header_handle { get; set; }
    }

    public class TemplateButtonModel
    {
        public string? type { get; set; }
        public string? text { get; set; }
        public string? phone_number { get; set; }
        public string? url { get; set; }
        public List<string?> example { get; set; } = new();
    }
}
