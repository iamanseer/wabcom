using PB.Model;
using PB.Shared.Models.Inventory.Items;
using PB.Shared.Tables.Inventory.Items;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Item
{
    public class ItemVariantModel
    {
        public int ItemVariantID { get; set; }
        public int? ItemID { get; set; }
        public string? ItemModelName { get; set; }
        [Required(ErrorMessage = "Please enter the unieque url code for the item")]
        [RegularExpression("^[a-z0-9\\-]+$", ErrorMessage = "Please use only lowercase letters, numbers and hyphens.")]
        public string? UrlCode { get; set; }
        [Required(ErrorMessage = "Please choose the unit for the item")]
        public int? PackingTypeID { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please enter number of unit for the item")]
        public int UMUnit { get; set; } = 1;
        [Required(ErrorMessage = "Please enter the selling price for the item")]
        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }
        public int? SizeID { get; set; }
        public string? Size { get; set; }
        public string? PackingTypeName { get; set; }
        public int? ColorID { get; set; }
        public string? ColorName { get; set; }
        public int TaxPreferenceTypeID { get; set; }
        public string? TaxPreferenceName { get; set; }
        public bool IsGoods { get; set; } = false;
        public decimal? CurrentStock { get; set; }
        public List<ItemVariantImageModel> ItemVariantImages { get; set; } = new();
    }

    public class ItemVariantDetail
    {
        public int ItemVariantID { get; set; }
        public string? ItemName { get; set; }
        public string? Description { get; set; } 
        public decimal Price { get; set; }
        public int TaxPreferenceTypeID { get; set; }
        public string? TaxPreferenceName { get; set; } 
        public int? TaxCategoryID { get; set; } 
        public string? TaxCategoryName { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal? CurrentStock { get; set; }
        public bool IsGoods { get; set; } 
        public List<TaxCategoryItemModel> TaxCategoryItems { get; set; } = new();
    }
}
