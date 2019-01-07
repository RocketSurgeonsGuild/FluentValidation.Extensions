using System.Linq;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rocket.Surgery.Extensions.FluentValidation;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Extensions.DependencyInjection;

namespace Rocket.Surgery.Extensions.FluentValidation
{
    public static class ValidationServicesExtensions
    {
        public static T WithFluentValidation<T>(this T builder)
            where T : IServiceConventionContext
        {
            if (builder.Services.All(z => z.ServiceType != typeof(IValidatorFactory)))
            {
                foreach (var item in new AssemblyScanner(
                    builder
                        .AssemblyCandidateFinder
                        .GetCandidateAssemblies(nameof(FluentValidation))
                        .SelectMany(z => z.DefinedTypes)
                        .Select(x => x.AsType())
                ))
                {
                    builder.Services.TryAddEnumerable(
                        ServiceDescriptor.Transient(item.InterfaceType, item.ValidatorType)
                    );
                }

                builder.Services.TryAddSingleton<IValidatorFactory, ValidatorFactory>();
            }
            return builder;
        }
    }
}
