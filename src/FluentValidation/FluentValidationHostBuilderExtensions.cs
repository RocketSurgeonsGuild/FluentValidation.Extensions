using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.FluentValidation;

// ReSharper disable once CheckNamespace
namespace Rocket.Surgery.Conventions
{
    /// <summary>
    ///  FluentValidationHostBuilderExtensions.
    /// </summary>
    public static class FluentValidationHostBuilderExtensions
    {
        /// <summary>
        /// Adds fluent validation.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>IConventionHostBuilder.</returns>
        public static IConventionHostBuilder WithFluentValidation(this IConventionHostBuilder builder)
        {
            builder.Scanner.PrependConvention<FluentValidationConvention>();
            return builder;
        }
    }
}
