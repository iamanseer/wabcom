using PB.Shared.Models;
using PB.Model;
using PB.Shared.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.Items
{
    public class Item : Table
    {
        [PrimaryKey]
        public int ItemID { get; set; }
        [Required(ErrorMessage = "Please provide a name for the item")]
        public string? ItemName { get; set; }
        [Required(ErrorMessage = "Please provide a code for the item")]
        [RegularExpression("^[A-Za-z0-9-]+$", ErrorMessage = "The item code must consist of uppercase letters, digits, and hyphens only.")]
        public string? ItemCode { get; set; }
        public string? ItemName2 { get; set; }
        [RequiredIf(nameof(IsGoods), true, ErrorMessage = "Please provide SAC code for the item")]
        public int? HsnID { get; set; }
        [RequiredIf(nameof(IsGoods), false, ErrorMessage = "Please provide SAC code for the item")]
        public int? SacID { get; set; }
        public bool IsGoods { get; set; } = true;

        [Required(ErrorMessage = "Please choose a tax preference for the item")]
        public int? TaxPreferenceTypeID { get; set; } = (int)TaxPreferences.Taxable;
        [RequiredIf(nameof(TaxPreferenceTypeID), (int)TaxPreferences.Taxable, ErrorMessage = "Please choose an Intra tax category")]
        public int? IntraTaxCategoryID { get; set; }
        [RequiredIf(nameof(TaxPreferenceTypeID), (int)TaxPreferences.Taxable, ErrorMessage = "Please choose an Inter tax category")]
        public int? InterTaxCategoryID { get; set; }
        public int? GroupID { get; set; }
        public int? BrandID { get; set; }
        [RequiredIf(nameof(IsGoods),true,ErrorMessage = "Please choose an item category")]
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
    }
}
