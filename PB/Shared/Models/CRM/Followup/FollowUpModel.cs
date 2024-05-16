
using PB.Shared.Tables.CRM;

namespace PB.Model.Models
{
    public class FollowUpModel : FollowUp
    {
        public string? FollowupName { get; set; }
        public string? Username { get; set; }
        public string? Status { get; set; }

    }

    public class FollowUpNotificationPostModel
    {
        public int? ID { get; set; }
        public int? FollowupType { get; set; }
        public string? Message { get; set; }
        public string? Tittle { get; set; }
        public int? NotificationType { get; set; }
    }
}
