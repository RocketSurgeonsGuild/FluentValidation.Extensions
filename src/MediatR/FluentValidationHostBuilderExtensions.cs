using System;
using JetBrains.Annotations;
using Rocket.Surgery.Extensions.FluentValidation.MediatR;

// ReSharper disable once CheckNamespace
namespace Rocket.Surgery.Conventions
{
    /// <summary>
    /// FluentValidationHostBuilderExtensions.
    /// </summary>
    [PublicAPI]
    public static class FluentValidationHostBuilderExtensions
    {
        /// <summary>
        /// Adds fluent validation.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>IConventionHostBuilder.</returns>
        public static IConventionHostBuilder WithFluentValidationMediatR(this IConventionHostBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Scanner.PrependConvention<FluentValidationMediatRConvention>();
            return builder;
        }
    }
}