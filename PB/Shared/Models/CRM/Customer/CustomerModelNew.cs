using PB.Model;
using PB.Shared.Enum;
using PB.Shared.Tables.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.CRM.Customer
{
    public class CustomerModelNew
    {
        public int CustomerID { get; set; }
        public int? EntityID { get; set; }
        public int Status { get; set; } = 1;
        [Required(ErrorMessage = "Please choose the type of customer")]
        public int Type { get; set; } = (int)CustomerTypes.Individual;
        public string? Remarks { get; set; }
        public int? ClientID { get; set; }
        public string? TaxNumber { get; set; }
        //[Required(ErrorMessage = "Please provide customer name")]
        [RequiredIf("Type", (int)CustomerTypes.Individual, ErrorMessage = "Please provide customer name")]
        public string? Name { get; set; }
        [RequiredIf("Type", (int)CustomerTypes.Business, ErrorMessage = "Please provide company name")]
        public string? CompanyName { get; set; }
        public int EntityPersonalInfoID { get; set; }
        public int EntityInstituteInfoID { get; set; }
        public int EntityTypeID { get; set; }
        //[Required(ErrorMessage = "Please provide customer phone")]
        //[RegularExpression("^[0-9]+$", ErrorMessage = "Only numbers are allowed")]
        public string? Phone { get; set; }
        //[EmailAddress(ErrorMessage = "Please povide vallid e-mail address")]
        [RegularExpression("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", ErrorMessage = "Please provide a valid Email like example@test.com")]
        public string? EmailAddress { get; set; }
        public int? MediaID { get; set; }
        public bool? AllowType { get; set; }
        public int? ContactID { get; set; }
        // [Required(ErrorMessage = "Please choose a country")]
        public int? CountryID { get; set; } 
        public string? CountryName { get; set; }
        public string? ISDCode { get; set; }
        public DateTime? JoinedOn { get; set; } = DateTime.UtcNow;
        public int? CategoryID { get; set; }
        public string? CategoryName { get; set; }
        public int? BusinessTypeID { get; set; }
        public string? BusinessTypeName { get; set; }
        public string? TallyEmailID { get; set; }
        public int? CustomerPriority { get; set; }
        public int? OwnedBy { get; set; }
        public string? StaffName { get; set; }
        public string? BusinessGivenInValue { get; set; }
        public int? BusinessGivenInNos { get; set; }
        public DateTime? LastBusinessDate { get; set; }
        public decimal? LastBusinessAmount { get; set; }
        public int? LastBusinessType { get; set; }
        public int? AMCStatus { get; set; }
        public DateTime? AMCExpiry { get; set; }
        public int? SubscriptionID { get; set; }
        public DateTime? SubscriptionExpiry { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Long { get; set; }
        public string? AccountantISDCode { get; set; }
        public string? AccountantContactNo { get; set; }
        public string? AccountantEmailID { get; set; }
        public CustomerSerialNoModel TallyNos { get; set; } = new();
        public List<AddressView> Addresses { get; set; } = new();
        public List<CustomerContactPersonModel> ContactPersons { get; set; } = new();
        public List<CustomerServiceModel> CustomerServices { get; set; } = new();


    }

    public class CustomerSerialNoModel
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "Please add tally serial no")]
        [StringLength(maximumLength: 9, ErrorMessage = "Length must be between 0 and 9 characters")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "tally serial no field should only contain white spaces,small letters,numbers and hyphens")]
        public string? TallySerialNo { get; set; } = "0";
        [StringLength(maximumLength: 9, ErrorMessage = "Length must be between 0 and 9 characters")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "tally serial no2 field should only contain white spaces,small letters,numbers and hyphens")]
        public string? TallySerialNo2 { get; set; } = "0";
        [StringLength(maximumLength: 9, ErrorMessage = "Length must be between 0 and 9 characters")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "tally serial no3 field should only contain white spaces,small letters,numbers and hyphens")]
        public string? TallySerialNo3 { get; set; } = "0";
        public int? CustomerID { get; set; }
    }
}
