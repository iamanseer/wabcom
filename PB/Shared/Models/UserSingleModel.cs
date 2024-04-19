using PB.Model;
using PB.Shared.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace PB.Shared.Models
{
    public class UserSingleModel
    {
        public int UserID { get; set; }
        public int? EntityID { get; set; }
        public int? EntityTypeID { get; set; }
        public int? EntityPersonalInfoID { get; set; }
        [Required]
        public string? FirstName { get; set; }
        [Required(ErrorMessage = "Please provide a username")]
        public string? UserName { get; set; }
        [RequiredIf("EntityID", null, ErrorMessage = "Please provide a password")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [RequiredIf("EntityID", null, ErrorMessage = "Confirm the password again")]
        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage = "Password and Confirm Password must be equal")]
        public string? ConfirmPassword { get; set; }
        public int? UserTypeID { get; set; }
        [Required(ErrorMessage = "Please provide your company mobile number")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Only numbers are allowed")]
        public string? Phone { get; set; }
        public string? Phone2 { get; set; }
        [Required(ErrorMessage = "Please provide your company e-mail address")]
        [EmailAddress]
        public string? EmailAddress { get; set; }
        public List<BranchUserModel> Branches { get; set; } = new();
        public List<UserTypeAccessModel> Roles { get; set; } = new();
        //public List<ReportsModel> Reports { get; set; }
        public UserRoleAccessViewModel RolesList { get; set; } = new();
        public int? MediaID { get; set; }
        public bool LoginStatus { get; set; }
        public int? ClientID { get; set; }
        public bool IsAccessOnAllRole { get; set; } = false;
        public bool IsAccesOnAllReports { get; set; } = false;

        public string? ValidateUser(UserSingleModel model)
        {
            var validationContext = new ValidationContext(model);
            List<ValidationResult> validationResults = new List<ValidationResult>();

            Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);

            if (validationResults.Count == 0)
            {
                return null;
            }
            else
            {
                return validationResults.First().ErrorMessage;
            }
        }

    }

    public class UserTypeAccessModel
    {
        public int? ID { get; set; }
        public int? RoleID { get; set; }
        public string? RoleName { get; set; }
        public bool HasAccess { get; set; }
        public string? RoleGroupName { get; set; }
        public int? RoleGroupID { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanMail { get; set; }
        public bool CanWhatsapp { get; set; }
        public bool IsRowWise { get; set; }
    }
   
}

