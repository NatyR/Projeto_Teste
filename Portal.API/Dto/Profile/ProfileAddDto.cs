using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
namespace Portal.API.Dto.Profile
{
    public class PortalProfileAddDto
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [StringLength(100, ErrorMessage = "O campo {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
        public string Description { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Range(1, long.MaxValue, ErrorMessage = "O campo {0} é obrigatório.")]
        public long SistemaId { get; set; }
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [StringLength(800, ErrorMessage = "O campo {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
        public string Observation { get; set; }
    }
}
