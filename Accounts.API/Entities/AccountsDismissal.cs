using System;

namespace Accounts.API.Entities
{
    public class AccountDismissal
    {
       
        public long Convenio { get; set; }
       
        public string Cpf { get; set; }

        public int UserId { get; set; }
        public long SolicitationId { get; set; }

    }

    public class StatusDismissal
    {       
        public string Cpf { get; set; }
        public string Nanem { get; set; }
        public string Email { get; set; }
        public int PersonId { get; set; }
        public DateTime Data { get; set; }
        public string Observation { get; set; }
        public int IdBlock { get; set; }
        public int IdTypeBlock { get; set; }
        public string CardStatus { get; set; }
    }

    public class ValidDate
    {
       
        public DateTime DtStart { get; set; }
       
        public DateTime DtEnd { get; set; }

        public bool Situation { get; set; }


    }
}
