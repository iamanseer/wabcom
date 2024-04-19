using PB.Shared.Enum.CRM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.CRM.Enquiry
{
    public class EnquiryModelNew
    {
        public int EnquiryID { get; set; }
        [Required(ErrorMessage = "Please choose the a date for the enquiry")]
        public DateTime? Date { get; set; } = DateTime.UtcNow;
        [Required(ErrorMessage = "Please choose a customer for the enquiry")]
        public int? CustomerEntityID { get; set; }
        public string? CustomerName { get; set; }
        public int EnquiryNo { get; set; }
        public int? LeadQuality { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "Please choose fisrt followup date")]
        public DateTime? FirstFollowUpDate { get; set; } = DateTime.UtcNow.AddDays(1);
        public int? LeadThroughID { get; set; }
        public int CurrentFollowupNature { get; set; } = (int)FollowUpNatures.New;
        public bool IsInCart { get; set; } = false;
        public List<EnquiryItemModelNew> Items { get; set; } = new();
    }

    public class EnquiryItemModelNew 
    {
        public int EnquiryItemID { get; set; }
        public int? EnquiryID { get; set; }
        [Required(ErrorMessage = "Please choose an item")]
        public int? ItemVariantID { get; set; }
        public string? ItemName { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "Please provide quantity for the item")]
        [Range(1, int.MaxValue, ErrorMessage = "Please add a minimum of 1 quantity")]
        public int Quantity { get; set; } = 1;
        public bool IsEditMode { get; set; } = false;
    }

    public class EnquiryAssigneeModelNew
    {
        public int EnquiryAssigneeID { get; set; }
        public int EnquiryID { get; set; }
        public int EntityID { get; set; }
        public string? UserName { get; set; }  
    }
}
