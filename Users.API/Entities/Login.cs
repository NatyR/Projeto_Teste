using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Users.API.Common.Enum.User;

namespace Users.API.Entities
{
    public class Login
    {
        public Login()
        {
            CreatedAt = DateTime.Now;
        }
        public Login(long? userId,string email,string ip, string userAgent, string status)
        {
            CreatedAt = DateTime.Now;
            UsuarioId = userId;
            Email = email;
            IP = ip;
            UserAgent = userAgent;
            Status = status;
        }
        public long Id { get; private set; }
        public long? UsuarioId { get; private set; }
        public string Email { get; private set; }
        public string IP { get; private set; }
        public string UserAgent { get; private set; }
        public string UserName { get; private set; }
        public string Status { get; private set; }
        public DateTime? CreatedAt { get; private set; }
        public DateTime? LastLogin { get; private set; }

        public void SetId(long id)
        {
            Id = id;
        }
        public void SetUsuarioId(long? usuarioId)
        {
            UsuarioId = usuarioId;
        }
        public void SetEmail(string email)
        {
            Email = email;
        }
        public void SetIP(string ip)
        {
            IP = ip;
        }
        public void SetUserAgent(string userAgent)
        {
            UserAgent = userAgent;
        }
        public void SetStatus(string status)
        {
            Status = status;
        }
        public void SetCreatedAt(DateTime? createdAt)
        {
            CreatedAt = createdAt;
        }
    }
}
