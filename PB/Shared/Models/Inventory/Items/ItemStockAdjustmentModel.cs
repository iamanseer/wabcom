using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Items
{
    public class ItemStockAdjustmentModel 
    {
        public int InvoiceID { get; set; }
        public DateTime? Date { get; set; } = DateTime.UtcNow;
        public List<ItemStockAdjustmentItemModel> Items { get; set; } = new();
    }


    public class ItemStockAdjustmentItemModel 
    {
        [Required(ErrorMessage = "Please choose an item to adjust stock")]
        public int? ItemVariantID { get; set; }
        public string? ItemName { get; set; }
        public decimal SystemStock { get; set; }
        [Required(ErrorMessage = "Please provide a physical stock quantity")]
        [Range(0,int.MaxValue, ErrorMessage = "Please enter a valid physical stock")]
        public decimal? PhysicalStock { get; set; }
        public bool IsRowEditMode { get; set; } = false;
    }


}
