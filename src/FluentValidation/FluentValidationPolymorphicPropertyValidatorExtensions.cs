using JetBrains.Annotations;
using Rocket.Surgery.Extensions.FluentValidation;

// ReSharper disable once CheckNamespace
namespace FluentValidation
{
    /// <summary>
    ///  FluentValidationPolymorphicPropertyValidatorExtensions.
    /// </summary>
    [PublicAPI]
    public static class FluentValidationPolymorphicPropertyValidatorExtensions
    {
        /// <summary>
        /// Uses the polymorphic validator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty">The type of the t property.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="validatorFactory">The validator factory.</param>
        /// <returns>IRuleBuilderOptions{T, TProperty}.</returns>
        public static IRuleBuilderOptions<T, TProperty> UsePolymorphicValidator<T, TProperty>(
            this IRuleBuilder<T, TProperty> builder,
            IValidatorFactory validatorFactory)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            return builder.SetValidator(new PolymorphicPropertyValidator<TProperty>(validatorFactory));
        }
    }
}
