using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.Items
{
    public class ItemStock : Table
    {
        [PrimaryKey]
        public int StockID { get; set; }
        [Required(ErrorMessage = "Please choose an item")]
        public int? ItemVariantID { get; set; }
        public int? BranchID { get; set; }
        [Required(ErrorMessage = "Please provide quantity of the item")]
        public decimal? Quantity { get; set; }
    }
}
