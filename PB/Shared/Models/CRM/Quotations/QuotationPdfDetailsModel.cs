using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Model.Models
{
    public class QuotationPdfDetailsModel
    {
        public int QuotationID { get; set; }
        public int QuotationNo { get; set; }
        public DateTime? Date { get; set; }
        public int CustomerEntityID { get; set; }
        public string? CustomerName { get; set; }
        public string? Phone { get; set; }
        public string? EmailAddress { get; set; } 
        public string? CustomerNote { get; set; }
        public string? TermsandCondition { get; set; }
        public int? BillingAddressID { get; set; }
        public int? ShippingAddressID { get; set; }
        public string? FileName { get; set; }  
        public AddressModel BillingAddress { get; set; } = new();
        public AddressModel ShippingAddress { get; set; } = new();
        public List<QuotationItemPDFModel> Items { get; set; } = new();
        public DateTime? ExpiryDate { get; set; }
        public string? BillingAddressLine1 { get; set; }
        public string? BillingAddressLine2 { get; set; }
        public string? BillingAddressLine3 { get; set; }
        public string? BillingPinCode { get; set; }
        public string? BillingState { get; set; }
        public string? BillingCountry { get; set; }
        public string? ShippingAddressLine1 { get; set; }
        public string? ShippingAddressLine2 { get; set; }
        public string? ShippingAddressLine3 { get; set; }
        public string? ShippingPinCode { get; set; }
        public string? ShippingState { get; set; }
        public string? ShippingCountry { get; set; }
        public string? Subject { get; set; }

        public string? ClientAddressLine1 { get; set; }
        public string? ClientAddressLine2 { get; set; }
        public string? ClientAddressLine3 { get; set; }
        public string? ClientPinCode { get; set; }
        public string? ClientState { get; set; }
        public string? ClientCountry { get; set; }
        public string? ClientImage { get; set; }
        public int? ClientMediaID { get; set; }
        public string? ClientName { get; set; }
        public string? AmountInWords { get; set; }
        public string? GSTNo { get; set; }
        public string? DomainUrl { get; set; }
        public string? CurrencyName { get; set; }
        public string? CurrencySymbol { get; set; }
        public string? MainSuffix { get; set; }
        public string? SubSuffix { get; set; } 
        public string? TaxNumber { get; set; }

    }

    public class QuotationItemPDFModel
    {
        public int? RowIndex { get; set; }
        public string? ItemName { get; set; }
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
        public int TaxCategoryID { get; set; }
        public List<QuotationItemTaxCategoryItemsModel> TaxCategoryItems { get; set; } = new();
        public decimal? Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal TaxAmount { get; set; } 
    }
} 
