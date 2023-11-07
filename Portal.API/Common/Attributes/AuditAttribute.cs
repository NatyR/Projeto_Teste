using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using Portal.API.Entities;
using Portal.API.Interfaces.Repositories;
using Portal.API.Interfaces.Services;
using System.Net;

namespace Portal.API.Common.Attributes
{
    public class AuditAttribute : ActionFilterAttribute
    {
        private readonly ILogger<AuditAttribute> _logger;
        private readonly IAcessoRepository _acessoRepository;
        private readonly IHttpServicePortal _httpService;
        public AuditAttribute(ILogger<AuditAttribute> logger,
            IAcessoRepository acessoRepository,
            IHttpServicePortal httpService)

        {
            _logger = logger;
            _acessoRepository = acessoRepository;
            _httpService = httpService;
        }


        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //_logger.LogInformation("OnActionExecuting");
            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            //_logger.LogInformation("OnActionExecuted");
            //_logger.LogInformation($"host: {context.HttpContext.Request.Host}");
            //_logger.LogInformation($"pathAndQuery: {context.HttpContext.Request.GetEncodedPathAndQuery()}");
            try
            {
                Int64.TryParse(context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value, out long userId);
                if (userId > 0)
                {
                    string ip = _httpService.GetRequestIP();
                    var url = context.HttpContext.Request.GetEncodedPathAndQuery();
                    var method = context.HttpContext.Request.Method;
                    var postData = GetRequestBody(context);

                    Acesso acesso = new Acesso(userId, ip, url, method, postData);
                    Task.Run(async () =>
                    {
                        await _acessoRepository.Add(acesso);
                    }).Wait();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuditAttribute.OnActionExecuted");
            }



            base.OnActionExecuted(context);
        }
        private string GetRequestBody(ActionExecutedContext context)
        {
            context.HttpContext.Request.Body.Position = 0;
            string rawRequestBody = "";
            Task.Run(async () =>
            {
                rawRequestBody = await new StreamReader(context.HttpContext.Request.Body).ReadToEndAsync();
            }).Wait();
            context.HttpContext.Request.Body.Position = 0;
            return rawRequestBody;
        }
    }
}
