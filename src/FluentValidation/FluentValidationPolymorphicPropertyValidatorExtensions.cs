using Rocket.Surgery.Extensions.FluentValidation;

// ReSharper disable once CheckNamespace
namespace FluentValidation
{
    public static class FluentValidationPolymorphicPropertyValidatorExtensions
    {
        public static IRuleBuilderOptions<T, TProperty> UsePolymorphicValidator<T, TProperty>(
            this IRuleBuilder<T, TProperty> builder,
            IValidatorFactory validatorFactory)
        {
            return builder.SetValidator(new PolymorphicPropertyValidator<TProperty>(validatorFactory));
        }
    }
}
