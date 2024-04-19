using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Item
{
    public class ItemListModel
    {
        public int ItemID { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public string? TaxPreferenceName { get; set; }
        public string? HSNSAC { get; set; }
        public bool IsGoods { get; set; }
        public string? QrCodeFileName { get; set; } 
        public int? TaxPreferenceTypeID { get; set; }

    }
}
