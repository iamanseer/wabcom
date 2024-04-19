using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.CRM.Followup
{
    public class FollowupStatusListViewModel
    {
        public int FollowUpStatusID { get; set; }
        public string? StatusName { get; set; }
        public int Nature { get; set; }
        public int ClientID { get; set; } 
    }
}
