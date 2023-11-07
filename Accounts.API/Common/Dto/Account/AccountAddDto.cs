using Accounts.API.Common.Annotations.Validations;
using Accounts.API.Common.Dto.User;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Accounts.API.Common.Dto.Account
{
    public class AccountAddDto
    {
        [JsonIgnore]
        public long Convenio { get; set; }

        [ValidDate(ErrorMessage = "Data de admissão inválida.")]
        [JsonProperty("admissiondate")]
        public DateTime? AdmissionDate { get; set; }

        [ValidEmail(ErrorMessage = "Endereço de e-mail inválido")]
        [JsonProperty("email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [ValidCpf(ErrorMessage = "CPF inválido.")]
        [JsonProperty("cpf")]
        public string Cpf { get; set; }

        [JsonProperty("rg")]
        public string Rg { get; set; }

        [JsonProperty("rgIssuer")]
        public string RgIssuer { get; set; }

        [UnderAgeValidation]
        [MajorValidation]
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [ValidDate(ErrorMessage = "Data de nascimento inválida.")]
        [JsonProperty("birthdate")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [JsonProperty("mothersname")]
        public string MothersName { get; set; }

        [JsonProperty("fathersname")]
        public string FathersName { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [JsonProperty("cardname")]
        public string CardName { get; set; }

        [JsonProperty("neighborhood ")]
        public string Neighborhood { get; set; }

        [JsonProperty("addressnumber")]
        public string AddressNumber { get; set; }

        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("cityname")]
        public string CityName { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [RegularExpression(@"^([0-9]{5})[-]?([0-9]{3})$", ErrorMessage = "CEP inválido")]
        [JsonProperty("zipcode")]
        public string ZipCode { get; set; }

        [JsonProperty("addresscomplement")]
        public string AddressComplement { get; set; }

        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{2})\)?[-. ]?([0-9]{5})[-. ]?([0-9]{4})$", ErrorMessage = "Número de telefone inválido")]
        [JsonProperty("phonenumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("costcenter")]
        public long? CostCenter { get; set; }

        [JsonProperty("branch")]
        public long? Branch { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [JsonProperty("cardlimit")]
        public decimal CardLimit { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [JsonProperty("registrationnumber")]
        public string RegistrationNumber { get; set; }

        public UserDtoAccounts CurrentUser { get; set; }
    }

    /// <summary>
    /// Valida se a pessoa cadastrada tem menos de 18 anos
    /// </summary>
    public class UnderAgeValidation : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object value, ValidationContext context)
        {

            DateTime minDate = DateTime.Now.AddYears(-18);
            DateTime validateDate = (DateTime)value;

            if(validateDate > minDate)
            {
                return new ValidationResult("Usuário menor de 18 anos");
            }
            else
            {
                return ValidationResult.Success;
            }

        }
    }
    public class MajorValidation : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object value, ValidationContext context)
        {

            DateTime maxDate = DateTime.Now.AddYears(-65);
            DateTime validateDate = (DateTime)value;

            if (validateDate < maxDate)
            {
                return new ValidationResult("Usuário maior de 65 anos");
            }
            else
            {
                return ValidationResult.Success;
            }

        }
    }
}
