using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Model.Models
{
    public class ViewPageMenuModel
    {
        public int ID { get; set; }
        public string? MenuName { get; set; }
        public bool IsChecked { get; set; }
    }
}
