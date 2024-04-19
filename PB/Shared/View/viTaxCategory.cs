using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Views
{
    public class viTaxCategory : View
    {
        [PrimaryKey]
        public int TaxCategoryID { get; set; }
        public string? TaxCategoryName { get; set; }
        public int? ClientID { get; set; }
        public decimal TaxPercentage { get; set; } 
    }
}
