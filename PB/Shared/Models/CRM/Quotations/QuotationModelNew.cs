using PB.Model.Models;
using PB.Shared.Enum.CRM;
using PB.Shared.Tables.CRM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.CRM
{
    public class QuotationModelNew
    {
        public int QuotationID { get; set; }
        [Required(ErrorMessage = "Please select the date of qutation")]
        public DateTime? Date { get; set; } = DateTime.UtcNow;
        [Required(ErrorMessage = "Please choose a customer for this quotation")]
        public int? CustomerEntityID { get; set; }
        public int? ClientID { get; set; }

        //[Required(ErrorMessage = "Please enter quotation number")]
        //[Range(1, int.MaxValue, ErrorMessage = "Please enter quotation number")]
        public int QuotationNo { get; set; }
        public int? UserEntityID { get; set; }
        public int? EnquiryID { get; set; }
        [Required(ErrorMessage = "Please choose first folloup date")]
        public DateTime? FirstFollowUpDate { get; set; } = DateTime.Now.AddDays(1);
        [Required(ErrorMessage = "Please choose expiry date")]
        public DateTime? ExpiryDate { get; set; } = DateTime.Now.AddDays(8);
        public string? Subject { get; set; }
        public string? CustomerNote { get; set; }
        public string? TermsandCondition { get; set; }
        [Required(ErrorMessage = "Please choose a billing address for the quotation")]
        public int? BillingAddressID { get; set; }
        public int? ShippingAddressID { get; set; }
        public int? CurrentFollowupNature { get; set; } = (int)FollowUpNatures.New;
        public int? MediaID { get; set; }
        public int? BranchID { get; set; }
        //[Required(ErrorMessage = "Please choose a currency for this quotation")]
        public int? CurrencyID { get; set; }
       // [Required(ErrorMessage = "Please choose a currency for this quotation")]
        public int? PlaceOfSupplyID { get; set; }
        public string? CustomerName { get; set; }
        public string? TaxNumber { get; set; } 
        public string? MobileNumber { get; set; }
        public int? ContactID { get; set; }
        public string? EmailAddress { get; set; }
        public string? FileName { get; set; }
        public bool NeedShippingAddress { get; set; }
        public string? CurrencyName { get; set; }
        public string? PlaceOfSupplyName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrossAmount { get; set; }
        public bool GenerateQuotationPdf { get; set; } = false;
        [Required (ErrorMessage ="Please choose business type name")]
        public int? BusinessTypeID { get; set; }
        public string? BusinessTypeName { get; set; }
        [Required(ErrorMessage = "Please enter description")]
        public string? Description { get; set; }
        [Required(ErrorMessage = "Please choose created for")]
        public int? QuotationCreatedFor { get; set; }
        public string? StaffName { get; set; }
        public string? StaffPhoneNo { get; set; }
        public List<QuotationItemModelNew> QuotationItems { get; set; } = new();
        public List<QuotationAssignee>? QuotationAssignees { get; set; }
        public List<QuotationMailRecipient>? MailReciepients { get; set; }
    }

    public class QuotationItemModelNew : QuotationItem 
    {
        public bool IsRowInEditMode { get; set; } = false;
        public string? ItemName { get; set; }
        public string? TaxCategoryName { get; set; }
        public int? TaxPreferenceTypeID { get; set; }
        public string? TaxPreferenceName { get; set; }  
        public decimal TotalAmount { get; set; }
        public bool IsGoods { get; set; }
        public decimal CurrentStock { get; set; }  
        public List<TaxCategoryItemModel> TaxCategoryItems { get; set; } = new();
    }
}
