using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.Invoices
{
    public class InvoiceItem : Table
    {
        [PrimaryKey] public int InvoiceItemID { get; set; }
        public int? InvoiceID { get; set; }
        [Required(ErrorMessage = "Please add choose item")]
        public int? ItemVariantID { get; set; }
        public int? StockInvoiceItemID { get; set; }
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please enter quantity for the item")]
        [Range(1, int.MaxValue, ErrorMessage = "Please add a minimum of 1 quantity")]
        public decimal? Quantity { get; set; } = 1;
        [Required(ErrorMessage = "Please enter rate for the item")]
        public decimal Rate { get; set; }
        public decimal AverageRate { get; set; }
        public int? TaxCategoryID { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal Discount { get; set; }
        public decimal NetAmount { get; set; }
        public int UmUnit { get; set; }
        public decimal ChargeDiscountDivided { get; set; } = 0;
        public decimal TaxAmount { get; set; }
        public decimal GrossAmount { get; set; }
    }
}
