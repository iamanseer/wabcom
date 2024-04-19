using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CRM
{
    public class QuotationItem : Table
    {
        [PrimaryKey]
        public int QuotationItemID { get; set; }
        public int? QuotationID { get; set; }
        [Required(ErrorMessage = "Please choose a quotation item")]
        public int? ItemVariantID { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "Plaese provide the quantity for the item")]
        [Range(1, int.MaxValue, ErrorMessage = "Please add a minimum of 1 quantity")]
        public decimal Quantity { get; set; } = 1;
        [Required(ErrorMessage = "Plaese provide the rate for the item")]
        public decimal Rate { get; set; }
        public decimal Discount { get; set; } = 0;
        public decimal NetAmount { get; set; }
        public int? TaxCategoryID { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrossAmount { get; set; } 
    }
}
