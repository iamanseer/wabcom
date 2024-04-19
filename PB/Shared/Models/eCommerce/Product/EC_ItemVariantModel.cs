using PB.Shared.Models.Inventory.Item;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.eCommerce.Product
{
    public class EC_ItemVariantModel
    {
        public int ItemVaraintID { get; set; }
        public string? ItemVariantName { get; set; } 
        public int? ItemID { get; set; }
        [Required(ErrorMessage = "Please choose the unit for the item")]
        public int? PackingTypeID { get; set; }
        public string? PackingTypeName { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please enter number of unit for the item")]
        public int? UmUnit { get; set; }
        public int? SizeID { get; set; }
        public string? SizeName { get; set; } 
        public int? ColorID { get; set; }
        public string? ColorName { get; set; }
        public int TaxPreferenceTypeID { get; set; }
        public string? TaxPreferenceName { get; set; }
        public bool IsGoods { get; set; } = false;
        public decimal? CurrentStock { get; set; }
        [Required(ErrorMessage = "Please enter the selling price for the item")]
        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }
        [Required(ErrorMessage = "Please enter the slug url code for the item")]
        [RegularExpression("^[a-z0-9\\-]+$", ErrorMessage = "Please use only lowercase letters, numbers and hyphens.")]
        public string? SlugUrl { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? OgTitle { get; set; }
        public string? OgDescription { get; set; }
        public int? OgMediaID { get; set; }
        public List<ItemVariantImageModel> ItemVariantImages { get; set; } = new();
    }
}
