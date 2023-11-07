using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Users.API.Common.Enum.User;

namespace Users.API.Entities
{
    public class Acesso
    {
        public Acesso()
        {
            var UtcBr = DateTime.UtcNow;
            CreatedAt = UtcBr.AddHours(-3);
        }
        public Acesso(long? userId,string ip, string url, string method, string postData)
        {
            var UtcBr = DateTime.UtcNow;
            CreatedAt = UtcBr.AddHours(-3);
            UsuarioId = userId;
            Url = url;
            IP = ip;
            Method = method;
            PostData = postData;
        }
        public long Id { get; private set; }
        public long? UsuarioId { get; private set; }
        public string IP { get; private set; }
        public string Url { get; private set; }
        public string Method { get; private set; }
        public string PostData { get; private set; }
        public DateTime? CreatedAt { get; private set; }

        public string Nome { get; set; }
        public string Cpf { get; set; }

        public string Telefone { get; set; }
        public string Email { get; set; }
        public string Perfil { get; set; }

        public void SetId(long id)
        {
            Id = id;
        }
        public void SetUsuarioId(long? usuarioId)
        {
            UsuarioId = usuarioId;
        }
        public void SetUrl(string url)
        {
            Url = url;
        }
        public void SetIP(string ip)
        {
            IP = ip;
        }
        public void SetMethod(string method)
        {
            Method = method;
        }
        public void SetPostData(string postData)
        {
            PostData = postData;
        }
        public void SetCreatedAt(DateTime? createdAt)
        {
            CreatedAt = createdAt;
        }
    }
}
