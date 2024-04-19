using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.eCommerce.Products
{
    public class EC_ItemVariant : Table
    {
        [PrimaryKey]
        public int ItemVaraintID { get; set; }
        public int? ItemID { get; set; }
        public int? PackingTypeID { get; set; }
        public int? SizeID { get; set; }
        public int? UmUnit { get; set; }
        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }
        public string? SlugUrl { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? OgTitle { get; set; }
        public string? OgDescription { get; set; }
        public int? OgMediaID { get; set; } 
    }
}
