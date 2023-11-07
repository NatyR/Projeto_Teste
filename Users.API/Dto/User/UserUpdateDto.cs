using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Users.API.Common.Annotations.Validations;
using Users.API.Common.Enum.User;

namespace Users.API.Dto.User
{
    public class UserUpdateDto
    {

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [Range(1, long.MaxValue, ErrorMessage = "Valor inválido para o campo {0}.")]
        public long Id { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [StringLength(100, ErrorMessage = "O campo {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 3)]
        public string Name { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [StringLength(100, ErrorMessage = "O campo {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
        [ValidEmail(ErrorMessage = "O campo {0} é inválido.")]
        public string Email { get; set; }
       
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{2})\)?[-. ]?([0-9]{5})[-. ]?([0-9]{4})$", ErrorMessage = "Número de telefone inválido")]
        public string Telefone { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [ValidCpf(ErrorMessage = "CPF inválido.")]
        public string Cpf { get; set; }
        public string RegistrationNumber { get; set; }

        public UserTypeEnum UserType { get; set; }
        public long? GroupId { get; set; }
        public long? ProfileId { get; set; }
        [JsonProperty("userConvenios")]
        public List<UserConvenioAddDto> UserConvenios { get; set; }
 
        [JsonProperty("mothersname")]
        public string MothersName { get; set; }

        [UnderAgeValidation]
        [ValidDate(ErrorMessage = "Data de nascimento inválida.")]
        [JsonProperty("birthdate")]
        public DateTime? BirthDate { get; set; }
    }
}
