using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;


namespace Users.API.Common.Annotations.Validations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidEmailAttribute : ValidationAttribute
    {
        public ValidEmailAttribute() : base(errorMessage: "Email inválido.")
        {
        }

        public override bool IsValid(object value)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return true;

            var regExpEmail = new Regex("^[A-Za-z0-9](([_.-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([.-]?[a-zA-Z0-9]+)*)([.][A-Za-z]{2,4})$");
            return regExpEmail.IsMatch(value.ToString());
        }
    }
}