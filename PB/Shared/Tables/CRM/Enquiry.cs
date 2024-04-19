using PB.Model;
using PB.Shared.Enum.CRM;
using System.ComponentModel.DataAnnotations;

namespace PB.Shared.Tables.CRM
{
    public class Enquiry : Table
    {
        [PrimaryKey]
        public int EnquiryID { get; set; }
        [Required(ErrorMessage = "Please choose the a date for the enquiry")]
        public DateTime? Date { get; set; } = DateTime.UtcNow;
        [Required(ErrorMessage = "Please choose a customer for the enquiry")]
        public int? CustomerEntityID { get; set; }
        //[Required(ErrorMessage = "Please provide an enquiry number above zero")]
        public int EnquiryNo { get; set; }
        public int? UserEntityID { get; set; }
        public int? ClientID { get; set; }
        //[Required(ErrorMessage = "Please choose a lead quality for the enquiry")]
        public int? LeadQuality { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "Please choose fisrt followup date")]
        public DateTime? FirstFollowUpDate { get; set; } = DateTime.UtcNow.AddDays(1);
        public int? LeadThroughID { get; set; }
        public int CurrentFollowupNature { get; set; } = (int)FollowUpNatures.New;
        public int? BranchID { get; set; }
        public bool IsInCart { get; set; } = false; 
    }
}
