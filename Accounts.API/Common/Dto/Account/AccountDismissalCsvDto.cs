using Accounts.API.Common.Annotations.Validations;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Accounts.API.Common.Dto.Account
{
    public class AccountDismissalCsvDto
    {

        public string Cpf { get; set; }

        public string Name { get; set; }
        public string? ReasonDescription { get; set; }
    }
}
