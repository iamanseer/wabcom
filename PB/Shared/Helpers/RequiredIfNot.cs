using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Helpers
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class RequiredIfNot : ValidationAttribute
    {
        private readonly string _dependentProperty;
        private readonly object _targetValue;
        public RequiredIfNot(string dependentProperty, object targetValue)
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
                    if ((bool)_targetValue != (bool)dependentPropertyValue && value is null)
                    {
                        return new ValidationResult(ErrorMessage ?? "This field is required", new[] { validationContext.MemberName });
                    }
                }
            }
            return ValidationResult.Success;
        }
    }
}
