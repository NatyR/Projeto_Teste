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
using Accounts.API.Integrations.BulllaEmpresa;
using System.Threading.Tasks;
using Amazon.Runtime.Internal.Util;
using Accounts.API.Common.Dto.As;
using Accounts.API.Common.Dto.Account;
using Amazon.S3.Model;
using Accounts.API.Interfaces.Repositories;
using System.Reflection.Metadata;
using AutoMapper.Features;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Accounts.API.Entities;

namespace Accounts.API.Integrations.As
{
    public class AsIntegration : IntegrationServiceBase, IAsIntegration
    {
        private readonly IRequestHelper _requestHelper;
        protected readonly string IntegrationName;
        private readonly EnvironmentsBase environmentsBase;
        private readonly ILogger<AsIntegration> _logger;

        protected string AccessToken;
        private readonly IDistributedCache _cache;
        private const string AccessTokenCacheKey = "BulllaEmpresa_AccessToken";

        private readonly IAccessRepositoryAccounts _acessoRepository;

        public AsIntegration(IConfiguration _configuration, ILogger<AsIntegration> logger, IRequestHelper requestHelper, IIntegrationRepositoryBase integrationRepositoryBase, IDistributedCache cache, IAccessRepositoryAccounts acessoRepository, string integrationName = "BulllaEmpresa") : base(integrationRepositoryBase, $"{Assembly.GetEntryAssembly().GetName().Name}_BulllaEmpresa")
        {
            environmentsBase = new EnvironmentsBase(_configuration);
            _requestHelper = requestHelper;
            IntegrationName = integrationName;
            _logger = logger;

            _cache = cache;
            _acessoRepository = acessoRepository;
        }

        public string DesbloqueioDeSecuranca(string codigoConvenio, int IdUsuario, AccountModel listaContas)
        {
            List<int> contas = listaContas?.accounts?.Select(a => a.idConta)?.ToList();
            if (contas == null || !contas.Any())
            {
                throw new ServiceUnavailableException("Por favor, selecione pelo menos uma conta para ser desbloqueada.");
            }

            if (String.IsNullOrEmpty(AccessToken))
            {
                AddAccessToken();
            }
            _logger.LogInformation("desbloqueio-seguranca");
            var url = $"{environmentsBase.BulllaEmpresaUrlApi}/api/contas/desbloqueio-seguranca";
            _logger.LogInformation($"url: {url}");
            var method = HttpMethodEnum.Put;
            try
            {

                var customHeaders = new Dictionary<string, string> {
                { "codigoConvenio", codigoConvenio.ToString() },
                { "idUsuario",  IdUsuario.ToString() }
                };
                var headers = GetDefaultHeaders(customHeaders);

                _logger.LogInformation($"listaContas: {contas.ToArray()}");
                var request = new Func<HttpResponseDto>(() => _requestHelper.Execute(url, method, null, headers, new { idConta = contas.ToArray() }));

                var response = Execute(request, "Método", "Desbloqueio de Segurança");

                if (!response.StatusCode.IsSuccess())
                    throw new ServiceUnavailableException("Erro ao realizar entrada ao serviço Bullla. Por favor, tente novamente mais tarde ou entre em contato com o suporte. Erro: B001001.: " + response.Content);
                _logger.LogInformation($"response: {response.Content}");

                return response.Content;
            }
            catch (Exception ex)
            {

                throw new Exception("Erro ao acessar serviço:" + url, ex);
            }

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

        internal BulllaEmpresaAuthResponse GerarToken()
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
            var content = new { login = environmentsBase.BulllaEmpresaUsername, password = environmentsBase.BulllaEmpresaPassword };
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
            UpdateAccessToken(GerarToken());
            var bytes = _cache.Get(AccessTokenCacheKey);
            if (bytes != null && bytes.Any())
            {
                AccessToken = Encoding.Default.GetString(bytes);
                return;
            }


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
        
        public dynamic ContasDesligadas(string codigoConvenio, int IdUsuario, string dataFiltro)
        {
            var bulllaEmpresaAuthResponse = GerarToken();            

            _logger.LogInformation("contas-desligadas");
            var url = $"{environmentsBase.ASUrlApi}/api/contas/desligadas";

            if (!string.IsNullOrEmpty(dataFiltro))
            {
                url += "?" + dataFiltro;
            }

            _logger.LogInformation($"url: {url}");
            var method = HttpMethodEnum.Get;
            var headers = new Dictionary<string, string>
            {
                { "Authorization", bulllaEmpresaAuthResponse.token },
                { "Canal", environmentsBase.ASChannel },
                { "codigoConvenio", codigoConvenio },
                { "idUsuario", IdUsuario.ToString() },
                { "Content-Type", "application/json" }
            };
            try
            {                
                var request = new Func<HttpResponseDto>(() => _requestHelper.Execute(url, method, null, headers));
                var response = Execute(request, "Método", "Contas Desligadas");

                if (!response.StatusCode.IsSuccess())
                    throw new ServiceUnavailableException("Erro ao realizar entrada ao serviço Bullla. Por favor, tente novamente mais tarde ou entre em contato com o suporte. Erro: B001001.: " + response.Content);

                _logger.LogInformation($"response: {response.Content}");

                dynamic contentJson = JsonSerializer.Deserialize<dynamic>(response.Content);

                return contentJson;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao acessar serviço:" + url, ex);
            }

        }

        public dynamic Faturas(string codigoConvenio, int IdUsuario, int idConta)
        {
            if (idConta == 0)
            {
                throw new ServiceUnavailableException("O IdConta é obrigatório.");
            }

            var bulllaEmpresaAuthResponse = GerarToken();

            _logger.LogInformation("faturas");
            var url = $"{environmentsBase.ASUrlApi}/api/faturas";

            url += "?idConta=" + idConta;

            _logger.LogInformation($"url: {url}");
            var method = HttpMethodEnum.Get;
            var headers = new Dictionary<string, string>
            {
                { "Authorization", bulllaEmpresaAuthResponse.token },
                { "Canal", environmentsBase.ASChannel },
                { "codigoConvenio", codigoConvenio },
                { "idUsuario", IdUsuario.ToString() },
                { "Content-Type", "application/json" }
            };
            try
            {
                var request = new Func<HttpResponseDto>(() => _requestHelper.Execute(url, method, null, headers));
                var response = Execute(request, "Método", "Faturas");

                if (!response.StatusCode.IsSuccess())
                    throw new ServiceUnavailableException("Erro ao realizar entrada ao serviço Bullla. Por favor, tente novamente mais tarde ou entre em contato com o suporte. Erro: B001001.: " + response.Content);

                _logger.LogInformation($"response: {response.Content}");

                dynamic contentJson = JsonSerializer.Deserialize<dynamic>(response.Content);

                return contentJson;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao acessar serviço:" + url, ex);
            }
        }

        public dynamic Contas(string codigoConvenio, int IdUsuario, string dataFiltro)
        {
            var bulllaEmpresaAuthResponse = GerarToken();

            _logger.LogInformation("contas");
            var url = $"{environmentsBase.ASUrlApi}/api/contas";

            if (!string.IsNullOrEmpty(dataFiltro))
            {
                url += "?" + dataFiltro;
            }

            _logger.LogInformation($"url: {url}");
            var method = HttpMethodEnum.Get;
            var headers = new Dictionary<string, string>
            {
                { "Authorization", bulllaEmpresaAuthResponse.token },
                { "Canal", environmentsBase.ASChannel },
                { "codigoConvenio", codigoConvenio },
                { "idUsuario", IdUsuario.ToString() },
                { "Content-Type", "application/json" }
            };
            try
            {
                var request = new Func<HttpResponseDto>(() => _requestHelper.Execute(url, method, null, headers));
                var response = Execute(request, "Método", "Contas lista");

                if (!response.StatusCode.IsSuccess())
                    throw new ServiceUnavailableException("Erro ao realizar entrada ao serviço Bullla. Por favor, tente novamente mais tarde ou entre em contato com o suporte. Erro: B001001.: " + response.Content);

                _logger.LogInformation($"response: {response.Content}");

                dynamic contentJson = JsonSerializer.Deserialize<dynamic>(response.Content);

                return contentJson;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao acessar serviço:" + url, ex);
            }
        }

        public dynamic AbatimentoDivida(string codigoConvenio, int IdUsuario, AbatimentoDivida data)
        {
            if (data == null || data.idConta == 0)
            {
                throw new ServiceUnavailableException("O IdConta é obrigatório.");
            }

            if (string.IsNullOrEmpty(data.email))
            {
                throw new ServiceUnavailableException("O Email é obrigatório.");
            }

            if (string.IsNullOrEmpty(data.telefone))
            {
                throw new ServiceUnavailableException("O Telefone é obrigatório.");
            }

            var bulllaEmpresaAuthResponse = GerarToken();

            _logger.LogInformation("abatimento-divida");
            var url = $"{environmentsBase.ASUrlApi}/api/abatimento-divida";

            _logger.LogInformation($"url: {url}");
            var method = HttpMethodEnum.Post;
            var headers = new Dictionary<string, string>
            {
                { "Authorization", bulllaEmpresaAuthResponse.token },
                { "Canal", environmentsBase.ASChannel },
                { "codigoConvenio", codigoConvenio },
                { "idUsuario", IdUsuario.ToString() },
                { "Content-Type", "application/json" }
            };
            try
            {
                var abatimentoDivida = new { idConta = data.idConta, email = data.email, telefone = data.telefone, valorAbatimento = data.valorAbatimento, linkArquivo = data.linkArquivo };
                _logger.LogInformation($"text: {abatimentoDivida}");

                var request = new Func<HttpResponseDto>(() => _requestHelper.Execute(url, method, null, headers, abatimentoDivida));
                var response = Execute(request, "Método", "AbatimentoDivida");

                _logger.LogInformation($"response: {response.Content}");

                dynamic contentJson = JsonSerializer.Deserialize<dynamic>(response.Content);

                return contentJson;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao acessar serviço:" + url, ex);
            }
        }

        public dynamic FaturasResidual(string codigoConvenio, int IdUsuario, int idConta)
        {
            if (idConta == 0)
            {
                throw new ServiceUnavailableException("O IdConta é obrigatório.");
            }

            var bulllaEmpresaAuthResponse = GerarToken();

            _logger.LogInformation("Faturas Residual");
            var url = $"{environmentsBase.ASUrlApi}/api/faturas/residual";

            url += "?idConta=" + idConta;

            _logger.LogInformation($"url: {url}");
            var method = HttpMethodEnum.Get;
            var headers = new Dictionary<string, string>
            {
                { "Authorization", bulllaEmpresaAuthResponse.token },
                { "Canal", environmentsBase.ASChannel },
                { "codigoConvenio", codigoConvenio },
                { "idUsuario", IdUsuario.ToString() },
                { "Content-Type", "application/json" }
            };
            try
            {
                var request = new Func<HttpResponseDto>(() => _requestHelper.Execute(url, method, null, headers));
                var response = Execute(request, "Método", "Faturas Residual");

                if (!response.StatusCode.IsSuccess())
                    throw new ServiceUnavailableException("Erro ao realizar entrada ao serviço Bullla. Por favor, tente novamente mais tarde ou entre em contato com o suporte. Erro: B001001.: " + response.Content);

                _logger.LogInformation($"response: {response.Content}");

                dynamic contentJson = JsonSerializer.Deserialize<dynamic>(response.Content);

                return contentJson;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao acessar serviço:" + url, ex);
            }
        }

        public dynamic Cargas(string codigoConvenio, int IdUsuario, int idConta)
        {
            if (idConta == 0)
            {
                throw new ServiceUnavailableException("O IdConta é obrigatório.");
            }

            var bulllaEmpresaAuthResponse = GerarToken();

            _logger.LogInformation("cargas");
            var url = $"{environmentsBase.ASUrlApi}/api/cargas";

            url += "?idConta=" + idConta;

            _logger.LogInformation($"url: {url}");
            var method = HttpMethodEnum.Get;
            var headers = new Dictionary<string, string>
            {
                { "Authorization", bulllaEmpresaAuthResponse.token },
                { "Canal", environmentsBase.ASChannel },
                { "codigoConvenio", codigoConvenio },
                { "idUsuario", IdUsuario.ToString() },
                { "Content-Type", "application/json" }
            };
            try
            {
                var request = new Func<HttpResponseDto>(() => _requestHelper.Execute(url, method, null, headers));
                var response = Execute(request, "Método", "Cargas");

                if (!response.StatusCode.IsSuccess())
                    throw new ServiceUnavailableException("Erro ao realizar entrada ao serviço Bullla. Por favor, tente novamente mais tarde ou entre em contato com o suporte. Erro: B001001.: " + response.Content);

                _logger.LogInformation($"response: {response.Content}");

                dynamic contentJson = JsonSerializer.Deserialize<dynamic>(response.Content);

                return contentJson;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao acessar serviço:" + url, ex);
            }
        }
        public dynamic Cartoes(string codigoConvenio, int IdUsuario, string dataFiltro)
        {
            var bulllaEmpresaAuthResponse = GerarToken();

            _logger.LogInformation("contas");
            var url = $"{environmentsBase.ASUrlApi}/api/cartoes";

            if (!string.IsNullOrEmpty(dataFiltro))
            {
                url += "?" + dataFiltro;
            }

            _logger.LogInformation($"url: {url}");
            var method = HttpMethodEnum.Get;
            var headers = new Dictionary<string, string>
            {
                { "Authorization", bulllaEmpresaAuthResponse.token },
                { "Canal", environmentsBase.ASChannel },
                { "codigoConvenio", codigoConvenio },
                { "idUsuario", IdUsuario.ToString() },
                { "Content-Type", "application/json" }
            };
            try
            {
                var request = new Func<HttpResponseDto>(() => _requestHelper.Execute(url, method, null, headers));
                var response = Execute(request, "Método", "Cartoes lista");

                if (!response.StatusCode.IsSuccess())
                    throw new ServiceUnavailableException("Erro ao realizar entrada ao serviço Bullla. Por favor, tente novamente mais tarde ou entre em contato com o suporte. Erro: B001001.: " + response.Content);

                _logger.LogInformation($"response: {response.Content}");

                dynamic contentJson = JsonSerializer.Deserialize<dynamic>(response.Content);

                return contentJson;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao acessar serviço:" + url, ex);
            }
        }

        public dynamic ReportDismissalDiscount(string userMail, string dataFiltro)
        {
            var bulllaEmpresaAuthResponse = GerarToken();            

            _logger.LogInformation("relatorio/descontorescisao");

            var url = $"{environmentsBase.ASUrlApi}/api/relatorio/descontorescisao";

            if (!string.IsNullOrEmpty(dataFiltro))
            {
                url += "?" + dataFiltro;
            }

            _logger.LogInformation($"url: {url}");
            var method = HttpMethodEnum.Get;
            var headers = new Dictionary<string, string>
            {
                { "Authorization", bulllaEmpresaAuthResponse.token },
                { "Canal", environmentsBase.ASChannel },
                { "idUsuario", userMail },
                { "Content-Type", "application/json" }
            };
            try
            {                
                var request = new Func<HttpResponseDto>(() => _requestHelper.Execute(url, method, null, headers));
                var response = Execute(request, "Método", "Desconto Rescisao");

                if (!response.StatusCode.IsSuccess())
                    throw new ServiceUnavailableException("Erro ao realizar entrada ao serviço Bullla. Por favor, tente novamente mais tarde ou entre em contato com o suporte. Erro: " + response.Content);

                _logger.LogInformation($"response: {response.Content}");

                dynamic contentJson = JsonSerializer.Deserialize<dynamic>(response.Content);

                return contentJson;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao acessar serviço:" + url, ex);
            }

        }
    }
}