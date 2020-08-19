using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace Rocket.Surgery.AspNetCore.FluentValidation
{
    [System.Obsolete]
    internal class ValidatorInterceptor : IActionContextValidatorInterceptor
    {
        public IValidationContext BeforeMvcValidation(ActionContext actionContext, IValidationContext validationContext) => validationContext;

        public ValidationResult AfterMvcValidation(ActionContext actionContext, IValidationContext validationContext, ValidationResult result)
        {
            actionContext.HttpContext.Items[typeof(ValidationResult)] = result;
            return result;
        }
    }
}