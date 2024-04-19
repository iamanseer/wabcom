using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class PlanCheckOutModel
    {
        public int? EntityID { get; set; }
        public int? ClientID { get; set; }
        public int? UserID { get; set; }

        [Required(ErrorMessage = "Please provide your name")]
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Please provide the company name")]
        public string? CompanyName { get; set; }

        [Required(ErrorMessage = "Please provide your email address")]
        public string? EmailAddress { get; set; }

        [Required(ErrorMessage = "Please provide address")]
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
        public int? PackageID { get; set; }
        public string? PackageName { get; set; }
        public string? PackageDescription { get; set; }
        public string? PackageFeatures { get; set; }
        public decimal Fee { get; set; }
        public int? PlanID { get; set; } 
        public int AddressID { get; set; }
        public int InvoiceID { get; set; }
        public int? MonthCount { get; set; }
        public int? UserEntityID { get; set; }
        public int? InvoiceJournalMasterID { get; set; }
        public List<string> features { get; set; } = new();
        public int? BranchID { get; set; }
        public int? PaymentStatus { get; set; }
        public int? TaxCategoryID { get; set; }
        public int? InvoiceNo { get; set; }
    }
}
