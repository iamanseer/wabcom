using PB.Shared.Tables.Spin;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Spin
{
    public class AISCMembershipModel
    {
        public int ID { get; set; }
        [Required]
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [Required]
        public string? PhoneNo { get; set; }
        [Required]
        public string? WhatsappNo { get; set; }
        [Required]
        public string? EmailAddress { get; set; }
        public string? Nationality { get; set; }
        public string? Region { get; set; }
        public string? VisaStatus { get; set; }
        public string? Age { get; set; }
        public string? LevelOfGame { get; set; }
        public string? Gender { get; set; }
        public int? MediaID { get; set; }
        public string? Message { get; set; }
        public string? FileName { get; set; }
        public string? Prefix { get; set; } = "AISC";
        public int? Number { get; set; }
        public int? TeamID { get; set; }
        public string? TeamName { get; set; }
        public string? PhoneISDCode { get; set; }
        public string? WhatsappISDCode { get; set; }
        public string? BloodGroup { get; set; }

    }



    public class AISCMembersListViewModel
    {
        public string? Name { get; set; }
        public string? MembershipNo { get; set; }
        public string? FileName { get; set; }
        public string? Teamleader { get; set; }
    }

    public class AISCTeamViewListModel : AISCTeam
    {
        public string? LogoName { get; set; }
        public List<AISCMembersListViewModel> Members { get; set; } = new();
    }
}
