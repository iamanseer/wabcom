using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace PB.Shared.ValidationAttributes
{
    public class MobileNumberValidationAttribute : ValidationAttribute
    {
        public string? MobileNumber { get; set; }
        Regex Exp = new Regex("^[0-9]{10}$");

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var inputValue = value.ToString().ToLower();
            if(Exp.IsMatch(inputValue))
            {
                return null; 
            }
            return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName });
        }
    }
}
