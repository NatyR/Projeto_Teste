using System;
using Newtonsoft.Json;
using Portal.API.Common.Annotations.Validations;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Portal.API.Dto.Banner
{
    public class BannerUpdateDto
    {
        public long Id { get; set; }
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [StringLength(200, ErrorMessage = "O campo {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 10)]
        public string Title { get; set; }
        public string Description { get; set; }
        public string Action { get; set; }
        public string Link { get; set; }
        public string Image { get; set; }
        [ValidDate(ErrorMessage = "Data de inicio inválida.")]
        [JsonProperty("dateStart")]
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public string Status { get; set; }
        public int Order { get; set; }
        [DataType(DataType.Upload)]
        [MaxFileSize(1 * 1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".jpeg" })]
        [Display(Name = "File")]
        public IFormFile FormFile { get; set; }
    }
}
