using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Item
{
    public class ItemDetailsForOrderAppModel
    {
        public int ItemVariantID { get; set; } 
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public int Quantity { get; set; } = 1;
        public string? Description { get; set; }
        public decimal MRP { get; set; }
        public decimal SellingPrice { get; set; }
        public string? Symbol { get; set; } 
        public List<string?>? Images { get; set; }  

    }
}
