
using PB.Shared.Enum.CRM;

namespace PB.Model.Models
{
    public class EnquiryListModel
    {
        public int EnquiryID { get; set; }
        public int EnquiryNo { get; set; }
        public DateTime? Date { get; set; } = null;
        public string? CustomerName { get; set; }
        public string? AddedBy { get; set; }
        public int? LeadQuality { get; set; }
        public int? LeadThroughID { get; set; }
        public string? LeadThroughName { get; set; } 
        public short CurrentFollowupNature { get; set; }
        public string? Assignee { get; set; } 
        public string? NatureBadgeClass
        {
            get
            {
                switch (CurrentFollowupNature)
                {
                    case (int)FollowUpNatures.New:
                        return "badge bg-dark";
                    case (int)FollowUpNatures.Followup:
                        return "badge bg-info";
                    case (int)FollowUpNatures.Dropped:
                        return "badge bg-danger";
                    case (int)FollowUpNatures.Interested:
                        return "badge bg-success"; 
                }
                return "";
            }
        }

    }
}
