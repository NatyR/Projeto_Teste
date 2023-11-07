using System;
using Newtonsoft.Json;
using Portal.API.Common.Annotations.Validations;
using System.ComponentModel.DataAnnotations;

namespace Portal.API.Dto.Faq
{
    public class FaqUpdateDto
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [Range(1, long.MaxValue, ErrorMessage = "Valor inválido para o campo {0}.")]
        public long Id { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [StringLength(200, ErrorMessage = "O campo {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 10)]
        public string Question { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [StringLength(800, ErrorMessage = "O campo {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 10)]
        public string Answer { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [Range(1, long.MaxValue, ErrorMessage = "Valor inválido para o campo {0}.")]
        public int Order { get; set; }
    }
}
