using Accounts.API.Common.Dto.User;
using System.Collections.Generic;
using System;

namespace Accounts.API.Common.Dto.Account
{
    public class AccountDismissalDto
    {
        public long Convenio { get; set; }

        public UserDtoAccounts CurrentUser { get; set; }
        public List<AccountSelectedDto> Accounts { get; set; }
        public int? UserId { get; set; }

    }

    public class StatusDismissalDto
    {
        public string Cpf { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public int IdPessoaFisica { get; set; }
        public DateTime Data { get; set; }
        public string Observacao { get; set; }
        public int IdBlock { get; set; }
        public int IdTypeBlock { get; set; }
        public string Status { get; set; }
    }
    
}
