using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;
using Humanizer;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using Rocket.Surgery.Extensions.FluentValidation.Vue;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Extensions.CommandLine;

[assembly: Convention(typeof(VeeValidateCommandConvention))]

namespace Rocket.Surgery.Extensions.FluentValidation.Vue
{
    /// <summary>
    ///  VeeValidateCommandConvention.
    /// Implements the <see cref="Rocket.Surgery.Extensions.CommandLine.ICommandLineConvention" />
    /// </summary>
    /// <seealso cref="Rocket.Surgery.Extensions.CommandLine.ICommandLineConvention" />
    class VeeValidateCommandConvention : ICommandLineConvention
    {
        /// <summary>
        /// Registers the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Register(ICommandLineConventionContext context)
        {
            context.AddCommand<VueCommand>("vue");
        }
    }

    /// <summary>
    ///  VueCommand.
    /// </summary>
    [Command("vue", Description = "Commands related to using the vue framework"),
        Subcommand(typeof(VeeValidateCommand))]
    class VueCommand
    {
        /// <summary>
        /// Called when [execute].
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="console">The console.</param>
        /// <returns>System.Int32.</returns>
        public int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.WriteLine("You must specify at a subcommand.");
            app.ShowHelp();
            return 1;
        }
    }

    /// <summary>
    ///  VeeValidateCommand.
    /// </summary>
    [Command(Description = "Export vee validate definitions for all the models that have the IVeeValidate interface attached")]
    class VeeValidateCommand
    {
        const BindingFlags CommonBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        private readonly IAssemblyCandidateFinder _assemblyCandidateFinder;
        private readonly IValidatorFactory _validatorFactory;
        private readonly ILogger<VeeValidateCommand> _logger;
        private readonly JsonSerializerSettings _serializerSettings;

        /// <summary>
        /// Gets the output directory.
        /// </summary>
        /// <value>The output directory.</value>
        [Argument(0, Description = "The path where to write the validation definitions out to (as a typescript file)")]
        public string OutputDirectory { get; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="VeeValidateCommand"/> class.
        /// </summary>
        /// <param name="assemblyCandidateFinder">The assembly candidate finder.</param>
        /// <param name="validatorFactory">The validator factory.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="serializerSettings">The serializer settings.</param>
        public VeeValidateCommand(
            IAssemblyCandidateFinder assemblyCandidateFinder,
            IValidatorFactory validatorFactory,
            ILogger<VeeValidateCommand> logger,
            JsonSerializerSettings serializerSettings)
        {
            _assemblyCandidateFinder = assemblyCandidateFinder;
            _validatorFactory = validatorFactory;
            _logger = logger;
            _serializerSettings = serializerSettings;
        }

        /// <summary>
        /// on execute as an asynchronous operation.
        /// </summary>
        /// <returns>Task{System.Int32}.</returns>
        public async Task<int> OnExecuteAsync()
        {
            await Task.Yield();
            var validationTypes = _assemblyCandidateFinder.GetCandidateAssemblies("Rocket.Surgery.Vue.Validation")
                .SelectMany(assembly => assembly.GetTypes()
                    .Where(typeof(IVeeValidate).IsAssignableFrom));

            foreach (var type in validationTypes)
            {
                var sb = new StringBuilder("/* tslint:disable */\n");
                sb.AppendLine($"export function {type.Name}Validator(this: any) {{");
                sb.AppendLine($"    return {{");

                var validator = _validatorFactory.GetValidator(type);
                if (validator == null)
                {
                    _logger.LogWarning("Could not find validator for {Type}", type.FullName);
                    continue;
                }
                var descriptor = validator.CreateDescriptor();

                foreach (var member in descriptor.GetMembersWithValidators())
                {
                    sb.AppendLine($"        {member.Key.Camelize()}: {{");

                    Type? memberType = null;

                    var propertyInfo = type.GetProperty(member.Key, CommonBindingFlags);
                    if (propertyInfo != null) memberType = propertyInfo.PropertyType;

                    var fieldInfo = type.GetField(member.Key, CommonBindingFlags);
                    if (fieldInfo != null) memberType = fieldInfo.FieldType;

                    var rules = ProcessMember(type, member.Key.Camelize(), memberType!, member)
                        .GroupBy(x => x.key, x => x.rules)
                        .ToDictionary(x => x.Key, x => x.First());

                    foreach (var rule in rules)
                    {
                        sb.AppendLine($"            '{rule.Key}': {rule.Value},");
                    }

                    sb.AppendLine($"        }},");
                }

                sb.AppendLine($"    }};");
                sb.AppendLine("}");

                File.WriteAllText(Path.Combine(OutputDirectory, $"{type.Name}.ts"), sb.ToString());
            }

            return 0;
        }

        /// <summary>
        /// Processes the member.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="memberType">Type of the member.</param>
        /// <param name="propertyValidators">The property validators.</param>
        /// <returns>List{ValueTuple{String, String}}</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal List<(string key, string? rules)> ProcessMember(Type type, string memberName, Type memberType, IEnumerable<IPropertyValidator> propertyValidators)
        {
            var rules = new List<(string key, string? rules)>();

            bool ValidateLengthValidator(LengthValidator lv)
            {
                if (lv.MinFunc != null || lv.MaxFunc != null)
                {
                    _logger.LogWarning("{Validator} {Type}:{MemberName} with a min or max delegate is not supported.", lv.GetType().Name, type.FullName, memberName);
                    return false;
                }

                var isNumericType = IsNumericType(memberType);
                var isString = memberType == typeof(string);
                if (!isNumericType && !isString)
                {
                    _logger.LogWarning("{Validator} {Type}:{MemberName} is not supported.", lv.GetType().Name, type.FullName, memberName);
                    return false;
                }

                return true;
            }

            foreach (var propertyValidator in propertyValidators)
            {
                switch (propertyValidator)
                {
                    //AsyncPredicateValidator asyncPredicateValidator){}
                    case CreditCardValidator creditCardValidator:
                        rules.Add(("credit_card", "true"));
                        break;
                    //IDelegatingValidator delegatingValidator) { }
                    case IEmailValidator emailValidator:
                        rules.Add(("email", "true"));
                        break;
                    case EnumValidator enumValidator:
                        var enumType = (Type)enumValidator.GetType().GetField("_enumType", CommonBindingFlags)?.GetValue(enumValidator)!;
                        var values = Enum.GetValues(enumType).Cast<object>();

                        rules.Add(("in", $"{JsonConvert.SerializeObject(values.ToArray(), _serializerSettings)}"));
                        break;
                    //EqualValidator equalValidator:
                    //GreaterThanOrEqualValidator greaterThanOrEqualValidator:
                    //GreaterThanValidator greaterThanValidator:
                    //LessThanOrEqualValidator lessThanOrEqualValidator:
                    //LessThanValidator lessThanValidator:
                    //NotEqualValidator notEqualValidator:
                    case IComparisonValidator comparisonValidator:
                        {
                            var isNumericType = IsNumericType(memberType);
                            var isDateType = IsDateType(memberType);

                            if (!isNumericType && !isDateType)
                            {
                                _logger.LogWarning("{Validator} {Type}:{MemberName} is not supported.", comparisonValidator.GetType().Name, type.FullName, memberName);
                                continue;
                            }
                            var key = comparisonValidator.Comparison switch
                            {
                                Comparison.Equal => "is",
                                Comparison.NotEqual => "not_is", // custom validator
                                Comparison.LessThan => isNumericType ? "min_neq_value" /* custom validator */ : "before",
                                Comparison.LessThanOrEqual => isNumericType ? "min_value" : "before",
                                Comparison.GreaterThan => isNumericType ? "max_neq_value" /* custom validator */ : "after",
                                Comparison.GreaterThanOrEqual => isNumericType ? "max_value" : "after",
                                _ => throw new ArgumentOutOfRangeException(),
                            };
                            string value;
                            if (comparisonValidator.MemberToCompare != null)
                            {
                                value = $"this.{comparisonValidator.MemberToCompare?.Name.Camelize()}";
                            }
                            else
                            {
                                value = JsonConvert.SerializeObject(comparisonValidator.ValueToCompare, _serializerSettings);
                            }

                            if (isDateType)
                            {
                                var inclusive = JsonConvert.SerializeObject(
                                    comparisonValidator.Comparison == Comparison.LessThanOrEqual ||
                                    comparisonValidator.Comparison == Comparison.GreaterThanOrEqual,
                                    _serializerSettings);
                                rules.Add(("is_date_format", null));
                                rules.Add((key, $"[{value},{inclusive}]"));
                            }
                            else
                            {
                                rules.Add((key, value));
                            }

                            break;
                        }
                    //ExclusiveBetweenValidator exclusiveBetweenValidator:
                    //InclusiveBetweenValidator inclusiveBetweenValidator:
                    case IBetweenValidator betweenValidator:
                        {
                            var isNumericType = IsNumericType(memberType);
                            var isDateType = IsDateType(memberType);

                            if (!isNumericType && !isDateType)
                            {
                                _logger.LogWarning("{Validator} {Type}:{MemberName} is not supported.", betweenValidator.GetType().Name, type.FullName, memberName);
                                continue;
                            }

                            var inclusive = betweenValidator is InclusiveBetweenValidator;
                            var from = JsonConvert.SerializeObject(betweenValidator.From, _serializerSettings);
                            var to = JsonConvert.SerializeObject(betweenValidator.To, _serializerSettings);
                            if (isDateType)
                            {
                                rules.Add(("is_date_format", null));
                                rules.Add(("date_between", $"[{from},{to},\"{(inclusive ? "[]" : "()")}\"]"));
                            }
                            else
                            {
                                rules.Add((inclusive ? "between" : "between_neq", $"[{from},{to}]"));
                            }

                            break;
                        }
                    case ExactLengthValidator exactLengthValidator when !ValidateLengthValidator(exactLengthValidator):
                        continue;
                    case ExactLengthValidator exactLengthValidator when IsNumericType(memberType):
                        rules.Add(("digits", JsonConvert.SerializeObject(exactLengthValidator.Min, _serializerSettings)));
                        break;
                    case ExactLengthValidator exactLengthValidator:
                        rules.Add(("min", JsonConvert.SerializeObject(exactLengthValidator.Min, _serializerSettings)));
                        rules.Add(("max", JsonConvert.SerializeObject(exactLengthValidator.Max, _serializerSettings)));
                        break;
                    case MaximumLengthValidator maximumLengthValidator when !ValidateLengthValidator(maximumLengthValidator):
                        continue;
                    case MaximumLengthValidator maximumLengthValidator:
                        rules.Add(("max", JsonConvert.SerializeObject(maximumLengthValidator.Max, _serializerSettings)));
                        break;
                    case MinimumLengthValidator minimumLengthValidator when !ValidateLengthValidator(minimumLengthValidator):
                        continue;
                    case MinimumLengthValidator minimumLengthValidator:
                        rules.Add(("min", JsonConvert.SerializeObject(minimumLengthValidator.Min, _serializerSettings)));
                        break;
                    case LengthValidator lengthValidator when !ValidateLengthValidator(lengthValidator):
                        continue;
                    case LengthValidator lengthValidator:
                        rules.Add(("min", JsonConvert.SerializeObject(lengthValidator.Min, _serializerSettings)));
                        rules.Add(("max", JsonConvert.SerializeObject(lengthValidator.Max, _serializerSettings)));
                        break;
                    case INotEmptyValidator _:
                    case INotNullValidator _:
                        rules.Add(("required", "true"));
                        break;
                    //IEmptyValidator emptyValidator:
                    //INullValidator nullValidator:
                    //IPredicateValidator predicateValidator:
                    case IRegularExpressionValidator regularExpressionValidator:
                        rules.Add(("regex", regularExpressionValidator.Expression));
                        break;
                    default:
                        _logger.LogWarning("Skipping {Validator} {Type}:{MemberName} as it is not supported.", propertyValidator.GetType().Name, type.FullName, memberName);
                        break;
                }

                //ScalePrecisionValidator scalePrecisionValidator) {}
            }

            return rules;
        }

        private static readonly HashSet<Type> NumericTypes = new HashSet<Type>(new[]
        {
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal)
        });

        private static bool IsNumericType(Type type)
        {
            return NumericTypes.Contains(type);
        }

        private static readonly HashSet<Type> DateTypes = new HashSet<Type>(new[]
        {
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(Instant),
            typeof(NodaTime.AnnualDate),
            typeof(NodaTime.LocalDate),
            typeof(NodaTime.LocalDateTime),
            typeof(NodaTime.OffsetDateTime),
            typeof(NodaTime.ZonedDateTime),
        });

        private static bool IsDateType(Type type)
        {
            return DateTypes.Contains(type);
        }
    }

    /// <summary>
    ///  IVeeValidate
    /// </summary>
    public interface IVeeValidate { }
}
