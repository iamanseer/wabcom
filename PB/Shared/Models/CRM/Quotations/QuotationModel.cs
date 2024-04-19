
using PB.Shared.Tables.CRM;

namespace PB.Model.Models
{
    public class QuotationModel : Quotation
    {
        public string? CustomerName { get; set; }
        public string? MobileNumber { get; set; }
        public int? ContactID { get; set; }
        public string? EmailAddress { get; set; }
        public string? FileName { get; set; } 
        public bool NeedShippingAddress { get; set; }
        public string? CurrencyName { get; set; }
        public string? PlaceOfSupplyName { get; set; }
        public int CountryID { get; set; }  
        public List<QuotationItemModel> QuotationItems { get; set; } = new();
        public List<QuotationAssigneeModel> FollowupAssignees { get; set; } = new();
        public List<QuotationMailRecipientModel> MailReciepients { get; set; } = new();
        public string? TaxNumber { get; set; }
    }

    public class QuotationItemModel : QuotationItem
    {
        public bool IsRowInEditMode { get; set; } = false;
        public string? ItemName { get; set; }
        public int RowIndex { get; set; }
        public string? TaxCategoryName { get; set; }
        public decimal TotalAmount { get; set; }
        public List<QuotationItemTaxCategoryItemsModel> TaxCategoryItems { get; set; } = new();
    } 
    public class QuotationAssigneeModel : QuotationAssignee
    {
        public bool IsAssigned { get; set; } = false;
        public string? Name { get; set; }
    }

    public class QuotationMailRecipientModel : QuotationMailRecipient
    {
        public string? Name { get; set; }
        public bool IsAssigned { get; set; }
    }


    public class QuotationItemTaxCategoryItemsModel 
    {
        public int TaxCategoryItemID { get; set; } 
        public string? TaxCategoryItemName { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
    }

}
