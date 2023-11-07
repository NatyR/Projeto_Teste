
using System.ComponentModel.DataAnnotations;

namespace Portal.API.Dto.Menu
{
    public class MenuUpdateDto
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [Range(1, long.MaxValue, ErrorMessage = "Valor inválido para o campo {0}.")]
        public long Id { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [StringLength(100, ErrorMessage = "O campo {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 3)]
        public string Name { get; set; }

        public string Type { get; set; }
        public long? ParentId { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }

        public long SistemaId { get; set; }

        public string Url { get; set; }
        public bool CanInsert { get; set; }
        public bool CanDelete { get; set; }
        public bool CanUpdate { get; set; }

    }
}
