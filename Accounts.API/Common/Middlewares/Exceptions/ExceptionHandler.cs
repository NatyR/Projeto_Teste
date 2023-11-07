using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Accounts.API.Common.Middlewares.Exceptions
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;

        public ExceptionHandler(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                await HandleException(context, e);
            }
        }

        /// <summary>
        /// Método responsável por disparar exception na requisição
        /// </summary>
        /// <param name="context">Contexto da requisição</param>
        /// <param name="exception">Exceção ocorrida</param>
        /// <returns>Retorna algum status code para a requisição mal sucedida</returns>
        private static Task HandleException(HttpContext context, Exception exception)
        {
            var response = new InternalServerErrorException(GetGenericStatusCodeMessage(exception));
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = GetStatusCode(exception);
            return context.Response.WriteAsync(JsonConvert.SerializeObject(new { response.Message },
                new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
                    Formatting = Formatting.Indented
                }));
        }

        private static int GetStatusCode(Exception ex)
        {
            switch (ex)
            {
                case BadRequestException _:
                    return (int)HttpStatusCode.BadRequest;
                case NotFoundException _:
                    return (int)HttpStatusCode.NotFound;
                case UnauthorizedException _:
                    return (int)HttpStatusCode.Unauthorized;
                case ForbiddenException _:
                    return (int)HttpStatusCode.Forbidden;
                case ServiceUnavailableException _:
                    return (int)HttpStatusCode.ServiceUnavailable;
                case InternalServerErrorException _:
                    return (int)HttpStatusCode.InternalServerError;
                case StatusCodeException _:
                    return GetGenericStatusCode(ex.Message);
                default:
                    return (int)HttpStatusCode.InternalServerError;
            }
        }

        private static int GetGenericStatusCode(string exMessage)
        {
            var croped = CropStatusCodeFromMessage(exMessage).Trim();
            croped = croped.Replace("_$!_", string.Empty);

            return int.Parse(croped);
        }

        private static string GetGenericStatusCodeMessage(Exception exception)
        {
            if (exception.GetBaseException().GetType() != typeof(StatusCodeException))
                return exception.Message;

            var newMessage = CropStatusCodeFromMessage(exception.Message);
            return exception.Message.Replace(newMessage, string.Empty).Trim();

        }

        private static string CropStatusCodeFromMessage(string message)
        {
            return message.Substring(0, 12);
        }
    }
}
