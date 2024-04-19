using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Spin
{
    public class AISCMembershipViewModel
    {
        public int ID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNo { get; set; }
        public string? WhatsappNo { get; set; }
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
        public string? Prefix { get; set; }
        public int Number { get; set; }
        public string? TeamName { get; set; }
        public string? BloodGroup { get; set; }
    }

    public class MembershipNumberPostModel
    {
        public int? Number { get; set; }
        public int ID { get; set; }
    }
}
