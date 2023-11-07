using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Accounts.API.Common.Dto.Request;
using Accounts.API.Common.Enum.Request;
using Accounts.API.Common.Extensions.Request;
using Accounts.API.Common.Helpers.Interfaces;
using Accounts.API.Common.Middlewares.Exceptions;

namespace Accounts.API.Common.Helpers
{
    public class RequestHelper : IRequestHelper
    {
        #region Métodos de requisições
        public HttpResponseDto ExecuteWithCerts(string url, HttpMethodEnum method, Dictionary<string, string> parameters = null, Dictionary<string, string> headers = null, List<string> certs = null, object body = null, HttpStatusCode? expectStatusCode = null)
        {
            var uri = BindParameters(url, parameters);
            var json = JsonConvert.SerializeObject(body);
            var data = MountContent(json);
            var handler = AddCerts(certs);
            var client = new HttpClient(handler);
            AddHeaders(ref client, headers);

            return Execute(client, uri, data, method, 10, expectStatusCode);
        }

        public HttpResponseDto DeleteWithBody(string url, HttpMethodEnum method, int timeout, Dictionary<string, string> parameters = null,
            Dictionary<string, string> headers = null, object body = null)
        {
            var uri = BindParameters(url, parameters);
            var json = JsonConvert.SerializeObject(body);
            var data = MountContent(json);
            var client = new HttpClient();
            AddHeaders(ref client, headers);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = uri,
                Content = data
            };
            var response = client.SendAsync(request);
            return CreateResult(response);
        }

        public HttpResponseDto ExecuteLoginOAuth2(string url, HttpClientHandler handler, int timeout = 20, Dictionary<string, string> parameters = null, Dictionary<string, string> headers = null, Dictionary<string, string> body = null)
        {
            var uri = BindParameters(url, parameters);
            var content = new FormUrlEncodedContent(body);
            content.Headers.Clear();
            content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(timeout)
            };
            AddHeaders(ref client, headers);

            var response = client.PostAsync(uri, content);
            return CreateResult(response);
        }

        public HttpResponseDto Execute(string url, HttpMethodEnum method, Dictionary<string, string> parameters = null, Dictionary<string, string> headers = null, object body = null, HttpStatusCode? expectStatusCode = null)
        {
            return Execute(url, method, 10, parameters, headers, body, expectStatusCode);
        }

        public HttpResponseDto Execute(string url, HttpMethodEnum method, int timeout, Dictionary<string, string> parameters = null, Dictionary<string, string> headers = null, object body = null, HttpStatusCode? expectStatusCode = null)
        {
            var uri = BindParameters(url, parameters);
            var json = JsonConvert.SerializeObject(body);
            var data = MountContent(json);
            var client = new HttpClient();
            AddHeaders(ref client, headers);

            return Execute(client, uri, data, method, timeout, expectStatusCode);
        }

        public HttpResponseDto ExecuteWithHandler(string url, HttpMethodEnum method, HttpClientHandler handler, int timeout = 20,
            Dictionary<string, string> parameters = null, Dictionary<string, string> headers = null, object body = null,
            HttpStatusCode? expectStatusCode = null)
        {
            var uri = BindParameters(url, parameters);
            var json = JsonConvert.SerializeObject(body);
            var data = MountContent(json);
            var client = new HttpClient(handler);
            AddHeaders(ref client, headers);

            return Execute(client, uri, data, method, timeout, expectStatusCode);
        }

        public HttpResponseDto ExecuteXmlWithHandler(string url, HttpMethodEnum method, HttpClientHandler handler,
            int timeout = 20, Dictionary<string, string> parameters = null, Dictionary<string, string> headers = null, string body = null,
            string contentType = "text/xml")
        {
            var uri = BindParameters(url, parameters);
            var data = string.IsNullOrEmpty(body) ? null : MountContent(body, contentType);
            var client = new HttpClient(handler);
            AddHeaders(ref client, headers);

            return Execute(client, uri, data, method, timeout);
        }

        public T Execute<T>(string url, HttpMethodEnum method, Dictionary<string, string> parameters = null, Dictionary<string, string> headers = null, object body = null, HttpStatusCode? expectStatusCode = null)
        {
            var response = Execute(url, method, parameters, headers, body, expectStatusCode);

            return TryParse<T>(response);
        }


        #endregion

        #region Métodos auxiliares para requisições.
        private HttpResponseDto Execute(HttpClient client, Uri uri, StringContent data, HttpMethodEnum method, int timeout, HttpStatusCode? expectStatusCode = null)
        {
            client.Timeout = TimeSpan.FromSeconds(timeout);
            switch (method)
            {
                case HttpMethodEnum.Get:
                    {
                        var response = Get(client, uri);
                        client.Dispose();

                        return CheckStatusCodeAndReturn(response, expectStatusCode);
                    }

                case HttpMethodEnum.Post:
                    {
                        var response = Post(client, uri, data);
                        client.Dispose();

                        return CheckStatusCodeAndReturn(response, expectStatusCode);
                    }

                case HttpMethodEnum.Put:
                    {
                        var response = Put(client, uri, data);
                        client.Dispose();

                        return CheckStatusCodeAndReturn(response, expectStatusCode);
                    }

                case HttpMethodEnum.Patch:
                    {
                        var response = Patch(client, uri, data);
                        client.Dispose();

                        return CheckStatusCodeAndReturn(response, expectStatusCode);
                    }

                case HttpMethodEnum.Delete:
                    {
                        var response = Delete(client, uri, data);
                        client.Dispose();

                        return CheckStatusCodeAndReturn(response, expectStatusCode);
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        private HttpResponseDto Get(HttpClient client, Uri uri)
        {
            using (var response = client.GetAsync(uri))
            {
                response.Wait();
                return CreateResult(response);
            }
        }

        private HttpResponseDto Post(HttpClient client, Uri uri, StringContent data)
        {
            using (var response = client.PostAsync(uri, data))
            {
                response.Wait();
                return CreateResult(response);
            }
        }

        private HttpResponseDto Patch(HttpClient client, Uri uri, StringContent data)
        {
            using (var response = client.PatchAsync(uri, data))
            {
                response.Wait();
                return CreateResult(response);
            }
        }

        private HttpResponseDto Put(HttpClient client, Uri uri, StringContent data)
        {
            using (var response = client.PutAsync(uri, data))
            {
                response.Wait();
                return CreateResult(response);
            }
        }

        private HttpResponseDto Delete(HttpClient client, Uri uri, StringContent content)
        {
            using (var response = client.DeleteAsync(uri))
            {
                response.Wait();
                return CreateResult(response);
            }
        }

        private HttpResponseDto CreateResult(Task<HttpResponseMessage> response)
        {
            return new HttpResponseDto
            {
                StatusCode = response.Result.StatusCode,
                Content = response.Result.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult(),
                ContentBytes = response.Result.Content.ReadAsByteArrayAsync().ConfigureAwait(false).GetAwaiter().GetResult(),
            };
        }

        private T TryParse<T>(HttpResponseDto response)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(response.Content);
            }
            catch (Exception)
            {
                throw new InternalServerErrorException("Fail parse response.");
            }
        }

        private void AddHeaders(ref HttpClient client, Dictionary<string, string> headers = null)
        {
            if (headers == null)
                return;

            foreach (KeyValuePair<string, string> item in headers)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(item.Key, item.Value);
            }
        }

        private StringContent MountContent(string json, string mediaType = "application/json")
        {
            return new StringContent(json, Encoding.UTF8, mediaType);
        }

        private Uri BindParameters(string url, Dictionary<string, string> parameters = null)
        {
            if (parameters == null)
                return new Uri(url);

            return new Uri(QueryHelpers.AddQueryString(url, parameters));
        }
        #endregion

        #region Métodos auxiliares para requisições internas
        private HttpResponseDto CheckStatusCodeAndReturn(HttpResponseDto response, HttpStatusCode? expectStatusCode)
        {
            if (!expectStatusCode.HasValue)
                return response;

            if (expectStatusCode.Value != response.StatusCode)
                ExceptionHelper.ThrowsByStatusCode(response.StatusCode);

            return response;
        }

        private class LoginSystemDto
        {
            [JsonProperty("access_token")]
            public string Token { get; set; }
        }

        private HttpClientHandler AddCerts(List<string> certs)
        {
            var handler = new HttpClientHandler
            {
                UseCookies = false,
                ClientCertificateOptions = ClientCertificateOption.Manual,
                SslProtocols = SslProtocols.Tls12
            };

            if (certs == null || !certs.Any())
                return handler;

            certs.ForEach(f => handler.ClientCertificates.Add(new X509Certificate2(f)));

            return handler;
        }
        #endregion
    }
}
