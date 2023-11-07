using Portal.API.Common.Dto.Request;
using Portal.API.Common.Enum.Request;
using Portal.API.Common.Middlewares.Exceptions;
using Portal.API.Common;
using System.Collections.Generic;
using System;
using Portal.API.Integrations.Bullla.Utils;
using Portal.API.Common.Helpers.Interfaces;
using Portal.API.Common.Repositories.Interfaces;
using System.Reflection;
using Portal.API.Common.Services;
using Portal.API.Integrations.Interfaces;
using Portal.API.Integrations.Bullla.Responses;
using Portal.API.Common.Extensions.Request;
using Amazon.Runtime.Internal.Util;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using System.Linq;
using Amazon.Runtime.Internal;
using Portal.API.Integrations.Bullla.Interfaces;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Portal.API.Integrations.Bullla
{
    public class BulllaIntegration : IntegrationServiceBase, IBulllaIntegration
    {
        private readonly IRequestHelper _requestHelper;
        protected readonly string IntegrationName;
        protected string AccessToken;

        private readonly IDistributedCache _cache;
        private const string AccessTokenCacheKey = "Bullla_AccessToken";
        private readonly EnvironmentsBase environmentsBase;
        private BulllaEncryptor BulllaEncryptor;
        private readonly ILogger<BulllaIntegration> _logger;

        public BulllaIntegration(IConfiguration _configuration, ILogger<BulllaIntegration> logger, IRequestHelper requestHelper, IDistributedCache cache, IIntegrationRepositoryBase integrationRepositoryBase, string integrationName = "Bullla") : base(integrationRepositoryBase, $"{Assembly.GetEntryAssembly().GetName().Name}_Bullla")
        {
            environmentsBase = new EnvironmentsBase(_configuration);
            BulllaEncryptor = new BulllaEncryptor(_configuration);
            _requestHelper = requestHelper;
            IntegrationName = integrationName;
            _cache = cache;
            _logger = logger;
            AddAccessToken();
        }
        private BulllaLoginResponse Login()
        {
            _logger.LogInformation("Login");
            var url = $"{environmentsBase.BulllaAuthUrlApi}";
            _logger.LogInformation($"url: {url}");
            var method = HttpMethodEnum.Post;
            var headers = new Dictionary<string, string>
            {
                { "Canal", "PORTALRH" },
                { "Content-Type", "application/json" }
            };
            var content = new { login = environmentsBase.BulllaLogin, password = environmentsBase.BulllaPassword };
            var text = Encrypt(content);
            _logger.LogInformation($"text: {text}");
            var request = new Func<HttpResponseDto>(() => _requestHelper.Execute(url, method, null, headers, text));
            var response = Execute(request, "Método", "Login na Bullla");
            if (!response.StatusCode.IsSuccess())
                throw new ServiceUnavailableException("Erro ao realizar entrada ao serviço Bullla. Por favor, tente novamente mais tarde ou entre em contato com o suporte. Erro: B001001.");
            _logger.LogInformation($"response: {response.Content}");
            var result = Decrypt<BulllaLoginResponse>(response.Content);
            _logger.LogInformation($"result: {result}");
            if (string.IsNullOrEmpty(result?.Token))
                throw new ServiceUnavailableException("Erro ao realizar entrada ao serviço Bullla. Por favor, tente novamente mais tarde ou entre em contato com o suporte. Erro: B001002.");

            return result;
        }

        private void AddAccessToken()
        {
            var bytes = _cache.Get(AccessTokenCacheKey);
            if (bytes != null && bytes.Any())
            {
                AccessToken = Encoding.Default.GetString(bytes);
                return;
            }

            UpdateAccessToken(Login());
        }

        private void UpdateAccessToken(BulllaLoginResponse bulllaLoginResponse)
        {
            Console.WriteLine("-----Horario que o token foi gerado: " + DateTime.Now);
            Console.WriteLine("-----Data de expiração do token no cardholder " + bulllaLoginResponse.dataValidade);
            DateTime dataExpiracao = DateTime.ParseExact(bulllaLoginResponse.dataValidade, "dd/MM/yyyy HH:mm:ss", null);
            var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(dataExpiracao);
            Console.WriteLine("-----Horario definido para expirar o cache do token no padrao MM/dd/yyyy HH:mm:ss: " + dataExpiracao);
            _cache.Remove(AccessTokenCacheKey);
            _cache.Set(AccessTokenCacheKey, Encoding.ASCII.GetBytes(bulllaLoginResponse.Token), options);

            AccessToken = bulllaLoginResponse.Token;
        }

        protected override void OnUpdateAccessToken()
        {
            _cache.Remove(AccessTokenCacheKey);
            AddAccessToken();
        }

        public BulllaAllowedShopsResponse GetShops(string email)
        {
            _logger.LogInformation("GetShops");
            var url = $"{environmentsBase.BulllaUrlApi}/portal/{email}";
            _logger.LogInformation($"url: {url}");
            var method = HttpMethodEnum.Get;
            var headers = new Dictionary<string, string> {};
            var request = new Func<HttpResponseDto>(() => _requestHelper.Execute(url, method, 30, null, GetDefaultHeaders(headers)));
            var response = ExecuteWithInterceptor(request, email, "Consulta de convenios do email.");
            if (!response.StatusCode.IsSuccess())
                throw new ServiceUnavailableException("Erro ao consultar convenios por e-mail. Por favor, tente novamente mais tarde ou entre em contato com o suporte. Erro: B001003.");
            _logger.LogInformation($"response: {response.Content}");
            var result = JsonConvert.DeserializeObject<BulllaAllowedShopsResponse>(response.Content);
            //var result = TryParseResponse<BulllaAllowedShopsResponse>(response);
            //CheckResponseCodeOk(result);

            return result;
        }

        public T Decrypt<T>(string content) where T : class
        {
            return BulllaEncryptor.Decrypt<T>(content);
        }

        public string Encrypt(object content)
        {
            return BulllaEncryptor.Encrypt(content);
        }

        protected void CheckResponseCodeOk<T>(T result) where T : BulllaResponseBase
        {
            if (!result.IsValid)
                throw new BadRequestException(result.Message);
        }

        public override T TryParseResponse<T>(HttpResponseDto response, bool checkStatusCodeSuccessfully = true)
        {
            if (checkStatusCodeSuccessfully && !response.StatusCode.IsSuccess())
            {
                Log.Logger.Fatal($"{IntegrationName} - Erro ao receber resposta. Erro: {response.Content}");
                throw new ServiceUnavailableException();
            }

            try
            {
                return Decrypt<T>(response.Content);
            }
            catch (Exception e)
            {
                Log.Logger.Fatal(e, $"{IntegrationName} - Erro ao desserializar objeto -> {response.Content} para o tipo -> {typeof(T)}");
                throw new ServiceUnavailableException();
            }
        }

        protected virtual Dictionary<string, string> GetDefaultHeaders(Dictionary<string, string> custom = null)
        {
            var baseDictionary = new Dictionary<string, string>
            {
                { "Authorization" , AccessToken },
                { "CANAL" , "PORTALRH" },
                { "Content-Type" , "application/json" }
            };
            if (custom == null || !custom.Any())
                return baseDictionary;

            custom.ToList().ForEach(x => baseDictionary[x.Key] = x.Value);
            return baseDictionary;
        }
    }
}
