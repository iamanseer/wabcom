using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.WhatsaApp
{
    public class WhatsappTemplateOtpPostModel
    {
        [Required(ErrorMessage = "Please provide api password")]
        public string? Password { get; set; }
        [Required(ErrorMessage = "Please provide whatsapp account ID")]
        public int? WhatsappAccountID { get; set; }
        [Required(ErrorMessage = "Please provide template name")]
        public string? TemplateName { get; set; }
        [Required(ErrorMessage = "Please provide template ID")]
        public int TemplateID { get; set; }
        [Required(ErrorMessage = "Please provide otp to send")]
        public string? OTP { get; set; }
        [Required(ErrorMessage = "Please provide reciepient phone number")]
        public string? ToNumber { get; set; }  
        
    }
}
