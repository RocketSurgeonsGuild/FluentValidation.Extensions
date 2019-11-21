using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.AspNetCore.FluentValidation;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.DependencyInjection;

[assembly: Convention(typeof(AspNetCoreFluentValidationConvention))]

namespace Rocket.Surgery.AspNetCore.FluentValidation
{
    /// <summary>
    /// AspNetCoreFluentValidationConvention.
    /// Implements the <see cref="Rocket.Surgery.Extensions.DependencyInjection.IServiceConvention" />
    /// </summary>
    /// <seealso cref="Rocket.Surgery.Extensions.DependencyInjection.IServiceConvention" />
    /// <seealso cref="IServiceConvention" />
    public class AspNetCoreFluentValidationConvention : IServiceConvention
    {
        /// <summary>
        /// Registers the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Register(IServiceConventionContext context)
        {
            context.Services.AddMvcCore()
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new ValidationProblemDetailsConverter()))
                .AddFluentValidation();
            context.Services.AddSingleton<IValidatorInterceptor, ValidatorInterceptor>();
            context.Services.AddSingleton<ProblemDetailsFactory, FluentValidationProblemDetailsFactory>();

            context.Services.Configure<ApiBehaviorOptions>(o =>
            {
                ProblemDetailsFactory? _problemDetailsFactory = null;
                o.InvalidModelStateResponseFactory = context =>
                {
                    // ProblemDetailsFactory depends on the ApiBehaviorOptions instance. We intentionally avoid constructor injecting
                    // it in this options setup to to avoid a DI cycle.
                    _problemDetailsFactory ??= context.HttpContext.RequestServices
                        .GetRequiredService<ProblemDetailsFactory>();
                    return ProblemDetailsInvalidModelStateResponse(_problemDetailsFactory, context);
                };

                static IActionResult ProblemDetailsInvalidModelStateResponse(
                    ProblemDetailsFactory problemDetailsFactory, ActionContext context)
                {
                    var problemDetails =
                        problemDetailsFactory.CreateValidationProblemDetails(context.HttpContext, context.ModelState);
                    ObjectResult result;
                    if (problemDetails.Status == 400)
                    {
                        // For compatibility with 2.x, continue producing BadRequestObjectResult instances if the status code is 400.
                        result = new BadRequestObjectResult(problemDetails);
                    }
                    else
                    {
                        result = new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
                    }

                    result.ContentTypes.Add("application/problem+json");
                    result.ContentTypes.Add("application/problem+xml");

                    return result;
                }
            });
        }
    }
}
