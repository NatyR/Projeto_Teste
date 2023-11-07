using System.ComponentModel.DataAnnotations;

namespace Users.API.Dto.Profile
{
    public class ProfileUpdateDto
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [Range(1, long.MaxValue, ErrorMessage = "Valor inválido para o campo {0}.")]
        public long Id { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [StringLength(100, ErrorMessage = "O campo {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
        public string Description { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [Range(1, long.MaxValue, ErrorMessage = "O campo {0} é obrigatório.")]
        public long SistemaId { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [StringLength(800, ErrorMessage = "O campo {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
        public string Observation { get; set; }
    }
}
