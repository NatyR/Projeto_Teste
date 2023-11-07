using System.ComponentModel.DataAnnotations;
using Users.API.Common.Attributes;

namespace Users.API.Dto.Auth
{
    public class SignInDto
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public string Email { get; set; }
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public string Password { get; set; }
        //[Required(ErrorMessage = "O campo {0} é obrigatório")]
        // [GoogleReCaptchaValidation(ErrorMessage = "Não foi possível validar o recaptcha")]
        // public string Recaptcha { get; set; }
    }
}
