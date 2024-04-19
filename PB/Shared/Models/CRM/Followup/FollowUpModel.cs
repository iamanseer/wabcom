
using PB.Shared.Tables.CRM;

namespace PB.Model.Models
{
    public class FollowUpModel : FollowUp
    {
        public string? FollowupName { get; set; }
        public string? Username { get; set; }
        public string? Status { get; set; }

    }
}
