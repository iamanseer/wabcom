using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public class HangfireException :Table
    {
        [PrimaryKey]
        public int ID { get; set; }
        public string? Message { get; set; }
        public string? FunctionName { get; set; }
        public string? TemplateName { get; set; }
    }
}
