using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class ResetPasswordExtendedModel : PasswordModel
    {
        [Required]
        public string? EmailAddress { get; set; }
        public int UserID { get; set; }

    }
}
