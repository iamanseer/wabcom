
using PB.Shared.Tables.CRM;

namespace PB.Model.Models
{
    public class QuotationFollowupModel : Quotation
    {
        public string? QuotationName { get; set; }
        public string? CustomerName { get; set; }
        public string? Username { get; set; }
        public int ItemsCount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<FollowUpModel> Histories { get; set; } = new();
        public List<string> FollowupAssignees { get; set; } = new();
        public List<QuotationItemModel> QuotationItems { get; set; } = new();
        public List<QuotationMailRecipientModel> MailReciepients { get; set; } = new();
        public string? MobileNumber { get; set; }
        public int? ContactID { get; set; }
        public string? EmailAddress { get; set; }
        public string? FileName { get; set; } 
        public string? TaxNumber { get; set; }
        public int? InvoiceID { get; set; } 
    }
}
