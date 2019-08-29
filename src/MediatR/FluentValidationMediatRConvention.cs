using Rocket.Surgery.Extensions.FluentValidation;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;
using Rocket.Surgery.Extensions.FluentValidation.MediatR;
using MediatR;

[assembly: Convention(typeof(FluentValidationMediatRConvention))]

namespace Rocket.Surgery.Extensions.FluentValidation.MediatR
{
    /// <summary>
    /// ValidationConvention.
    /// Implements the <see cref="Rocket.Surgery.Extensions.DependencyInjection.IServiceConvention" />
    /// </summary>
    /// <seealso cref="Rocket.Surgery.Extensions.DependencyInjection.IServiceConvention" />
    /// <seealso cref="IServiceConvention" />
    public class FluentValidationMediatRConvention : IServiceConvention
    {
        /// <summary>
        /// Registers the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Register(IServiceConventionContext context)
        {
            context.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(FluentValidationMediatRPipelineBehavior<,>));
        }
    }
}
