using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class BranchModel
    {
        public int? EntityID { get; set; }
        public int? EntityTypeID { get; set; }
        public int BranchID { get; set; }
        public int? AddressID { get; set; }
        [Required(ErrorMessage = "Please provide banch name")]
        public string? BranchName { get; set; }
        [Required(ErrorMessage = "Please provide branch phone number")]
        public string? Phone { get; set; }
        public string? Phone2 { get; set; }
        [Required(ErrorMessage = "Please provide branch e-mail")]
        [EmailAddress(ErrorMessage = "Please provide a valid e-mail like test@test.com")]
        public string? EmailAddress { get; set; }
        [Required(ErrorMessage = "Please provide address line 1")]
        public string? AddressLine1 { get; set; }
        [Required(ErrorMessage = "Please provide address line 2")]
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? Pincode { get; set; }
        public string? Country { get; set; }
        public int? CountryID { get; set; }
        public int? StateID { get; set; }
        public string? CountryName { get; set; }
        public string? StateName { get; set; }

        public int? EntityInstituteInfoID { get; set; }
        public int? ClientID { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPersonMobile { get; set; }
        public string? ContactEmail { get; set; }
    }
}
