using System;
using JetBrains.Annotations;
using Rocket.Surgery.Extensions.FluentValidation;

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
        public static IConventionHostBuilder WithFluentValidation([NotNull] this IConventionHostBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Scanner.PrependConvention<FluentValidationConvention>();
            return builder;
        }
    }
}