using System;
using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
        private readonly FluentValidationMvcConfiguration _validationMvcConfiguration;
        private readonly ValidatorConfiguration _validatorConfiguration;

        /// <summary>
        /// THe validation settings
        /// </summary>
        /// <param name="validatorConfiguration"></param>
        /// <param name="validationMvcConfiguration"></param>
        public AspNetCoreFluentValidationConvention(
            [CanBeNull] ValidatorConfiguration? validatorConfiguration = null,
            [CanBeNull] FluentValidationMvcConfiguration? validationMvcConfiguration = null)
        {
            _validatorConfiguration = validatorConfiguration ??= new ValidatorConfiguration();
            _validationMvcConfiguration = validationMvcConfiguration ?? new FluentValidationMvcConfiguration(validatorConfiguration);
        }

        /// <summary>
        /// Registers the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Register([NotNull] IServiceConventionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Services
               .AddFluentValidationExtensions(_validatorConfiguration, _validationMvcConfiguration);
        }
    }
}