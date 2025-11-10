using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Week4.Validators.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ValidISBNAttribute : ValidationAttribute, IClientModelValidator
    {
        public ValidISBNAttribute()
        {
            ErrorMessage = "Invalid ISBN format (must be 10 or 13 digits).";
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true; 

            var isbn = value.ToString()!.Replace("-", "").Replace(" ", "");

            var regex = new Regex(@"^\d{9}[\dXx]$|^\d{13}$");
            return regex.IsMatch(isbn);
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-isbn", ErrorMessage!);
        }

        private bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key))
                return false;

            attributes.Add(key, value);
            return true;
        }
    }
}
