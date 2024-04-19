using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Model.Models
{
    public class FollowupStatusListModel
    {
        public int FollowUpStatusID { get; set; }
        public string? StatusName { get; set; }
        public int RowIndex { get; set; }
        public bool IsRowInEditMode { get; set; } = false;
    }



    public class FollowupStatusSearchModel
    {
        public int? Nature { get; set; }
        public int? Type { get; set; }
    }
}
