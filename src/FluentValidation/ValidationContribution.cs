using Rocket.Surgery.Extensions.FluentValidation;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.DependencyInjection;

[assembly: Convention(typeof(ValidationConvention))]

namespace Rocket.Surgery.Extensions.FluentValidation
{
    /// <summary>
    /// ValidationConvention.
    /// Implements the <see cref="Rocket.Surgery.Extensions.DependencyInjection.IServiceConvention" />
    /// </summary>
    /// <seealso cref="Rocket.Surgery.Extensions.DependencyInjection.IServiceConvention" />
    /// <seealso cref="IServiceConvention" />
    public class ValidationConvention : IServiceConvention
    {
        /// <summary>
        /// Registers the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Register(IServiceConventionContext context)
        {
            context.WithFluentValidation();
        }
    }
}
