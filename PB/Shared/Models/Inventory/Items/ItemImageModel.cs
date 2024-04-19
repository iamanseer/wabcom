using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Items
{
    public class ItemImageModel
    {
        public int ID { get; set; }
        public int? ItemID { get; set; }
        public int? MediaID { get; set; }
        public string? AltText { get; set; }
        public string? FileName { get; set; }
    }
}
