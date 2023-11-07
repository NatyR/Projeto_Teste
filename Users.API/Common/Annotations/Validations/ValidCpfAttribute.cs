using Maoli;
using System;
using System.ComponentModel.DataAnnotations;

namespace Users.API.Common.Annotations.Validations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class ValidCpfAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return true;

            return Cpf.Validate(value.ToString());
        }
    }
}