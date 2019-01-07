using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentAssertions;
using FluentValidation.Validators;
using Humanizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using NodaTime.Testing;
using Rocket.Surgery.Extensions.FluentValidation.Vue;
using Rocket.Surgery.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.FluentValidation.Tests
{
    public class VeeValidateCommandTests : AutoTestBase
    {
        class T
        {
            public int Property { get; }
        }

        enum A
        {
            Hello, World, ThisIsGreat
        }

        private readonly VeeValidateCommand command;

        public VeeValidateCommandTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            AutoFake.Provide(new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Converters = { new StringEnumConverter(true) }
            }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));
            command = AutoFake.Resolve<VeeValidateCommand>();
        }

        [Fact]
        public void Should_Return_No_Rules_When_Given_No_Validators()
        {
            var rules = command.ProcessMember(typeof(T), "property", typeof(string), new IPropertyValidator[]
            {

            });
            rules.Should().BeEmpty();
        }

        [Fact]
        public void Should_Handle_Email_Validator()
        {
            var rules = command.ProcessMember(typeof(T), "property", typeof(string), new IPropertyValidator[]
            {
                new EmailValidator(),
            });
            rules[0].key.Should().Be("email");
            rules[0].rules.Should().Be("true");
        }

        [Fact]
        public void Should_Handle_Enum_Validator()
        {
            var rules = command.ProcessMember(typeof(T), "property", typeof(A), new IPropertyValidator[]
            {
                new EnumValidator(typeof (A)),
            });
            rules[0].key.Should().Be("in");
            rules[0].rules.Should().Be("[\"hello\",\"world\",\"thisIsGreat\"]");
        }

        [Theory, MemberData(nameof(Should_Handle_Comparison_Validators_Data))]
        public void Should_Handle_Comparison_Validators(
            IComparisonValidator validator,
            Type propertyType,
            (string key, string rules)[] expectedRules)
        {
            var rules = command.ProcessMember(typeof(T), "property", propertyType, new IPropertyValidator[]
            {
                validator
            });
            rules.Should().ContainInOrder(expectedRules);
        }

        #region Should_Handle_Comparison_Validators_Data
        public static IEnumerable<object[]> Should_Handle_Comparison_Validators_Data()
        {
            var clock = new FakeClock(Instant.FromUnixTimeSeconds(1524000000));
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Converters = { new StringEnumConverter(true) }
            }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            //EqualValidator equalValidator:
            //GreaterThanOrEqualValidator greaterThanOrEqualValidator:
            //GreaterThanValidator greaterThanValidator:
            //LessThanOrEqualValidator lessThanOrEqualValidator:
            //LessThanValidator lessThanValidator:
            //NotEqualValidator notEqualValidator:
            foreach (IComparable value in new object[] { 1, 1.2f, 1.3d, 1.4M })
                yield return new object[]
                {
                    new EqualValidator(value),
                    value.GetType(),
                    new [] { ("is", JsonConvert.SerializeObject(value, settings)) }
                };
            yield return new object[]
            {
                new EqualValidator(x => null, typeof(T).GetProperty(nameof(T.Property))),
                typeof(int),
                new [] { ("is", $"this.{nameof(T.Property).Camelize()}") }
            };

            foreach (IComparable value in new object[] { 1, 1.0f, 1.0d, 1.0M })
                yield return new object[] {
                    new NotEqualValidator(value),
                    value.GetType(),
                new [] { ("not_is", JsonConvert.SerializeObject(value, settings)) }
                };
            yield return new object[]
            {
                new NotEqualValidator(x => null, typeof(T).GetProperty(nameof(T.Property))),
                typeof(int),
                new [] { ("not_is", $"this.{nameof(T.Property).Camelize()}") }
            };

            foreach (IComparable value in new object[] { 1, 1.0f, 1.0d, 1.0M })
                yield return new object[] {
                    new GreaterThanOrEqualValidator(value),
                    value.GetType(),
                    new [] { ("max_value", JsonConvert.SerializeObject(value, settings)) }
                };

            foreach (IComparable value in new object[]
            {
                clock.GetCurrentInstant().ToDateTimeUtc(),
                clock.GetCurrentInstant().ToDateTimeOffset(),
                clock.GetCurrentInstant()
            })
                yield return new object[] {
                    new GreaterThanOrEqualValidator(value),
                    value.GetType(),
                    new [] { ("is_date_format", null), ("after", $"[{JsonConvert.SerializeObject(value, settings)},true]") }
                };

            yield return new object[]
            {
                new GreaterThanOrEqualValidator(x => null, typeof(T).GetProperty(nameof(T.Property))),
                typeof(int),
                new [] { ("max_value", $"this.{nameof(T.Property).Camelize()}") }
            };

            foreach (IComparable value in new object[] { 1, 1.0f, 1.0d, 1.0M, })
                yield return new object[]
                {
                    new GreaterThanValidator(value),
                    value.GetType(),
                    new [] { ("max_neq_value", JsonConvert.SerializeObject(value, settings)) }
                };

            foreach (IComparable value in new object[]
            {
                clock.GetCurrentInstant().ToDateTimeUtc(),
                clock.GetCurrentInstant().ToDateTimeOffset(),
                clock.GetCurrentInstant()
            })
                yield return new object[]
                {
                    new GreaterThanValidator(value),
                    value.GetType(),
                    new [] { ("is_date_format", null), ("after", $"[{JsonConvert.SerializeObject(value, settings)},false]") }
                };

            yield return new object[]
            {
                new GreaterThanValidator(x => null, typeof(T).GetProperty(nameof(T.Property))),
                typeof(int),
                new [] { ("max_neq_value", $"this.{nameof(T.Property).Camelize()}") }
            };

            foreach (IComparable value in new object[] { 1, 1.0f, 1.0d, 1.0M })
                yield return new object[]
                {
                    new LessThanOrEqualValidator(value),
                    value.GetType(),
                    new [] { ("min_value", JsonConvert.SerializeObject(value, settings)) }
                };

            foreach (IComparable value in new object[]
            {
                clock.GetCurrentInstant().ToDateTimeUtc(),
                clock.GetCurrentInstant().ToDateTimeOffset(),
                clock.GetCurrentInstant()
            })
                yield return new object[]
                {
                    new LessThanOrEqualValidator(value),
                    value.GetType(),
                    new [] { ("is_date_format", null), ("before", $"[{JsonConvert.SerializeObject(value, settings)},true]") }
                };

            yield return new object[]
            {
                new LessThanOrEqualValidator(x => null, typeof(T).GetProperty(nameof(T.Property))),
                typeof(int),
                new [] { ("min_value", $"this.{nameof(T.Property).Camelize()}") }
            };

            foreach (IComparable value in new object[] { 1, 1.0f, 1.0d, 1.0M })
                yield return new object[]
                {
                    new LessThanValidator(value),
                    value.GetType(),
                    new [] { ("min_neq_value", JsonConvert.SerializeObject(value, settings)) }
                };

            foreach (IComparable value in new object[]
            {
                clock.GetCurrentInstant().ToDateTimeUtc(),
                clock.GetCurrentInstant().ToDateTimeOffset(),
                clock.GetCurrentInstant()
            })
                yield return new object[]
                {
                    new LessThanValidator(value),
                    value.GetType(),
                    new [] { ("is_date_format", null), ("before", $"[{JsonConvert.SerializeObject(value, settings)},false]") }
                };

            yield return new object[]
            {
                new LessThanValidator(x => null,
                    typeof(T).GetProperty(nameof(T.Property))),
                typeof(int),
                new [] { ("min_neq_value", $"this.{nameof(T.Property).Camelize()}") }
            };

        }
        #endregion


        [Theory, MemberData(nameof(Should_Handle_Betweet_Validators_Data))]
        public void Should_Handle_Betweet_Validators(
            IBetweenValidator validator,
            Type propertyType,
            (string key, string rules)[] expectedRules)
        {
            var rules = command.ProcessMember(typeof(T), "property", propertyType, new IPropertyValidator[]
            {
                validator
            });
            rules.Should().ContainInOrder(expectedRules);
        }

        #region Should_Handle_Betweet_Validators_Data
        public static IEnumerable<object[]> Should_Handle_Betweet_Validators_Data()
        {
            var clock = new FakeClock(Instant.FromUnixTimeSeconds(1524000000));
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Converters = { new StringEnumConverter(true) }
            }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            //EqualValidator equalValidator:
            //GreaterThanOrEqualValidator greaterThanOrEqualValidator:
            //GreaterThanValidator greaterThanValidator:
            //LessThanOrEqualValidator lessThanOrEqualValidator:
            //LessThanValidator lessThanValidator:
            //NotEqualValidator notEqualValidator:

            foreach (var (from, to) in new(IComparable from, IComparable to)[] { (1, 10), (1.0f, 2.0f), (1.0d, 2.0d), (1.0M, 100.0M) })
                yield return new object[] {
                    new InclusiveBetweenValidator(from, to),
                    from.GetType(),
                    new [] { ("between", JsonConvert.SerializeObject(new [] { from, to }, settings)) }
                };

            foreach (var (fromx, tox) in new(IComparable from, IComparable to)[]
            {
                (clock.GetCurrentInstant().ToDateTimeUtc(), clock.GetCurrentInstant().Plus(Duration.FromDays(7)).ToDateTimeUtc()),
                (clock.GetCurrentInstant().ToDateTimeOffset(), clock.GetCurrentInstant().Plus(Duration.FromDays(7)).ToDateTimeOffset()),
                (clock.GetCurrentInstant(), clock.GetCurrentInstant().Plus(Duration.FromDays(7))),
            })
            {
                var from = JsonConvert.SerializeObject(fromx, settings);
                var to = JsonConvert.SerializeObject(tox, settings);
                yield return new object[]
                {
                    new InclusiveBetweenValidator(fromx, tox),
                    fromx.GetType(),
                    new[]
                    {
                        ("is_date_format", null),
                        ("date_between", $"[{from},{to},\"[]\"]")
                    }
                };
            }

            foreach (var (from, to) in new(IComparable from, IComparable to)[] { (1, 10), (1.0f, 2.0f), (1.0d, 2.0d), (1.0M, 100.0M) })
                yield return new object[] {
                    new ExclusiveBetweenValidator(from, to),
                    from.GetType(),
                    new [] { ("between_neq", JsonConvert.SerializeObject(new [] { from, to }, settings)) }
                };

            foreach (var (fromx, tox) in new(IComparable from, IComparable to)[]
            {
                (clock.GetCurrentInstant().ToDateTimeUtc(), clock.GetCurrentInstant().Plus(Duration.FromDays(7)).ToDateTimeUtc()),
                (clock.GetCurrentInstant().ToDateTimeOffset(), clock.GetCurrentInstant().Plus(Duration.FromDays(7)).ToDateTimeOffset()),
                (clock.GetCurrentInstant(), clock.GetCurrentInstant().Plus(Duration.FromDays(7))),
            })
            {
                var from = JsonConvert.SerializeObject(fromx, settings);
                var to = JsonConvert.SerializeObject(tox, settings);
                yield return new object[]
                {
                    new ExclusiveBetweenValidator(fromx, tox),
                    fromx.GetType(),
                    new[]
                    {
                        ("is_date_format", null),
                        ("date_between", $"[{from},{to},\"()\"]")
                    }
                };
            }
        }
        #endregion

    }
}
