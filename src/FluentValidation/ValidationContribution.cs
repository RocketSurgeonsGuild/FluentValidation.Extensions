using Rocket.Surgery.Core.Validation;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.DependencyInjection;

[assembly: Convention(typeof(ValidationConvention))]

namespace Rocket.Surgery.Core.Validation
{
    /// <summary>
    /// Class ValidationConvention.
    /// </summary>
    /// <seealso cref="IServiceConvention" />
    /// TODO Edit XML Comment Template for ValidationConvention
    public class ValidationConvention : IServiceConvention
    {
        /// <summary>
        /// Registers the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// TODO Edit XML Comment Template for Register
        public void Register(IServiceConventionContext context)
        {
            context.WithFluentValidation();
        }
    }
}
