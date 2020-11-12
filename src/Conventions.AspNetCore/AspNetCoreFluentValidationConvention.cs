using System;
using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.AspNetCore.FluentValidation;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.AspNetCore.FluentValidation;
using Rocket.Surgery.Conventions.DependencyInjection;
using Rocket.Surgery.Extensions.FluentValidation;

[assembly: Convention(typeof(AspNetCoreFluentValidationConvention))]

namespace Rocket.Surgery.Conventions.AspNetCore.FluentValidation
{
    /// <summary>
    /// AspNetCoreFluentValidationConvention.
    /// Implements the <see cref="IServiceConvention" />
    /// </summary>
    /// <seealso cref="IServiceConvention" />
    [PublicAPI]
    public class AspNetCoreFluentValidationConvention : IServiceConvention
    {
        private readonly ValidatorConfiguration _configuration;
        private readonly FluentValidationMvcConfiguration _mvcConfiguration;

        /// <summary>
        /// THe validation settings
        /// </summary>
        /// <param name="validatorConfiguration"></param>
        /// <param name="validationMvcConfiguration"></param>
        public AspNetCoreFluentValidationConvention([CanBeNull] ValidatorConfiguration? validatorConfiguration = null, FluentValidationMvcConfiguration? validationMvcConfiguration = null)
        {
            _configuration ??= validatorConfiguration ?? new ValidatorConfiguration();
            _mvcConfiguration = validationMvcConfiguration ?? new FluentValidationMvcConfiguration(_configuration);
        }

        /// <summary>
        /// Registers the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="services">The services.</param>
        public void Register(IConventionContext context, IConfiguration configuration, IServiceCollection services)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            services
               .AddFluentValidationExtensions(_configuration, _mvcConfiguration);
        }
    }
}