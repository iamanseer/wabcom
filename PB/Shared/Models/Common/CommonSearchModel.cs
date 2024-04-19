using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Model.Models
{
    public class CommonSearchModel
    {
        public int ID { get; set; }
        public string? SearchString { get; set; } = "";
        public bool ReadDataOnSearch { get; set; } = false;
        public int DropdownMode { get; set; } 
    }
}
