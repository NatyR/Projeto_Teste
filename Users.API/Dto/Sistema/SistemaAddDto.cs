
using System.ComponentModel.DataAnnotations;

namespace Users.API.Dto.Sistema
{
    public class SistemaAddDto
   {
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [StringLength(100, ErrorMessage = "O campo {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
       public string Description { get; set; }
    }
}
