
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class RegistrationModel
    {
        //[Required(ErrorMessageResourceName = nameof(Resources.Validation.Name), ErrorMessageResourceType = typeof(Resources.Validation))]
        [Required(ErrorMessage = "Please provide company name")]
        public string? CompanyName { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Validation.MobileNo), ErrorMessageResourceType = typeof(Resources.Validation))]
        public string? MobileNo { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Validation.Email), ErrorMessageResourceType = typeof(Resources.Validation))]
        public string? Email { get; set; }

        //[Required(ErrorMessageResourceName = nameof(Resources.Validation.Password), ErrorMessageResourceType = typeof(Resources.Validation))]
        [Required(ErrorMessage = "Please provide a password for the account")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }


        //[Required(ErrorMessageResourceName = nameof(Resources.Validation.ConfirmPassword), ErrorMessageResourceType = typeof(Resources.Validation))]
        [Required(ErrorMessage = "Please provide confirm password for the account")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm Password And Password Do Not Match")]
        public string? ConfirmPassword { get; set; }
        [Required]
        public string? ClientName { get; set; }
        public bool IsAgree { get; set; }
        [Required]   
        public int? CountryID { get; set; }
        public bool IsSelect { get; set; }
        public int? PackageID { get; set; }
        public decimal? Fee { get; set; }
        public int? MonthCount { get; set; }
        public int ClientID { get; set; }
        public int EntityID { get; set; }
        public string? CountryName { get; set; }
        public string? ISDCode { get; set; }

        public string? CustomStyle
        {
            get
            {
                if (PackageID == 0)
                {
                    return "opacity: .5;pointer-events: none;";

                }
                return "";
            }
        }

    }
    public class MembershipPackageListModel 
    {
        public int PackageID { get; set; }
        public string? PackageName { get; set; }
        public decimal? Fee { get; set; }
        public int? PlanID { get; set; }
        public string? PackageDescription { get; set; }
        public string? PackageFeatures { get; set; }
        public List<string> featureList { get; set; } = new();
        public bool IsCustom { get; set; }
        public int? MonthCount { get; set; }
        public int? MediaID { get; set; }
        public string? FileName { get; set; }

    }


    public class CustomDetailsModel
    {
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? EmailAddress { get; set; }
        [Required]
        public string? Phone { get; set; }
        public string? CountryName { get; set; }
        public int? CountryID { get; set; }
        [Required]
        public string? Description { get; set; }
        public int? EntityID { get; set; }
        public int? EnquiryNo { get; set; }
        [Required]
        public string? CompanyName { get; set; }
    }
}
