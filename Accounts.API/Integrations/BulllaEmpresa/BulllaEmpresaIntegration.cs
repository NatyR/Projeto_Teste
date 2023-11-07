using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System;
using Accounts.API.Common.Services;
using Accounts.API.Integrations.BulllaEmpresa.Interfaces;
using Accounts.API.Common.Helpers.Interfaces;
using Accounts.API.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Accounts.API.Common.Repositories.Interfaces;
using Accounts.API.Common.Enum.Request;
using Accounts.API.Common.Dto.Request;
using Accounts.API.Common.Middlewares.Exceptions;
using System.Linq;
using Accounts.API.Integrations.BulllaEmpresa.Responses;
using Accounts.API.Common.Extensions.Request;
using Accounts.API.Integrations.BulllaEmpresa.Request;

namespace Accounts.API.Integrations.BulllaEmpresa
{
    public class BulllaEmpresaIntegration : IntegrationServiceBase, IBulllaEmpresaIntegration
    {
        private readonly IRequestHelper _requestHelper;
        protected readonly string IntegrationName;
        protected string AccessToken;

        private readonly IDistributedCache _cache;
        private const string AccessTokenCacheKey = "BulllaEmpresa_AccessToken";
        private readonly EnvironmentsBase environmentsBase;
        private readonly ILogger<BulllaEmpresaIntegration> _logger;

        public BulllaEmpresaIntegration(IConfiguration _configuration, ILogger<BulllaEmpresaIntegration> logger, IRequestHelper requestHelper, IDistributedCache cache, IIntegrationRepositoryBase integrationRepositoryBase, string integrationName = "BulllaEmpresa") : base(integrationRepositoryBase, $"{Assembly.GetEntryAssembly().GetName().Name}_BulllaEmpresa")
        {
            environmentsBase = new EnvironmentsBase(_configuration);
            _requestHelper = requestHelper;
            IntegrationName = integrationName;
            _cache = cache;
            _logger = logger;
            
        }
        private BulllaEmpresaAuthResponse Auth()
        {
            _logger.LogInformation("Auth");
            var url = $"{environmentsBase.BulllaEmpresaUrlApi}/api/login";
            _logger.LogInformation($"url: {url}");
            var method = HttpMethodEnum.Post;
            var headers = new Dictionary<string, string>
            {
                { "Canal", environmentsBase.BulllaEmpresaChannel },
                { "Content-Type", "application/json" }
            };
            var content = new { login = environmentsBase.BulllaEmpresaUsername, password = environmentsBase.BulllaEmpresaPassword};
            _logger.LogInformation($"text: {content}");
            var request = new Func<HttpResponseDto>(() => _requestHelper.Execute(url, method, null, headers, content));
            var response = Execute(request, "Método", "Login na Bullla Empresa Api");
            if (!response.StatusCode.IsSuccess())
            {
                _logger.LogError($"response: {response.Content}");
                throw new ServiceUnavailableException("Erro ao realizar entrada ao serviço Bullla. Por favor, tente novamente mais tarde ou entre em contato com o suporte. Erro: B001001.");
            }
            _logger.LogInformation($"response: {response.Content}");
            var result = JsonConvert.DeserializeObject<BulllaEmpresaAuthResponse>(response.Content);
            _logger.LogInformation($"result: {result}");
            if (string.IsNullOrEmpty(result?.token))
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

            UpdateAccessToken(Auth());
        }

        private void UpdateAccessToken(BulllaEmpresaAuthResponse bulllaEmpresaAuthResponse)
        {
            Console.WriteLine("-----Horario que o token foi gerado: " + DateTime.Now);
            Console.WriteLine("-----Data de expiração do token no cardholder " + bulllaEmpresaAuthResponse.dataValidade);
            DateTime dataExpiracao = DateTime.ParseExact(bulllaEmpresaAuthResponse.dataValidade, "dd/MM/yyyy HH:mm:ss", null);
            var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(dataExpiracao);
            Console.WriteLine("-----Horario definido para expirar o cache do token no padrao MM/dd/yyyy HH:mm:ss: " + dataExpiracao);
            _cache.Remove(AccessTokenCacheKey);
            _cache.Set(AccessTokenCacheKey, Encoding.ASCII.GetBytes(bulllaEmpresaAuthResponse.token), options);

            AccessToken = bulllaEmpresaAuthResponse.token;
        }

        protected override void OnUpdateAccessToken()
        {
            _cache.Remove(AccessTokenCacheKey);
            AddAccessToken();
        }
        public BulllaEmpresaResponse CancelamentoConta(long convenioId, string user, List<BulllaEmpresaCancelamentoContaRequest> contas)
        {
            try
            {
                if (String.IsNullOrEmpty(AccessToken))
                {
                    AddAccessToken();
                }
                _logger.LogInformation("cancelamento");
                var url = $"{environmentsBase.BulllaEmpresaUrlApi}/api/contas/cancelamento";
                _logger.LogInformation($"url: {url}");
                var callbackUrl = environmentsBase.BulllaEmpresaCallbackUrl;
                var method = HttpMethodEnum.Put;
                var customHeaders = new Dictionary<string, string> {
                { "codigoConvenio", convenioId.ToString() },
                { "idUsuario", user },
                { "urlRetorno", callbackUrl }
            };
                var headers = GetDefaultHeaders(customHeaders);
                _logger.LogInformation($"headers: {String.Join(Environment.NewLine, headers)}");
                var bodyData = new { listaConta = contas };
                _logger.LogInformation($"request: {JsonConvert.SerializeObject(bodyData)}");
                var request = new Func<HttpResponseDto>(() => _requestHelper.Execute(url, method, 300, null, headers, bodyData));
                var response = ExecuteWithInterceptor(request, convenioId.ToString(), "Cancelamento de contas.");
                if (!response.StatusCode.IsSuccess())
                {
                    _logger.LogError($"response: {response.Content}");
                    throw new BadRequestException("Erro ao solicitar cancelamento de contas. Por favor, tente novamente mais tarde ou entre em contato com o suporte. Error:" + response.Content);
                }
                _logger.LogInformation($"response: {response.Content}");
                return JsonConvert.DeserializeObject<BulllaEmpresaResponse>(response.Content);
            }catch(Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                throw ex;
            }
        }
       

        protected void CheckResponseCodeOk<T>(T result) where T : BulllaEmpresaResponse        
        {
            if (result.Codigo != "000")
                throw new BadRequestException(result.Mensagem);
        }

       

        protected virtual Dictionary<string, string> GetDefaultHeaders(Dictionary<string, string> custom = null)
        {
            var baseDictionary = new Dictionary<string, string>
            {
                { "Authorization" , AccessToken },
                { "CANAL" , environmentsBase.BulllaEmpresaChannel },
                { "Content-Type" , "application/json" }
            };
            if (custom == null || !custom.Any())
                return baseDictionary;

            custom.ToList().ForEach(x => baseDictionary[x.Key] = x.Value);
            return baseDictionary;
        }

        
    }
}
