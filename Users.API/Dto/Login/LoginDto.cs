using System;

namespace Users.API.Dto.Login
{
    public class LoginDto
    {
        public long Id { get;  set; }
        public long? UsuarioId { get;  set; }
        public string Email { get;  set; }

        public string UserName{ get; set; }
        public string IP { get;  set; }
        public string UserAgent { get;  set; }
        public string Status { get;  set; }
        public DateTime? CreatedAt { get;  set; }
    }
}
