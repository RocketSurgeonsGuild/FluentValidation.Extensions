using System.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rocket.Surgery.Extensions.FluentValidation;
using Rocket.Surgery.Extensions.FluentValidation.MediatR;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Extensions.DependencyInjection;

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
            var serviceConfig = context.GetOrAdd(() => new MediatRServiceConfiguration());
            context.Services.Add(new ServiceDescriptor(typeof(IPipelineBehavior<,>), typeof(FluentValidationMediatRPipelineBehavior<,>), serviceConfig.Lifetime));
        }
    }
}
