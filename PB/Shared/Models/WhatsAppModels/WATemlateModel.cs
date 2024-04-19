using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.WhatsAppModels
{
    public class Example
    {
        public object? body_text { get; set; }
        public object? header_text { get; set; }
        public object? header_handle { get; set; }
    }

    public class Button
    {
        public string? type { get; set; }
        public string? text { get; set; }
        public string? phone_number { get; set; }
        public string? url { get; set; }
        public List<string?>? example { get; set; }
    }

    public class Component
    {
        public string? type { get; set; }
        public string? format { get; set; }
        public string? text { get; set; }
        public Example? example { get; set; }
        public List<Button>? buttons { get; set; }
    }

    public class WANewTemplatePostModel 
    {
        public string? name { get; set; }
        public string? language { get; set; }
        public string? category { get; set; }
        public List<Component>? components { get; set; }
    }

    public class WATemplateEditPostModel
    {
        public string? category { get; set; }
        public List<Component>? components { get; set; }
    }

    public class ErrorModel
    {
        public string? message { get; set; }
        public string? type { get; set; }
        public int code { get; set; }
        public int error_subcode { get; set; }
        public bool is_transient { get; set; }
        public string? error_user_title { get; set; }
        public string? error_user_msg { get; set; }
        public string? fbtrace_id { get; set; }
    }

    public class CreateTemplateResultModel
    {
        public string? id { get; set; }
        public string? status { get; set; }
        public string? category { get; set; }
        public ErrorModel? error { get; set; } 
    }

    public class EditTemplateResponseModel
    {
        public string? success { get; set; }
        public string? message { get; set; }
        public ErrorModel? error { get; set; }
    }
}
