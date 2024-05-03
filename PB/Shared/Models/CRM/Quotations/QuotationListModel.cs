using PB.Shared.Models;
using PB.Shared.Enum.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Model.Models
{
    public class QuotationListModel
    {
        public int QuotationID { get; set; }
        public string? QuotationName { get; set; }
        public DateTime? AddedOn { get; set; }
        public DateTime? ExpireOn { get; set; } 
        public int QuotationNo { get; set; }
        public string? CustomerName { get; set; }
        public string? Username { get; set; }
        public short CurrentFollowupNature { get; set; }
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
                    case (int)FollowUpNatures.ClosedWon:
                        return "badge bg-danger";
                    case (int)FollowUpNatures.Interested:
                        return "badge bg-success";
                }
                return "";
            }
        }

    }
}
