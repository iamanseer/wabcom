using PB.Model;
using PB.Shared.Enum.CRM;
using PB.Shared.Enum.Inventory;
using PB.Shared.Tables.Inventory.Invoices;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Invoices
{
    public class InvoiceViewModel
    {
        public int InvoiceID { get; set; } 
        public int? InvoiceTypeID { get; set; }
        public DateTime? InvoiceDate { get; set; } = DateTime.UtcNow;
        public DateTime? AccountsDate { get; set; } = DateTime.UtcNow;
        public int InvoiceNumber { get; set; }
        public string? Prefix { get; set; }
        public int GSTRound { get; set; } = 2;
        public int? CustomerEntityID { get; set; }
        public string? Remarks { get; set; }
        public int? BranchID { get; set; }
        public int? InvoiceJournalMasterID { get; set; }
        public int? ReceiptJournalMasterID { get; set; }
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
        public int? CurrencyID { get; set; }
        public int? PlaceOfSupplyID { get; set; }
        public int? QuotationID { get; set; }
        public int? CurrentFollowupNature { get; set; } = (int)FollowUpNatures.New;
        public int? SourceOfSupplyID { get; set; }
        public int? SupplierEntityID { get; set; }
        public string? InvoiceTypeName { get; set; }
        public string? SupplierName { get; set; }
        public string? TaxNumber { get; set; }
        public string? CustomerName { get; set; }
        public string? BranchName { get; set; }
        public string? CurrencyName { get; set; }
        public string? PlaceOfSupplyName { get; set; }
        public decimal TotalDiscount { get; set; }
        public string? MobileNumber { get; set; }
        public int? ContactID { get; set; }
        public string? EmailAddress { get; set; }
        public string? FileName { get; set; }
        public string? Username { get; set; }
        public int InvoiceTypeNatureID { get; set; } 
        public List<InvoiceViewItemModel> Items { get; set; } = new();

    }

    public class InvoiceViewItemModel
    {
        public int ItemVariantID { get; set; }  
        public string? ItemName { get; set; }
        public int Quantity { get; set; }
        public string? Description { get; set; }
        public decimal Rate { get; set; }
        public decimal Discount { get; set; } 
        public decimal GrossAmount { get; set; }
    }
}
