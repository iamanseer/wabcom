using PB.Shared.Models;
using PB.Shared.Enum.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Model.Models
{
    public class FollowupListModel
    {
        public int FollowUpID { get; set; }
        public int? EnquiryID { get; set; }
        public int? QuotationID { get; set; }
        public int Days { get; set; } 
        public short Type { get; set; }
        public string? FollowupBy { get; set; }
        public string? CustomerName { get; set; }
        public DateTime? FollowupDate { get; set; }
        public DateTime? NextFollowupDate { get; set; }
        public short CurrentFollowupNature { get; set; }
        public string? AssigneeName { get; set; }
        public string? NatureBadgeClass
        {
            get
            {
                switch (CurrentFollowupNature)
                {
                    case (int)FollowUpNatures.New:
                        return "badge bg-success";
                    case (int)FollowUpNatures.Followup:
                        return "badge bg-info";
                    case (int)FollowUpNatures.Dropped:
                        return "badge bg-danger";
                    case (int)FollowUpNatures.ClosedWon:
                        return "badge bg-warning";
                }
                return "";
            }
        }
        public string? RowBgClass
        {
            get
            {
                if(CurrentFollowupNature == (int)FollowUpNatures.New || CurrentFollowupNature == (int)FollowUpNatures.Followup)
                {
                    switch (Days)
                    {
                        case < 0:
                            return "bg-danger-transparent text-danger";
                        case 0:
                            return "bg-success-transparent text-success";
                        case >0:
                            return "bg-info-transparent text-info";
                    }
                }
                return "";
            }
        }
        public int Number { get; set; }
    }
}
 