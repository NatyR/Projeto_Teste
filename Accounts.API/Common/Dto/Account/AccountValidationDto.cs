using System.Collections.Generic;

namespace Accounts.API.Common.Dto.Account
{
    public class AccountValidationDto
    {
        public bool Success { get; set; }
        public List<AccountOccurrenceDto> Occurrences { get; set; }
        public List<AccountDto> Accounts { get; set; }

        public AccountValidationDto()
        {
            Occurrences = new List<AccountOccurrenceDto>();
            Accounts = new List<AccountDto>();
        }
    }
}
