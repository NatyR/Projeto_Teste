using Accounts.API.Common.Annotations.Validations;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Accounts.API.Common.Dto
{
    public class FileUploadDto
    {
        [Required(ErrorMessage = "Por favor selecione um arquivo.")]
        [DataType(DataType.Upload)]
        [MaxFileSize(5 * 1024 * 1024)]
        [AllowedExtensions(new string[] { ".csv" })]
        [Display(Name = "File")]
        public IFormFile FormFile { get; set; }
    }
}
