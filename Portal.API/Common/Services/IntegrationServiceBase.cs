using Newtonsoft.Json;
using Serilog;
using System;
using System.Threading.Tasks;
using Portal.API.Common.Dto.Request;
using Portal.API.Common.Entities.Integrations;
using Portal.API.Common.Extensions.Request;
using Portal.API.Common.Middlewares.Exceptions;
using Portal.API.Common.Repositories.Interfaces;
namespace Portal.API.Common.Services
{
    public class IntegrationServiceBase
    {
        private readonly IIntegrationRepositoryBase _repository;
        private readonly string _name;

        public IntegrationServiceBase(IIntegrationRepositoryBase repository, string name)
        {
            _repository = repository;
            _name = name;
        }

        public virtual HttpResponseDto Execute(Func<HttpResponseDto> func, string reference = null, string referenceType = null)
        {
            try
            {
                var log = InitializeLog(func.Target, reference, referenceType);
                var response = func.Invoke();
                FillLog(ref log, response);
                AddLog(log);

                return response;
            }
            catch (Exception e)
            {
                IntegrationLog log;

                if (e.InnerException is TaskCanceledException)
                {
                    log = InitializeLog(TryGetRequestBody(func), reference, referenceType, "Requisição demorou mais que o tempo mínimo permitido.");
                    AddLog(log);

                    try
                    {
                        var jsonppp = JsonConvert.SerializeObject(func.Target);
                        throw new ServiceUnavailableException("Não foi possível processar sua solicitação no momento. Por favor, tente novamente mais tarde." + jsonppp);
                    }
                    catch (Exception ex)
                    {
                        throw new ServiceUnavailableException("Não foi possível processar sua solicitação no momento. Por favor, tente novamente mais tarde. não foi possivel converter json 2");
                    }
                }

                log = InitializeLog(TryGetRequestBody(func), reference, referenceType, e.Message);
                AddLog(log);

                throw;
            }
        }

        public virtual HttpResponseDto ExecuteWithInterceptor(Func<HttpResponseDto> request, string reference = null, string referenceType = null)
        {
            var response = Execute(request, reference, referenceType);
            if (!response.StatusCode.IsAuthenticationFailed())
                return response;

            OnUpdateAccessToken();
            return Execute(request, reference, referenceType);
        }

        protected virtual void OnUpdateAccessToken()
        {
        }

        public virtual void AddLog(IntegrationLog log)
        {
            Task.Run(() => _repository.Add(log));
        }

        public virtual void FillLog(ref IntegrationLog log, HttpResponseDto response)
        {
            log.Response = JsonConvert.SerializeObject(response);
            log.RepliedAt = DateTime.Now;
            log.TimeSpent = Math.Round((DateTime.Now - log.RequestedAt).TotalSeconds, 2);
            log.StatusCode = response.StatusCode;
        }

        public virtual IntegrationLog InitializeLog(object request, string reference, string referenceType, string exception = null)
        {
            return new IntegrationLog
            {
                Name = _name,
                Reference = reference,
                ReferenceType = referenceType,
                RequestedAt = DateTime.Now,
                Exception = exception,
                Request = JsonConvert.SerializeObject(request)
            };
        }

        public virtual string TryGetRequestBody(Func<HttpResponseDto> func)
        {
            try
            {
                return JsonConvert.SerializeObject(func.Target);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public virtual T TryParseResponse<T>(HttpResponseDto response, bool checkStatusCodeSuccessfully = true) where T : class
        {
            if (checkStatusCodeSuccessfully && !response.StatusCode.IsSuccess())
            {
                Log.Logger.Fatal($"{_name} - Erro ao receber resposta. Erro: {response.Content}");
                throw new ServiceUnavailableException();
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(response.Content);
            }
            catch (Exception e)
            {
                Log.Logger.Fatal(e, $"{_name} - Erro ao desserializar objeto -> {response.Content} para o tipo -> {typeof(T)}");
                throw new ServiceUnavailableException();
            }
        }

        public virtual bool TryParseSuccessResponse<T>(HttpResponseDto response, out T result) where T : class
        {
            result = null;
            if (!response.StatusCode.IsSuccess())
            {
                Log.Logger.Fatal($"{_name} - Erro ao receber resposta. Erro: {response.Content}");
                return false;
            }

            try
            {
                result = JsonConvert.DeserializeObject<T>(response.Content);
                return true;
            }
            catch (Exception e)
            {
                Log.Logger.Fatal(e, $"{_name} - Erro ao desserializar objeto -> {response.Content} para o tipo -> {typeof(T)}");
                return false;
            }
        }
    }

}
