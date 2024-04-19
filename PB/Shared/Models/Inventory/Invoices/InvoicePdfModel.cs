using PB.Model.Models;
using PB.Shared.Models.CRM.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Invoices
{
    public class InvoicePdfModel
    {
        public InvoicePdfClientDetails Client { get; set; } = new();
        public string? Prefix { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? PlaceOfSupplyName { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? Terms { get; set; } = "Due on Receipt";
        public DateTime? DueDate { get; set; }
        public int CustomerEntityID { get; set; }
        public InvoicePdfCustomerDetails Customer { get; set; } = new();
        public string? Subject { get; set; }
        public List<string> TaxCategoryHeadings { get; set; } = new();
        public List<InvoicePdfItemDetails> Items { get; set; } = new();
        public string? CurrencyName { get; set; }
        public string? Symbol { get; set; }
        public string? MainSuffix { get; set; }
        public string? SubSuffix { get; set; }
        public string? AmountInWords { get; set; }
        public string? TermsandCondition { get; set; }
        public string? CustomerNote { get; set; }
    }
    public class InvoicePdfCustomerDetails
    {
        public string? CustomerName { get; set; }
        public string? TaxNumber { get; set; }
        public int BillingAddressID { get; set; }
        public int? ShippingAddressID { get; set; }
        public AddressView BillingAddress { get; set; } = new();
        public AddressView ShippingAddress { get; set; } = new();
    }
    public class InvoicePdfClientDetails
    {
        public string? ClientName { get; set; }
        public int AddressID { get; set; }
        public string? ClientLogoFileName { get; set; }
        public AddressView ClientAddress { get; set; } = new();
        public string? TaxNumber { get; set; }
    }
    public class InvoicePdfItemDetails
    {
        public string? ItemName { get; set; }
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public int TaxCategoryID { get; set; }
        public decimal Rate { get; set; }
        public List<InvoicePdfItemTaxDetails> TaxItems { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal ChargeDiscountDivided { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrossAmount { get; set; }

    }
    public class InvoicePdfItemTaxDetails
    {
        public int? TaxCategoryID { get; set; } 
        public int TaxCategoryItemID { get; set; }
        public string? CategoryName { get; set; }
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }
}
