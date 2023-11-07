using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Portal.API.Common.Enum.User;

namespace Portal.API.Entities
{
    public class User
    {
        public long Id { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Telefone{ get; set; }
        public string Name { get; set; }
        public string Cpf { get; set; }

        public UserTypeEnum UserType { get; set; }
        public string RegistrationNumber { get; set; }
        public long? GroupId { get; set; }
        public long? ProfileId { get; set; }
        public int FailedLogins { get; set; }
        public string Status { get; set; }
        public string RecoverPasswordToken { get; set; }
        public DateTime? RecoverPasswordTokenExp { get; set; }
        public ProfilePortal Profile { get; set; }


    }
}
