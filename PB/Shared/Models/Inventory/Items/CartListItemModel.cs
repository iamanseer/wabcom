using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Items
{
    public class CartListItemModel
    {
        public int EnquiryItemID { get; set; }
        public int ItemVariantID { get; set; }
        public int EnquiryID { get; set; }
        public decimal Quantity { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; } 
        public string? FileName { get; set; }
        public string? Symbol { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; } 
    }
}
