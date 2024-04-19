using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class PaymentVerificationModel
    {
        [Required]
        public string? PaymentRefNo { get; set; }
        public int? MediaID { get; set; }
        public int? ClientID { get; set; }
    }

}
