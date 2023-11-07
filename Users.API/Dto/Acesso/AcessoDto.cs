using System;

namespace Users.API.Dto.Acesso
{
    public class AcessoDto
    {
        public long Id { get;  set; }
        public long? UsuarioId { get;  set; }
        public string IP { get;  set; }
        public string Url { get;  set; }
        public string Method { get;  set; }
        public string PostData { get;  set; }
        public string Cpf { get; set; }
        public string Nome { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
        public string Perfil { get; set; }
        public DateTime? CreatedAt { get;  set; }
    }
}
