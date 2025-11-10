using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Week4.Validators.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PriceRangeAttribute : ValidationAttribute
    {
        private readonly decimal _min;
        private readonly decimal _max;

        public PriceRangeAttribute(double min, double max)
        {
            _min = Convert.ToDecimal(min);
            _max = Convert.ToDecimal(max);
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true;

            if (decimal.TryParse(value.ToString(), out var price))
            {
                return price >= _min && price <= _max;
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be between {_min.ToString("C2", CultureInfo.InvariantCulture)} and {_max.ToString("C2", CultureInfo.InvariantCulture)}.";
        }
    }
}
