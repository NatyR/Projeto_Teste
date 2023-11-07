using Accounts.API.Common.Dto.User;
using System.Collections.Generic;

namespace Accounts.API.Common.Dto.Account
{
    public class GlobalLimitFileUploadDto
    {
        public long shopId { get; set; }
        public string shopName { get; set; }
        public long groupId { get; set; }
        public string groupName { get; set; }
        public int numberofstaff { get; set; }
        public List<FileDto> files { get; set; }
        public UserDtoAccounts  CurrentUser { get; set; }
    }
}
