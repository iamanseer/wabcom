using PB.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.CRM.Quotations
{
    public class QuotationPdfCoverPageViewModel
    {
        public int QuotationID { get; set; }
        public int CustomerEntityID { get; set; }
        public string? CustomerName { get; set; }
        public string? ContactName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmailAddress { get; set; }
        public int? BillingAddressID { get; set; }
        public string? BillingAddressLine1 { get; set; }
        public string? BillingAddressLine2 { get; set; }
        public string? BillingAddressLine3 { get; set; }
        public string? BillingPinCode { get; set; }
        public string? BillingState { get; set; }
        public string? BillingCountry { get; set; }
        public string? ClientAddressLine1 { get; set; }
        public string? ClientAddressLine2 { get; set; }
        public string? ClientAddressLine3 { get; set; }
        public string? ClientPinCode { get; set; }
        public string? ClientState { get; set; }
        public string? ClientCountry { get; set; }
        public int? ClientMediaID { get; set; }
        public string? ClientName { get; set; }
        public string? ClientEmail { get; set; }
        public int? QuotationCreatedFor { get; set; }
        public string? StaffPhoneNo { get; set; }
        public string? StaffName { get; set; }
        public string? ProposalFor { get; set; }
        public string? DomainUrl { get; set; }
        public string? ClientPhone { get; set; }
    }

    public class QuotationPdfItemPageViewModel
    {
        public int QuotationID { get; set; }
        public int QuotationNo { get; set; }
        public DateTime? Date { get; set; }
        public List<QuotationItemPDFModel> Items { get; set; } = new();
        public DateTime? ExpiryDate { get; set; }
        public string? CustomerName { get; set; }
        public string? AmountInWords { get; set; }
        public string? GSTNo { get; set; }
        public string? DomainUrl { get; set; }
        public string? CurrencyName { get; set; }
        public string? CurrencySymbol { get; set; }
        public string? MainSuffix { get; set; }
        public string? SubSuffix { get; set; }
        public string? TaxNumber { get; set; }
        public int? BusinessTypeID { get; set; }
        public string? Description { get; set; }
        public string? BusinessTypeName { get; set; }
        public string? Prefix { get; set; }
        public int? Suffix { get; set; }
        public string? CustomerNote { get; set; }
        public string? TermsandCondition { get; set; }
        public List<string> CustomerNoteList { get; set; } = new();
    }

    public class QuotationPdfTermsPageViewModel
    {
        public int? QuotationCreatedFor { get; set; }
        public string? StaffPhoneNo { get; set; }
        public string? StaffName { get; set; }
        public string? DomainUrl { get; set; }
        public string? TermsandCondition { get; set; }
        public List<string> TermsList { get; set; } = new();
    }

}
