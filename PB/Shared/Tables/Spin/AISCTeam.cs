using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Spin
{
    public class AISCTeam:Table
    {
        [PrimaryKey]
        public int TeamID { get; set; }
        public string? TeamName { get; set; }
        public int? ManagerMembershipID { get; set; }
        public int? MediaID { get; set; }
        public string? SponsoredBy { get; set; }
    }
}
