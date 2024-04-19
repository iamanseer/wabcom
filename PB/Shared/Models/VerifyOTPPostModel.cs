using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class VerifyOTPPostModel
    {
        [Required(ErrorMessageResourceName = nameof(Resources.Validation.Email), ErrorMessageResourceType = typeof(Resources.Validation))]
        public string? UserName { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Validation.OTP), ErrorMessageResourceType = typeof(Resources.Validation))]
        public string? OTP { get; set; }
        public string? MachineID { get; set; }
        public int? UserID { get; set; }
        public int TimeOffset { get; set; }
    }
}
