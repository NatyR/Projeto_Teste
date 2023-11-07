using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;


namespace Users.API.Common.Annotations.Validations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class ValidDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return true;

            return DateTime.TryParseExact(value.ToString(), "MM/dd/yyyy HH:mm:ss",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }
    }
}
