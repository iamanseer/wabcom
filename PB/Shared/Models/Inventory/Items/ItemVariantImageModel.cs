using PB.Shared.Tables.Inventory.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Item
{
    public class ItemVariantImageModel
    {
        public int ImageID { get; set; }
        public int? MediaID { get; set; }
        public int? ItemVariantID { get; set; }
        public string? AltText { get; set; }
        public string? FileName { get; set; }
    }
}
