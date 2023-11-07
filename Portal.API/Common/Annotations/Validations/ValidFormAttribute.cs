using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Portal.API.Common.Annotations.Validations
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

        private ErrorResponsePortal GetErrors(ModelStateDictionary modelState)
        {
            var erro = new ErrorResponsePortal
            {
                Errors = modelState.Where(w => w.Value.Errors.Any()).Select(s =>
                    new ItemErrorResponsePortal(s.Key)
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

    public class ErrorResponsePortal
    {
        public ErrorResponsePortal()
        {
            Message = "O modelo fornecido é inválido.";
        }

        public ErrorResponsePortal(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
        public List<ItemErrorResponsePortal> Errors { get; set; }
    }

    public class ItemErrorResponsePortal
    {
        public ItemErrorResponsePortal()
        {
        }

        public ItemErrorResponsePortal(string field)
        {
            Field = field;
        }

        public ItemErrorResponsePortal(string field, List<string> messages)
        {
            Field = field;
            Messages = messages;
        }

        public string Field { get; set; }
        public List<string> Messages { get; set; }
    }
}
