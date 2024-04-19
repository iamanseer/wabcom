using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Spin
{
    public class AISCTeamModel
    {
        public int TeamID { get; set; }
        [Required]
        public string? TeamName { get; set; }
        [Required]
        public int? ManagerMembershipID { get; set; }
        public string? FirstName { get; set; }
        public int? MediaID { get; set; }
        public string? SponsoredBy { get; set; }
    }
}
