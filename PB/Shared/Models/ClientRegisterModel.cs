using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class ClientRegisterModel
    {
        public int? EntityID { get; set; }
        [Required(ErrorMessage = "Please provide contact person's name")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "Please provide company name")]
        public string? CompanyName { get; set; }
        [Required(ErrorMessage = "Please provide company email addrerss")]
        public string? EmailAddress { get; set; }
        [Required(ErrorMessage = "Please provide company phone number")]
        public string? Phone { get; set; }
        [Required(ErrorMessage = "Please choose a country")]
        public int? CountryID { get; set; }
        public string? CountryName { get; set; }
        [Required(ErrorMessageResourceName = nameof(Resources.Validation.Password), ErrorMessageResourceType = typeof(Resources.Validation))]
        public string? Password { get; set; }
        [Required(ErrorMessageResourceName = nameof(Resources.Validation.ConfirmPassword), ErrorMessageResourceType = typeof(Resources.Validation))]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm Password And Password Do Not Match")]
        public string? ConfirmPassword { get; set; }
        public string? PackageName { get; set; }
        public int? CapacityID { get; set; }
        public int Capacity { get; set; }
        public string? Description { get; set; }
        public int? PlanID { get; set; }
        public string? PlanName { get; set; }
        public int? TaxCategoryID { get; set; }
        public string? TaxCategoryName { get; set; }
        public decimal Fee { get; set; }
        public bool IsCustom { get; set; }
        public string? ISDCode { get; set; }
        public string? Features { get; set; }
        [Required(ErrorMessage = "Please choose a package for client")]
        public int? PackageID { get; set; }
        public int? ClientID { get; set; }
        public bool IsPaid { get; set; }
        public List<IdnValuePair> feature { get; set; } = new();
        public decimal? Discount { get; set; }
        [RequiredIf("IsPaid",true,ErrorMessage = "Please choose a reciept voucher type")]
        public int? VoucherTypeID { get; set; }
    }
}
