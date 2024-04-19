using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PB.Shared.Enum;

namespace PB.Shared.Tables.Inventory.Items
{
    public class ItemVariant : Table
    {
        [PrimaryKey]
        public int ItemVariantID { get; set; }
        public int? ItemID { get; set; }
        [Required(ErrorMessage = "Please choose the unit for the item")]
        public int? PackingTypeID { get; set; }
        // [Required(ErrorMessage = "Please enter number of unit for the item")]
        // [RequiredIf("UMUnit", targetValue: 0, ErrorMessage = "Unit never set to 0")]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter number of unit for the item")]
        public int UMUnit { get; set; } = 1;
        [Required(ErrorMessage = "Please enter the selling price for the item")]
        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }
        public int? SizeID { get; set; }
        public string? UrlCode { get; set; }
    }
}
