using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.AspNetCore.FluentValidation;
using Rocket.Surgery.AspNetCore.FluentValidation.NewtonsoftJson;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.DependencyInjection;

[assembly: Convention(typeof(AspNetCoreFluentValidationNewtonsoftJsonConvention))]

namespace Rocket.Surgery.AspNetCore.FluentValidation.NewtonsoftJson
{
    /// <summary>
    /// AspNetCoreFluentValidationConvention.
    /// Implements the <see cref="Rocket.Surgery.Extensions.DependencyInjection.IServiceConvention" />
    /// </summary>
    /// <seealso cref="Rocket.Surgery.Extensions.DependencyInjection.IServiceConvention" />
    /// <seealso cref="IServiceConvention" />
    public class AspNetCoreFluentValidationNewtonsoftJsonConvention : IServiceConvention
    {
        /// <summary>
        /// Registers the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Register(IServiceConventionContext context)
        {
            context.Services
                .AddMvcCore()
                .AddNewtonsoftJson(options => options.SerializerSettings.Converters.Add(new ValidationProblemDetailsNewtonsoftJsonConverter()));
        }
    }
}
