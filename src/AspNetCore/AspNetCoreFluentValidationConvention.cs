using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.AspNetCore.FluentValidation;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.DependencyInjection;

[assembly: Convention(typeof(AspNetCoreFluentValidationConvention))]

namespace Rocket.Surgery.AspNetCore.FluentValidation
{

    /// <summary>
    /// AspNetCoreFluentValidationConvention.
    /// Implements the <see cref="Rocket.Surgery.Extensions.DependencyInjection.IServiceConvention" />
    /// </summary>
    /// <seealso cref="Rocket.Surgery.Extensions.DependencyInjection.IServiceConvention" />
    /// <seealso cref="IServiceConvention" />
    public class AspNetCoreFluentValidationConvention : IServiceConvention
    {
        /// <summary>
        /// Registers the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Register(IServiceConventionContext context)
        {
            context.Services.AddMvcCore().AddFluentValidation();
            context.Services.AddSingleton<IValidatorInterceptor, ValidatorInterceptor>();
            context.Services.AddSingleton<ProblemDetailsFactory, FluentValidationProblemDetailsFactory>();
        }
    }
}
