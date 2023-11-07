using Accounts.API.Common.Annotations.Validations;
using Accounts.API.Common.Dto.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Accounts.API.Common.Dto.Account
{
    public class AccountReissueDto
    {
        [JsonIgnore]
        public long Convenio { get; set; }

        [JsonProperty("accounts")]
        public List<AccountSelectedDto> Accounts { get; set; }
        public UserDtoAccounts CurrentUser { get; set; }
    }
}
