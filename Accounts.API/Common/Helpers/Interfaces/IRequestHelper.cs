using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Accounts.API.Common.Dto.Request;
using Accounts.API.Common.Enum.Request;


namespace Accounts.API.Common.Helpers.Interfaces
{
    public interface IRequestHelper
    {
        T Execute<T>(string url, HttpMethodEnum method, Dictionary<string, string> parameters = null, Dictionary<string, string> headers = null, object body = null, HttpStatusCode? expectStatusCode = null);
        HttpResponseDto Execute(string url, HttpMethodEnum method, Dictionary<string, string> parameters = null, Dictionary<string, string> headers = null, object body = null, HttpStatusCode? expectStatusCode = null);
        HttpResponseDto Execute(string url, HttpMethodEnum method, int timeout, Dictionary<string, string> parameters = null, Dictionary<string, string> headers = null, object body = null, HttpStatusCode? expectStatusCode = null);
        HttpResponseDto ExecuteWithHandler(string url, HttpMethodEnum method, HttpClientHandler handler, int timeout = 20, Dictionary<string, string> parameters = null, Dictionary<string, string> headers = null, object body = null, HttpStatusCode? expectStatusCode = null);
        HttpResponseDto ExecuteXmlWithHandler(string url, HttpMethodEnum method, HttpClientHandler handler, int timeout = 20, Dictionary<string, string> parameters = null, Dictionary<string, string> headers = null, string body = null, string contentType = "text/xml");
        HttpResponseDto ExecuteWithCerts(string url, HttpMethodEnum method, Dictionary<string, string> parameters = null, Dictionary<string, string> headers = null, List<string> certs = null, object body = null, HttpStatusCode? expectStatusCode = null);
        HttpResponseDto ExecuteLoginOAuth2(string url, HttpClientHandler handler, int timeout = 20, Dictionary<string, string> parameters = null, Dictionary<string, string> headers = null, Dictionary<string, string> body = null);
        HttpResponseDto DeleteWithBody(string url, HttpMethodEnum method, int timeout, Dictionary<string, string> parameters = null, Dictionary<string, string> headers = null, object body = null);
    }
}
