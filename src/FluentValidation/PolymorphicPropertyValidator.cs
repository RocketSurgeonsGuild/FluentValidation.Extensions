using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using FluentValidation.Validators;

namespace Rocket.Surgery.Extensions.FluentValidation
{
    /// <summary>
    ///  PolymorphicPropertyValidator.
    /// Implements the <see cref="NoopPropertyValidator" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="NoopPropertyValidator" />
    public class PolymorphicPropertyValidator<T> : NoopPropertyValidator
    {
        private readonly IValidatorFactory _validatorFactory;
        private readonly ConcurrentDictionary<Type, IValidator> _derivedValidators = new ConcurrentDictionary<Type, IValidator>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PolymorphicPropertyValidator{T}"/> class.
        /// </summary>
        /// <param name="validatorFactory">The validator factory.</param>
        internal PolymorphicPropertyValidator(IValidatorFactory validatorFactory)
        {
            _validatorFactory = validatorFactory;
        }

        /// <summary>
        /// Validates the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>IEnumerable{ValidationFailure}.</returns>
        public override IEnumerable<ValidationFailure> Validate(PropertyValidatorContext context)
        {
            // bail out if the property is null
            if (context.PropertyValue == null) return Enumerable.Empty<ValidationFailure>();
            if (!(context.PropertyValue is T value)) return Enumerable.Empty<ValidationFailure>();

            if (!_derivedValidators.TryGetValue(value.GetType(), out var validator))
            {
                validator = _derivedValidators[value.GetType()] = _validatorFactory.GetValidator(value.GetType());
            }

            if (context.ParentContext.IsChildCollectionContext)
            {
                return validator.Validate(context.ParentContext.CloneForChildValidator(value)).Errors;
            }

            var validationContext = new ValidationContext<T>(value, PropertyChain.FromExpression(context.Rule.Expression), context.ParentContext.Selector);

            return validator.Validate(validationContext).Errors;
        }

        /// <summary>
        /// validate as an asynchronous operation.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>Task{IEnumerable{ValidationFailure}}.</returns>
        public override async Task<IEnumerable<ValidationFailure>> ValidateAsync(PropertyValidatorContext context, CancellationToken cancellation)
        {
            // bail out if the property is null
            if (context.PropertyValue == null) return Enumerable.Empty<ValidationFailure>();
            if (!(context.PropertyValue is T value)) return Enumerable.Empty<ValidationFailure>();

            if (!_derivedValidators.TryGetValue(value.GetType(), out var validator))
            {
                validator = _derivedValidators[value.GetType()] = _validatorFactory.GetValidator(value.GetType());
            }

            if (context.ParentContext.IsChildCollectionContext)
            {
                return (await validator.ValidateAsync(
                    context.ParentContext.CloneForChildValidator(value)
                )).Errors;
            }

            var validationContext = new ValidationContext<T>(value, PropertyChain.FromExpression(context.Rule.Expression), context.ParentContext.Selector);

            return (await validator.ValidateAsync(validationContext)).Errors;
        }
    }
}
