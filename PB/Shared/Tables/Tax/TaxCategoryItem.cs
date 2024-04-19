using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PB.Model;

namespace PB.Shared.Tables.Tax
{
    public class TaxCategoryItem : Table
    {
        [PrimaryKey]
        public int TaxCategoryItemID { get; set; }
        public int? TaxCategoryID { get; set; }
        [Required(ErrorMessage = "Please provide Tax category item name")]
        public string? TaxCategoryItemName { get; set; }
        [Required(ErrorMessage = "Please provide Tax percentage")]
        public decimal Percentage { get; set; }
        public int? LedgerID { get; set; }
    }
}
