using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Court
{
    public class CustomerPaymentPostModel
    {
        [Required(ErrorMessage = "Please choose customer")]
        [Range(1, int.MaxValue, ErrorMessage = "please choose customer")]
        public int? EntityID { get; set; }
        [Required(ErrorMessage = "Cash collected should be greater than 0")]
        public decimal Amount { get; set; }
        [Required(ErrorMessage = "please choose payment type")]
        [Range(1,int.MaxValue,ErrorMessage = "please choose payment type")]
        public int PaymentType { get; set; }
    }
}
