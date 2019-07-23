using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Extensions.FluentValidation.AspNetCore;

[assembly: Convention(typeof(AspNetCoreFluentValidationConvention))]

namespace Rocket.Surgery.Extensions.FluentValidation.AspNetCore
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
            context.Services.Configure<MvcOptions>(options => options.Filters.Add<ValidationExceptionFilter>());
            context.Services.AddMvcCore().AddFluentValidation();
        }
    }
}
