using PB.Model;
using PB.Shared.Enum;
using PB.Shared.Enum.CRM;
using PB.Shared.Enum.Inventory;
using PB.Shared.Helpers;
using PB.Shared.Tables.Inventory.Invoices;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Invoices
{
    public class InvoiceModel
    {
        public int InvoiceID { get; set; }
        [Required(ErrorMessage = "Please choose an Invoice type")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a valid option")]
        public int? InvoiceTypeID { get; set; }
        public string? InvoiceTypeName { get; set; }
        public DateTime? InvoiceDate { get; set; } = DateTime.UtcNow;
        public DateTime? AccountsDate { get; set; } = DateTime.UtcNow;
        public int InvoiceNumber { get; set; }
        public string? Prefix { get; set; }
        public int GSTRound { get; set; } = 2;
        [PB.Shared.Helpers.RequiredIfMultiple(nameof(InvoiceTypeNatureID), new int[] { (int)InvoiceTypeNatures.Sales, (int)InvoiceTypeNatures.Sales_Return }, ErrorMessage = "Please choose a customer for the invoice")]
        public int? CustomerEntityID { get; set; }
        public string? CustomerName { get; set; }
        public string? Remarks { get; set; }
        public int? BranchID { get; set; }
        public string? BranchName { get; set; }
        public int? JournalMasterID { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrossAmount { get; set; }
        public int? CashLedgerID { get; set; }
        public string? CashLedgerName { get; set; }
        public decimal? CashAmount { get; set; }
        public int? BankLedgerID { get; set; }
        public string? BankLedgerName { get; set; }
        public decimal? BankAmount { get; set; }
        public decimal? AdvanceAmount { get; set; }
        public int? PaymentStatus { get; set; } = (int)InvoicePaymentStatus.None;
        public int? UserEntityID { get; set; }
        public string? Subject { get; set; }
        public string? CustomerNote { get; set; }
        public string? TermsandCondition { get; set; }
        [Required(ErrorMessage = "Please choose a billing address for the invoice")]
        public int? BillingAddressID { get; set; }
        public int? ShippingAddressID { get; set; }
        public int? MediaID { get; set; }
        public string? FileName { get; set; } 
        [Required(ErrorMessage = "Please choose a currency for this invoice")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a valid currency account for this invoice")]
        public int? CurrencyID { get; set; }
        public string? CurrencyName { get; set; }
        [PB.Shared.Helpers.RequiredIfMultiple(nameof(InvoiceTypeNatureID), new int[] { (int)InvoiceTypeNatures.Sales, (int)InvoiceTypeNatures.Sales_Return }, ErrorMessage = "Please choose a place of supply for this invoice")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a valid place of supply for this invoice")]
        public int? PlaceOfSupplyID { get; set; }
        public string? PlaceOfSupplyName { get; set; }
        public int? QuotationID { get; set; }
        public int? CurrentFollowupNature { get; set; } = (int)FollowUpNatures.New;
        [PB.Shared.Helpers.RequiredIfMultiple(nameof(InvoiceTypeNatureID), new int[] { (int)InvoiceTypeNatures.Purchase, (int)InvoiceTypeNatures.Purchase_Return }, ErrorMessage = "Please choose a source of supply for this invoice")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a valid source of supply for this invoice")]
        public int? SourceOfSupplyID { get; set; }
        public string? SourceOfSupplyName { get; set; }
        [PB.Shared.Helpers.RequiredIfMultiple(nameof(InvoiceTypeNatureID),new int[] { (int)InvoiceTypeNatures.Purchase, (int)InvoiceTypeNatures.Purchase_Return }, ErrorMessage = "Please choose a supplier for the invoice")]
        public int? SupplierEntityID { get; set; }
        public string? SupplierName { get; set; } 
        public string? TaxNumber { get; set; } 
        public bool NeedShippingAddress { get; set; } = false;
        public decimal TotalDiscount { get; set; }
        public bool GenerateInvoicePdf { get; set; } = false;
        public int? InvoiceTypeNatureID { get; set; }
        public decimal TotalAdvance { get; set; }
        public int? PaymentTermID { get; set; }
        public string? PaymentTermName { get; set; }
        public decimal CreditAmount { get; set; }
        public bool MoveSalesOrPurchaseReturnInvoiceAmountToAdvance { get; set; } = false;
        public int? ClientID { get; set; }
        public List<InvoiceItemModel> Items { get; set; } = new();  
        public List<InvoiceMailReceipient> InvoiceMailReceipients { get; set; } = new();  
        public List<InvoiceTaxItem> InvoiceTaxItems { get; set; } = new();
        public List<InvoiceChargeModel> InvoiceExtraChargeItems { get; set; } = new();
        public List<InvoiceAssignee> InvoiceAssignees { get; set; } = new();
        public List<InvoicePaymentsModel> InvoicePaymentsList { get; set; } = new();
        public List<BillToBillAgainstReferenceModel> InvoiceBillToBillAgainstReferences { get; set; } = new();
        public List<InvoicePaymentTermSlabModel> PaymentSlabs { get; set; } = new();
    }

    public class InvoiceItemModel
    {
        public int InvoiceItemID { get; set; }
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
        [RequiredIf(nameof(TaxPreferenceTypeID),(int)TaxPreferences.Taxable, ErrorMessage = "Please choose a tax category for the item")]
        public int? TaxCategoryID { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal Discount { get; set; }
        public decimal NetAmount { get; set; }
        public int UmUnit { get; set; }
        public decimal ChargeDiscountDivided { get; set; } = 0;
        public decimal TaxAmount { get; set; }
        public decimal GrossAmount { get; set; }
        public bool IsRowInEditMode { get; set; } = false;
        public string? ItemName { get; set; }
        public string? TaxCategoryName { get; set; }
        public int? TaxPreferenceTypeID { get; set; }
        public string? TaxPreferenceName { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsGoods { get; set; }
        public decimal? CurrentStock { get; set; }
        public List<TaxCategoryItemModel> TaxCategoryItems { get; set; } = new();
    }

    public class InvoiceChargeModel : InvoiceCharge 
    {
        public string? ChargeName { get; set; }
        public int OrderNumber { get; set; }
        public int ChargeNature { get; set; }
        public int ChargeOperation { get; set; }
        public int ChargeCalculation { get; set; }
        public int ChargeEffect { get; set; }
        public int? LedgerID { get; set; }
        public string? LedgerName { get; set; }
    }

    public class InvoicePaymentTermSlabModel
    {
        public int ID { get; set; }
        public int? SlabID { get; set; }
        public string? SlabName { get; set; }
        public int Days { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
        public int? PaymentTermID { get; set; }
        public DateTime? Date { get; set; }
    }

    public class BillToBillAgainstReferenceModel
    {
        public int? BillID { get; set; }
        public string? ReferenceNo { get; set; }
        public DateTime? Date { get; set; }
        public decimal? BillAmount { get; set; } 
        public decimal? Amount { get; set; }
    }

    public class InvoicePaymentsModel
    {
        public int JournalEntryID { get; set; }
        public int? LedgerID { get; set; }
        public decimal? Amount { get; set; }
        public string? LedgerName { get; set; }
        public int? AccountGroupTypeID { get; set; }
        public bool IsRowInEditMode { get; set; } = false;
    }

    public class PaymentTermSlabsListModel
    {
        public int InvoiceSlabID { get; set; }
        public int? SlabID { get; set; }
        public string? SlabName { get; set; }
        public int? PaymentTermID { get; set; }
        public int Day { get; set; }
        public decimal Percentage { get; set; }
        public DateTime? Date { get; set; }
        public decimal Amount { get; set; }
    }
}
