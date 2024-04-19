using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PB.Shared.Helpers
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class RequiredIfMultipleAttribute : ValidationAttribute
    {
        private readonly string _dependentProperty;
        private readonly List<int> _requiredIfValues;

        public RequiredIfMultipleAttribute(string dependentProperty, int[] requiredIfValues)
        {
            _dependentProperty = dependentProperty;
            _requiredIfValues = new List<int>(requiredIfValues);
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var dependentPropertyInfo = validationContext.ObjectType.GetProperty(_dependentProperty);

            if (dependentPropertyInfo != null)
            {
                object? dependentPropertyValue = dependentPropertyInfo.GetValue(validationContext.ObjectInstance);

                if (dependentPropertyValue is not null && dependentPropertyValue is int actualDependentPropertyValue)
                {
                    // Check if the dependent property value is in the list of required values
                    if (_requiredIfValues.Contains(actualDependentPropertyValue))
                    {
                        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
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



