using System;

namespace Accounts.API.Entities
{
    public class IntegracaoApiEmpresa
    {
        public string Url { get; set; }
        public string Response { get; set; }
        public string Request { get; set; }
        
        public string DataEnvio { get; set; }
    }

    public class Acesso
    {
        public Acesso()
        {
            var UtcBr = DateTime.UtcNow;
            CreatedAt = UtcBr.AddHours(-3);
        }
        public Acesso(long? userId, string ip, string url, string method, string postData)
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
