using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Court
{
    public class CourtPriceGroup:Table
    {
        [PrimaryKey]
        public int PriceGroupID { get; set; }
        [Required(ErrorMessage ="Please enter price group name")]
        [StringLength(100,ErrorMessage ="Price group name should be at most 100 characters")]
        public string? PriceGroupName { get; set; }
        public int? ClientID { get; set; }
        public int? TaxCategoryID { get; set; }
        public bool IncTax { get; set; }
        public int? CurrencyID { get; set; }
    }
}
