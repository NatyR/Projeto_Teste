using Accounts.API.Common.Dto.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Accounts.API.Common.Dto.Account
{
    public class AccountLimitDto
    {
        [JsonIgnore]
        public long Convenio { get; set; }

        [JsonProperty("accounts")]
        public List<AccountSelectedDto> Accounts { get; set; }

        public UserDtoAccounts CurrentUser { get; set; }

        public AccountLimitDto()
        {
            Accounts = new List<AccountSelectedDto>();
        }
    }
}
