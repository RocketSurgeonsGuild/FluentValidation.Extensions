using Rocket.Surgery.Extensions.FluentValidation;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Extensions.DependencyInjection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;

[assembly: Convention(typeof(FluentValidationConvention))]

namespace Rocket.Surgery.Extensions.FluentValidation
{
    /// <summary>
    /// ValidationConvention.
    /// Implements the <see cref="Rocket.Surgery.Extensions.DependencyInjection.IServiceConvention" />
    /// </summary>
    /// <seealso cref="Rocket.Surgery.Extensions.DependencyInjection.IServiceConvention" />
    /// <seealso cref="IServiceConvention" />
    public class FluentValidationConvention : IServiceConvention
    {
        /// <summary>
        /// Registers the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Register(IServiceConventionContext context)
        {
            foreach (var item in new AssemblyScanner(
                context
                    .AssemblyCandidateFinder
                    .GetCandidateAssemblies(nameof(FluentValidation))
                    .SelectMany(z => z.DefinedTypes)
                    .Select(x => x.AsType())
            ))
            {
                context.Services.TryAddEnumerable(
                    ServiceDescriptor.Transient(item.InterfaceType, item.ValidatorType)
                );
            }

            context.Services.TryAddSingleton<IValidatorFactory, ValidatorFactory>();
        }
    }
}
