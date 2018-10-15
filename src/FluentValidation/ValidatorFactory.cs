using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;

namespace Rocket.Surgery.Core.Validation
{
    class ValidatorFactory : ValidatorFactoryBase
    {
        private readonly IServiceProvider _context;

        public ValidatorFactory(IServiceProvider context)
        {
            _context = context;
        }

        public override IValidator CreateInstance(Type validatorType)
        {
            var service = _context.GetService(validatorType);
            if (service is IValidator validator)
            {
                return validator;
            }

            return null;
        }
    }
}
