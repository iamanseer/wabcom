using PB.Model;
using PB.Shared.Enum.CRM;
using PB.Shared.Enum.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.Invoices
{
    public class Invoice : Table
    {
        [PrimaryKey] public int InvoiceID { get; set; }
        [Required(ErrorMessage = "Please choose an Invoice type")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a valid option")]
        public int? InvoiceTypeID { get; set; }
        public DateTime? InvoiceDate { get; set; } = DateTime.UtcNow;
        public DateTime? AccountsDate { get; set; } = DateTime.UtcNow;
        public int InvoiceNumber { get; set; } 
        public string? Prefix { get; set; }
        public int GSTRound { get; set; } = 2;
        [Required(ErrorMessage = "Please choose a sales account for this invoice")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a valid sales account for this invoice")]
        public int? CustomerEntityID { get; set; }
        public string? Remarks { get; set; }
        public int? BranchID { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrossAmount { get; set; } 
        public int? PaymentStatus { get; set; } = (int)InvoicePaymentStatus.None;
        public int? UserEntityID { get; set; }
        public string? Subject { get; set; }
        public string? CustomerNote { get; set; }
        public string? TermsandCondition { get; set; }
        [Required(ErrorMessage = "Please choose a billing address for the invoice")]
        public int? BillingAddressID { get; set; }
        public int? ShippingAddressID { get; set; }
        public int? MediaID { get; set; }
        [Required(ErrorMessage = "Please choose a currency for this invoice")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a valid currency account for this invoice")]
        public int? CurrencyID { get; set; }
        [Required(ErrorMessage = "Please choose a place of supply for this invoice")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a valid place of supply for this invoice")]
        public int? PlaceOfSupplyID { get; set; }
        public int? QuotationID { get; set; }
        public int? CurrentFollowupNature { get; set; } = (int)FollowUpNatures.New;
        public int? SourceOfSupplyID { get; set; }
        public int? SupplierEntityID { get; set; }
        public int? JournalMasterID { get; set; }
        public int? PaymentTermID { get; set; }
    }
}
