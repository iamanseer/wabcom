using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PB.Model;
using PB.Shared.Models;
using PB.Model.Models;
using PB.Shared.Tables.Tax;

namespace PB.Shared.Models
{
    public class TaxCategoryModel : TaxCategory
    {
        public List<TaxCategoryItemModel> TaxCategoryItems { get; set; } = new();
    }

    public class TaxCategoryItemModel : TaxCategoryItem
    {
        public decimal TaxAmount { get; set; } 
        public string? LedgerName { get; set; }
    }

    public class TaxCategoryListModel
    {
        public int TaxCategoryID { get; set; }
        public string? TaxCategoryName { get; set; }
    }
}
