
using System.ComponentModel.DataAnnotations;

namespace Portal.API.Dto.Sistema
{
  public class SistemaUpdateDto
  {
    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [Range(1, long.MaxValue, ErrorMessage = "Valor inválido para o campo {0}.")]
    public long Id { get; set; }

    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [StringLength(100, ErrorMessage = "O campo {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
    public string Description { get; set; }
  }
}
