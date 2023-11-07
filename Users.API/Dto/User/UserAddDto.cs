
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Users.API.Common.Annotations.Validations;
using Users.API.Common.Enum.User;

namespace Users.API.Dto.User
{
    public class UserAddDto
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [StringLength(100, ErrorMessage = "O campo {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 3)]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [ValidEmail(ErrorMessage = "Endereço de e-mail inválido")]
        [JsonProperty("email")]
        public string Email { get; set; }


        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{2})\)?[-. ]?([0-9]{5})[-. ]?([0-9]{4})$", ErrorMessage = "Número de telefone inválido")]
        [JsonProperty("telefone")]
        public string Telefone { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [ValidCpf(ErrorMessage = "CPF inválido.")]
        [JsonProperty("cpf")]
        public string Cpf { get; set; }

        [JsonProperty("registrationnumber")]
        public string RegistrationNumber { get; set; }

        [JsonProperty("userType")]
        public UserTypeEnum UserType { get; set; }

        [JsonProperty("groupid")]
        public long? GroupId { get; set; }

        [JsonProperty("profileid")]
        public long? ProfileId { get; set; }

        [JsonProperty("userConvenios")]
        public List<UserConvenioAddDto>? UserConvenios { get; set; }
               
        [JsonProperty("mothersname")]
        public string MothersName { get; set; }

        [UnderAgeValidation]      
        [ValidDate(ErrorMessage = "Data de nascimento inválida.")]
        [JsonProperty("birthdate")]
        public DateTime? BirthDate { get; set; }

    }
    public class UnderAgeValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (!(value is DateTime validateDate))
            {
                return new ValidationResult("Data inválida");
            }

            DateTime minDate = DateTime.Now.AddYears(-18);

            if (validateDate > minDate)
            {
                return new ValidationResult("Usuário menor de 18 anos");
            }
            else
            {
                return ValidationResult.Success;
            }
        }
    }
}
