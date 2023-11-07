using Microsoft.AspNetCore.Http;

namespace Accounts.API.Common.Dto.Account
{
    public class FileDto
    {
        public string description { get; set; }
        public IFormFile file { get; set; }
    }
}
