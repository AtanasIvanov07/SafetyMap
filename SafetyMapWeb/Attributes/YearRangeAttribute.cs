using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SafetyMapWeb.Attributes
{
    public class YearRangeAttribute : ValidationAttribute, IClientModelValidator
    {
        private readonly int _minYear;

        public YearRangeAttribute(int minYear)
        {
            _minYear = minYear;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is int year)
            {
                int currentYear = DateTime.Now.Year;
                if (year < _minYear || year > currentYear)
                {
                    return new ValidationResult($"The field {validationContext.DisplayName} must be between {_minYear} and {currentYear}.", new[] { validationContext.MemberName! });
                }
            }
            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var currentYear = DateTime.Now.Year;
            var errorMessage = $"The field {context.ModelMetadata.GetDisplayName()} must be between {_minYear} and {currentYear}.";

            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-range", errorMessage);
            MergeAttribute(context.Attributes, "data-val-range-min", _minYear.ToString());
            MergeAttribute(context.Attributes, "data-val-range-max", currentYear.ToString());
        }

        private static bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key))
            {
                return false;
            }
            attributes.Add(key, value);
            return true;
        }
    }
}
