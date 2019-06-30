using Rocket.Surgery.Extensions.FluentValidation;

// ReSharper disable once CheckNamespace
namespace FluentValidation
{
    /// <summary>
    /// Class FluentValidationPolymorphicPropertyValidatorExtensions.
    /// </summary>
    public static class FluentValidationPolymorphicPropertyValidatorExtensions
    {
        /// <summary>
        /// Uses the polymorphic validator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty">The type of the t property.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="validatorFactory">The validator factory.</param>
        /// <returns>IRuleBuilderOptions&lt;T, TProperty&gt;.</returns>
        public static IRuleBuilderOptions<T, TProperty> UsePolymorphicValidator<T, TProperty>(
            this IRuleBuilder<T, TProperty> builder,
            IValidatorFactory validatorFactory)
        {
            return builder.SetValidator(new PolymorphicPropertyValidator<TProperty>(validatorFactory));
        }
    }
}
