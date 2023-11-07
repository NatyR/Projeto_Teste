using Accounts.API.Common.Dto.User;
using System.Collections.Generic;

namespace Accounts.API.Common.Dto.Account
{
    public class AccountUnblockDto
    {
        public long Convenio { get; set; }
        public UserDtoAccounts CurrentUser { get; set; }
        public List<AccountSelectedDto> Accounts { get; set; }
        
    }
}
