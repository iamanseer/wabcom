using PB.Model;
using PB.Shared.Enum.CRM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CRM
{
    public class Quotation : Table
    {
        [PrimaryKey]
        public int QuotationID { get; set; }
        [Required(ErrorMessage = "Please select the date of qutation")]
        public DateTime? Date { get; set; } = DateTime.UtcNow;
        [Required(ErrorMessage = "Please choose a customer for this quotation")]
        public int? CustomerEntityID { get; set; }
        public int? ClientID { get; set; }
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
       // [Required(ErrorMessage = "Please choose a billing address for the quotation")]
        public int? BillingAddressID { get; set; }
        public int? ShippingAddressID { get; set; }
        public int? CurrentFollowupNature { get; set; } = (int)FollowUpNatures.New;
        public int? MediaID { get; set; } 
        public int? BranchID { get; set; }
       // [Required(ErrorMessage = "Please choose a currency for this quotation")]
        public int? CurrencyID { get; set; }
        //[Required(ErrorMessage = "Please choose a currency for this quotation")]
        public int? PlaceOfSupplyID { get; set; }
    }
}
