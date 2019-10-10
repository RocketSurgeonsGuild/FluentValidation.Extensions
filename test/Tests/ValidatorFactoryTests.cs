using System;
using System.Collections.Generic;
using FakeItEasy;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.FluentValidation.Tests
{
    public class ValidatorFactoryTests : AutoFakeTest
    {
        public ValidatorFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper, LogLevel.Information) { }

        class AModel
        {
            public string? Id { get; set; }
            public string? Other { get; set; }
        }

        class ValidatorAa : AbstractValidator<AModel>
        {
            public ValidatorAa()
            {
                RuleFor(x => x.Id).NotEmpty();
            }
        }

        class ValidatorAb : AbstractValidator<AModel>
        {
            public ValidatorAb()
            {
                RuleFor(x => x.Other).NotEmpty();
            }
        }

        [Fact]
        public void Test1()
        {
            var sp = A.Fake<IServiceProvider>();
            A.CallTo(() => sp.GetService(typeof(IEnumerable<IValidator<AModel>>))).Returns(new IValidator[] { new ValidatorAb(), new ValidatorAa() });
            AutoFake.Provide(sp);

            var factory = AutoFake.Resolve<ValidatorFactory>();
            var validator = factory.GetValidator<AModel>();

        }
    }
}
