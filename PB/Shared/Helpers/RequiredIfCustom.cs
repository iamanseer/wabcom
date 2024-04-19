using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Helpers
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class RequiredIfCustom : ValidationAttribute
    {
        private readonly string _dependentProperty;
        private readonly int _targetValue;

        public RequiredIfCustom(string dependentProperty, int targetValue)
        {
            _dependentProperty = dependentProperty;
            _targetValue = targetValue;
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var dependentPropertyInfo = validationContext.ObjectType.GetProperty(_dependentProperty);
            if (dependentPropertyInfo != null)
            {
                object? dependentPropertyValue = dependentPropertyInfo.GetValue(validationContext.ObjectInstance);
                if (dependentPropertyValue is not null)
                {
                    // Check if the dependent property value is target value
                    if (_targetValue == (int)dependentPropertyValue)
                    {
                        if (value == null || (int)value == 0)
                        {
                            return new ValidationResult(ErrorMessage ?? "This field is required", new[] { validationContext.MemberName });
                        }
                    }
                }
            }
            return ValidationResult.Success;
        }
    }
}
