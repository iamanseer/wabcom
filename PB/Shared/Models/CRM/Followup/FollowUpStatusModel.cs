
using PB.Shared.Tables.CRM;

namespace PB.Model.Models
{
    public class FollowupStatusModel : FollowupStatus
    {
        public int RowIndex { get; set; }
        public bool IsRowInEditMode { get; set; } = false;
    }
}
