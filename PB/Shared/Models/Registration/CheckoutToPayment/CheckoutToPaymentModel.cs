using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Registration.CheckoutToPayment
{
    public class CheckoutToPaymentModel
    {
        public int EntityID { get; set; }
        public int ClientID { get; set; }
        public int UserID { get; set; }
        public int AddressID { get; set; }
        public int InvoiceID { get; set; }
        public int BranchID { get; set; }
        public int InvoiceNo { get; set; }
        public int InvoiceJournalMasterID { get; set; }

        [Required(ErrorMessage = "Please provide your name")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Please provide the company name")]
        public string? CompanyName { get; set; }

        [Required(ErrorMessage = "Please provide your email address")]
        public string? EmailAddress { get; set; }

        [Required(ErrorMessage = "Please provide company address")]
        public string? AddressLine1 { get; set; }

        [Required(ErrorMessage = "Please choose a country from the list")]
        public int? CountryID { get; set; }
        public string? CountryName { get; set; }

        [Required(ErrorMessage = "Please choose a state from the list")]
        public int? StateID { get; set; }
        public string? State { get; set; }

        [Required(ErrorMessage = "Please choose city from the list")]
        public int? CityID { get; set; }
        public string? City { get; set; }

        [Required(ErrorMessage = "Please choose a time zone from the list")]
        public int? ZoneID { get; set; }
        public string? ZoneName { get; set; }
        public string? PinCode { get; set; }

        public MembershipPackageViewModel PackageDetails { get; set; } = new();
    }
}
