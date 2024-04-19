using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.WhatsaApp
{
    public class Example
    {
        public List<string>? header_text { get; set; }
        public List<List<string>>? body_text { get; set; }
        public List<string>? example { get; set; }
        public List<string>? header_handle { get; set; }
    }

    public class Button
    {
        public string? type { get; set; }
        public string? text { get; set; }
        public string? url { get; set; }
        public string? phone_number { get; set; }
        public List<string>? example { get; set; } 
    }

    public class Component
    {
        public string? type { get; set; }
        public string? format { get; set; }
        public string? text { get; set; }
        public Example? example { get; set; }
        public List<Button>? buttons { get; set; }
    }

    public class Template
    {
        public string? name { get; set; }
        public List<Component> components { get; set; } = new();
        public string? language { get; set; }
        public string? status { get; set; }
        public string? category { get; set; }
        public string? id { get; set; }
    }

    public class Cursors
    {
        public string? before { get; set; }
        public string? after { get; set; }
    }

    public class Paging
    {
        public Cursors? cursors { get; set; }
        public string? next { get; set; }
        public string? previous { get; set; }
    }

    public class WhatsappTemplatePagedListModel
    {
        public List<Template> data { get; set; } = new();
        public Paging? paging { get; set; }
    }
}
