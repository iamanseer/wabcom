using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class SignupModel
    {
        [Required(ErrorMessageResourceName = nameof(Resources.Validation.Name), ErrorMessageResourceType = typeof(Resources.Validation))]
        public string? Name { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Validation.MobileNo), ErrorMessageResourceType = typeof(Resources.Validation))]
        public string? MobileNo { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Validation.Email), ErrorMessageResourceType = typeof(Resources.Validation))]
        public string? Email { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Validation.Password), ErrorMessageResourceType = typeof(Resources.Validation))]
        public string? Password { get; set; }


        [Required(ErrorMessageResourceName = nameof(Resources.Validation.ConfirmPassword), ErrorMessageResourceType = typeof(Resources.Validation))]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage= "Confirm Password And Password Do Not Match")]
        public string? ConfirmPassword { get; set; }
        public string? ClientName { get; set; }
        public string? CompanyName { get; set; }

    }
}
