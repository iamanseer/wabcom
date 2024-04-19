using PB.Shared.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Spin
{
    public class WhatsappContactModel
    {
        public int ContactID { get; set; }

        [Required(ErrorMessage = "Please Enter your Name")]
        public string? Name { get; set; }
        [Required]
        public string? Phone { get; set; }
        public string? ProfileName { get; set; }
        public int? ClientID { get; set; }
        public int? EntityID { get; set; }
        public string? OTP { get; set; }
        public bool IsVerified { get; set; }
        public int? ContestId { get; set; }
        public string? VallidateContactModel(WhatsappContactModel model)
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
                return validationResults[0].ErrorMessage;
            }
        }
    }
}
