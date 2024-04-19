using PB.Model;

namespace PB.Model.Models
{
    public class FollowupPagedListModel
    {
        public PagedList<FollowupListModel> FollowupList { get; set; } = new();
        public int UserTypeID { get; set; }
        
    }
}
