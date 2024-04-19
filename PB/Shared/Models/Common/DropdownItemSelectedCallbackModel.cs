using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class DropdownItemSelectedCallbackModel
    {
        public int? ID { get; set; }
        public string? Value { get; set; }
        public int DropdownMode { get; set; }
    }

    public class DropdownSelectedItemModel
    {
        public int? ID { get; set; }
        public string? Value { get; set; } 
    }
}
