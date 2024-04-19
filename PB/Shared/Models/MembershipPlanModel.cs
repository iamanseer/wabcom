using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class MembershipPlanModel
    {
        public int PlanID { get; set; }
        [Required]
        public string? PlanName { get; set; }
        [Required]
        public int MonthCount { get; set; }
        public bool IsSelected { get; set; }
    }
}
