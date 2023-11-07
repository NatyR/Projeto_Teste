using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Portal.API.Common.Annotations.Validations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class AlphanumericAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value != null && value.ToString().All(x => char.IsLetterOrDigit(x) || char.IsWhiteSpace(x)))
                return true;

            return false;
        }
    }
}
