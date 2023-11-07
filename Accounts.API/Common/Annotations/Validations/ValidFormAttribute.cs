using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Accounts.API.Common.Annotations.Validations
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ValidFormAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(GetErrors(context.ModelState));
                return;
            }

            base.OnActionExecuting(context);
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(GetErrors(context.ModelState));
                return;
            }

            base.OnResultExecuting(context);
        }

        private ErrorResponseAccounts GetErrors(ModelStateDictionary modelState)
        {
            var erro = new ErrorResponseAccounts
            {
                Errors = modelState.Where(w => w.Value.Errors.Any()).Select(s =>
                    new ItemErrorResponseAccounts(s.Key)
                    {
                        Messages = s.Value.Errors.Select(so => !string.IsNullOrEmpty(so.ErrorMessage) ? so.ErrorMessage : so.Exception.Message).ToList()
                    }).ToList()
            };

            var first = erro.Errors.FirstOrDefault();
            if (first != null && first.Messages.FirstOrDefault() != null)
                erro.Message = first.Messages.FirstOrDefault();

            return erro;
        }
    }

    public class ErrorResponseAccounts
    {
        public ErrorResponseAccounts()
        {
            Message = "O modelo fornecido é inválido.";
        }

        public ErrorResponseAccounts(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
        public List<ItemErrorResponseAccounts> Errors { get; set; }
    }

    public class ItemErrorResponseAccounts
    {
        public ItemErrorResponseAccounts()
        {
        }

        public ItemErrorResponseAccounts(string field)
        {
            Field = field;
        }

        public ItemErrorResponseAccounts(string field, List<string> messages)
        {
            Field = field;
            Messages = messages;
        }

        public string Field { get; set; }
        public List<string> Messages { get; set; }
    }
}
