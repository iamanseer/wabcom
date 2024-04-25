using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CRM
{
    public class FollowUp : Table
    {
        [PrimaryKey]
        public int FollowUpID { get; set; }
        public int? EnquiryID { get; set; }
        [Required(ErrorMessage = "Please choose the date of followup")]
        public DateTime? FollowUpDate { get; set; } = DateTime.Today;
        [Required(ErrorMessage = "Please a choose a followup status")]
        public int? FollowUpStatusID { get; set; }
        public int? EntityID { get; set; }
        [Required(ErrorMessage = "Please choose the next date for the followup")]
        public DateTime? NextFollowUpDate { get; set; }
        public int FollowUpType { get; set; }
        public int? QuotationID { get; set; }
        public int? InvoiceID { get; set; } 
        public string? ShortDescription { get; set; }
    }
}
