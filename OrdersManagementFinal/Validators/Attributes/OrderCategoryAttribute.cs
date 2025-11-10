using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Week4.Features.Orders;


namespace Week4.Validators.Attributes
    {
        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
        public class OrderCategoryAttribute : ValidationAttribute
        {
            private readonly OrderCategory[] _allowedCategories;

            public OrderCategoryAttribute(params OrderCategory[] allowedCategories)
            {
                _allowedCategories = allowedCategories;
            }

            public override bool IsValid(object? value)
            {
                if (value == null) return true;

                if (value is not OrderCategory category)
                    return false;

                return _allowedCategories.Contains(category);
            }

            public override string FormatErrorMessage(string name)
            {
                var allowed = string.Join(", ", _allowedCategories);
                return $"Invalid category. Allowed categories: {allowed}.";
            }
        }
    }


