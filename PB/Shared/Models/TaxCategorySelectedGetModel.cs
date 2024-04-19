using PB.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class TaxCategorySelectedGetModel
    {
        public int TaxCategoryID { get; set; }
        public string? TaxCategoryName { get; set; }
        public decimal TaxPercentage { get; set; }
        public List<QuotationItemTaxCategoryItemsModel> TaxCategoryItems { get; set; } = new();
    }
    public class TaxCategorySelectedGetModelNew
    {
        public int TaxCategoryID { get; set; }
        public string? TaxCategoryName { get; set; }
        public decimal TaxPercentage { get; set; }
        public List<TaxCategoryItemModel> TaxCategoryItems { get; set; } = new(); 
    }
}
