using System;
using Newtonsoft.Json;
using Portal.API.Common.Annotations.Validations;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Portal.API.Dto.Manual
{
    public class ManualUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        [Required(ErrorMessage = "Por favor selecione o tipo.")]
        public string ManualType { get; set; }

        public string Url { get; set; }
        [MaxLength(200, ErrorMessage = "A descrição deve ter no máximo 200 caracteres")]

        public string Description { get; set; }
        public int Order { get; set; }

        [DataType(DataType.Upload)]
        [MaxFileSize(5 * 1024 * 1024)]
        [AllowedExtensions(new string[] { ".pdf" })]
        [Display(Name = "File")]
        public IFormFile FormFile { get; set; }
    }
}
