using PB.Shared.Tables.CourtClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Court
{
    public class CourtCustomerModel:CourtCustomer
    {
        [Required(ErrorMessage = "Please provide customer name")]
        public string? Name { get; set; }
        public int EntityPersonalInfoID { get; set; }
        public int EntityInstituteInfoID { get; set; }
        public int EntityTypeID { get; set; }
        [Required(ErrorMessage = "Please provide customer phone")]
        public string? Phone { get; set; }
        //[EmailAddress(ErrorMessage = "Please povide vallid e-mail address")]
        [RegularExpression("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", ErrorMessage = "Please provide a valid Email like example@test.com")]
        public string? EmailAddress { get; set; }
        public int? MediaID { get; set; }
        public bool? AllowType { get; set; }
        public int? ContactID { get; set; }
        [Required(ErrorMessage = "Please choose a country")]
        public int? CountryID { get; set; }
        public string? CountryName { get; set; }
        public string? ISDCode { get; set; }
    }
}
