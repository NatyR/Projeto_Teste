using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using Users.API.Dto.Profile;

namespace Users.API.Dto.User
{
    public class UserStatusDto
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [Range(1, long.MaxValue, ErrorMessage = "Valor inválido para o campo {0}.")]
        [JsonProperty("id")]
        public long Id { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [StringLength(100, ErrorMessage = "O campo {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 3)]
        [JsonProperty("status")]
        public string Status { get; set; }

    }
}
