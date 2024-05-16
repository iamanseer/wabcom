
using System.ComponentModel.DataAnnotations;
using PB.Model;
using PB.Model.Tables;
using PB.Shared.Enum;
using PB.Shared.Enum.Inventory;
using PB.Shared.Models.Inventory.Item;
using PB.Shared.Tables.Inventory.Items;

namespace PB.Shared.Models.Inventory.Items
{
    public class ItemSingleModel
    {
        public int ItemID { get; set; }
        [Required(ErrorMessage = "Please provide a name for the item")]
        public string? ItemName { get; set; }
        //[Required(ErrorMessage = "Please provide a code for the item")]
        //[RegularExpression("^[A-Za-z0-9-]+$", ErrorMessage = "The item code must consist of uppercase letters, digits, and hyphens only.")]
        public string? ItemCode { get; set; }
        public string? ItemName2 { get; set; }
        //[RequiredIf(nameof(IsGoods), true, ErrorMessage = "Please provide SAC code for the item")]
        public int? HsnID { get; set; }
        //[RequiredIf(nameof(IsGoods), false, ErrorMessage = "Please provide SAC code for the item")]
        public int? SacID { get; set; }
        public bool IsGoods { get; set; } = false;

       // [Required(ErrorMessage = "Please choose a tax preference for the item")]
        public int? TaxPreferenceTypeID { get; set; } 
        //[RequiredIf(nameof(TaxPreferenceTypeID), (int)TaxPreferences.Taxable, ErrorMessage = "Please choose an Intra tax category")]
        public int? IntraTaxCategoryID { get; set; }
       // [RequiredIf(nameof(TaxPreferenceTypeID), (int)TaxPreferences.Taxable, ErrorMessage = "Please choose an Inter tax category")]
        public int? InterTaxCategoryID { get; set; }
        public int? GroupID { get; set; }
        public int? BrandID { get; set; }
       // [RequiredIf(nameof(IsGoods), true, ErrorMessage = "Please choose an item category")]
        public int? CategoryID { get; set; }
        public int? PurchaseAccountID { get; set; }
        public int? SaleAccountID { get; set; }
        public string? Description { get; set; }
        public string? PurchaseDescription { get; set; }
        public string? SaleDescription { get; set; }
        public int? ClientID { get; set; }
        public bool IsInclusiveTax { get; set; }
        public int? AddedBy { get; set; }
        public bool HasMultipleModels { get; set; } = false;
        public int? QrCodeMediaID { get; set; }
        public string? TaxPreferenceName { get; set; }
        public string? HsnCode { get; set; }
        public string? SacCode { get; set; }
        public string? IntraTaxCategoryName { get; set; }
        public string? InterTaxCategoryName { get; set; }
        public bool HaveSalesInfromation { get; set; } = false;
        public bool HavePurchaseInfromation { get; set; } = false;
        public string? SaleAccount { get; set; }
        public string? PurchaseAccount { get; set; }
        public string? AddedByUserName { get; set; }
        public string? QrCodeFileName { get; set; }
        public string? CategoryName { get; set; }
        public string? GroupName { get; set; }
        public string? BrandName { get; set; }
        public bool IsSubscription { get; set; }


        //Default Item variant
        public int ItemVariantID { get; set; }
        public string? ItemModelName { get; set; }
        //[RequiredIf(nameof(HasMultipleModels), false, ErrorMessage = "Please enter the unieque url code for the item")]
        //[RegularExpression("^[a-z0-9\\-]+$", ErrorMessage = "Please use only lowercase letters, numbers and hyphens.")]
        public string? UrlCode { get; set; }
        //[RequiredIf(nameof(HasMultipleModels), false, ErrorMessage = "Please choose the unit for the item")]
        public int? PackingTypeID { get; set; } = (int)PackingTypes.Piece;
        public string? PackingTypeName { get; set; } = PackingTypes.Piece.ToString();
        //[RequiredIf(nameof(HasMultipleModels), false, ErrorMessage = "Please provide um unit for the item variant")]
        //[Range(1, int.MaxValue, ErrorMessage = "Please enter number of unit for the item")]
        public int UMUnit { get; set; } = 1;
        [RequiredIf(nameof(HasMultipleModels), false, ErrorMessage = "Please enter the selling price for the item")]
        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }
        public int? SizeID { get; set; }
        public string? Size { get; set; }
        public int? ColorID { get; set; }
        public string? ColorName { get; set; }
        public List<ItemImageModel> ItemImages { get; set; } = new();
        public List<ItemVariantImageModel> DefaultItemVariantImages { get; set; } = new();

        public List<ItemVariantModel>? ItemVariants { get; set; }
    }
}
