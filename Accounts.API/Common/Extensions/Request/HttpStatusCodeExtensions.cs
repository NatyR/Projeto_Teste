using System.Net;

namespace Accounts.API.Common.Extensions.Request
{
    public static class HttpStatusCodeExtensions
    {
        public static bool IsSuccess(this HttpStatusCode statusCode)
        {
            return ((int)statusCode).ToString().StartsWith("2");
        }

        public static bool IsAuthenticationFailed(this HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.Unauthorized || statusCode == HttpStatusCode.Forbidden;
        }
    }
}
