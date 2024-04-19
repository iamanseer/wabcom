using PB.Shared.Tables.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory
{
    public class TaxDetailsModel
    {
        public int TaxCategoryID { get; set; }
        public string? TaxCategoryName { get; set; }
        public decimal TaxPercentage { get; set; }
        public List<TaxCategoryItem>? TaxCategoryItems { get; set; }
    }
}
