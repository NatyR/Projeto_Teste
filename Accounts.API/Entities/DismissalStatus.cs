using System;

namespace Accounts.API.Entities
{
    public class DismissalStatus
    {
       
        public long Convenio { get; set; }
        public string Name { get; set; }       
        public string Cpf { get; set; }
        public int UserId { get; set; }

    }
}
