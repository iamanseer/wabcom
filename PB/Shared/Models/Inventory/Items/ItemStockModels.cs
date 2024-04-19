using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Items
{
    public class ItemStockViewModel
    {
        public int ItemVariantID { get; set; }
        public string? ItemName { get; set; }
        public decimal Quantity { get; set; } 
    }  
}
