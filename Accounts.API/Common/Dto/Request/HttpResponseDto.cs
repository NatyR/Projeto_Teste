using System.Net;

namespace Accounts.API.Common.Dto.Request
{
    public class HttpResponseDto
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; }
        public byte[] ContentBytes { get; set; }
    }
}
