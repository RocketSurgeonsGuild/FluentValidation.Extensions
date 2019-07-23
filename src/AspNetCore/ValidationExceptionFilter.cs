
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.FluentValidation.AspNetCore;

[assembly: Convention(typeof(AspNetCoreFluentValidationConvention))]

namespace Rocket.Surgery.Extensions.FluentValidation.AspNetCore
{
    /// <summary>
    /// A validation exception filter
    /// </summary>
    public class ValidationExceptionFilter : IExceptionFilter, IAsyncExceptionFilter
    {
        /// <inheritdoc />
        public void OnException(ExceptionContext context)
        {
            if (!(context.Exception is ValidationException validationException)) return;

            context.ExceptionHandled = true;
            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState));
        }

        /// <inheritdoc />
        public Task OnExceptionAsync(ExceptionContext context)
        {
            OnException(context);
            return Task.CompletedTask;
        }
    }
}
