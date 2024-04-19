using PB.Model;
using PB.Shared.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.eCommerce.Products
{

    public class EC_Item : Table
    {
        [PrimaryKey]
        public int ItemID { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName2 { get; set; }
        public int? HsnID { get; set; }
        public int? SacID { get; set; }
        public bool IsGoods { get; set; } = true;
        public int? TaxPreferenceTypeID { get; set; } = (int)TaxPreferences.Taxable;
        public int? IntraTaxCategoryID { get; set; }
        public int? InterTaxCategoryID { get; set; }
        public int? GroupID { get; set; }
        public int? BrandID { get; set; }
        public int? CategoryID { get; set; }
        public string? Description { get; set; }
        public int? ClientID { get; set; }
        public bool IsInclusiveTax { get; set; }
        public int? AddedBy { get; set; }
        public bool HasMultipleModels { get; set; } = false;
        public bool IsFeatured { get; set; }
        public bool IsNewArrival { get; set; }
    }
}
