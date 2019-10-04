/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Contracts
{
    [ContractClassFor(typeof(IMappingValidatorFactory))]
    abstract class MappingValidatorFactoryContract : IMappingValidatorFactory
    {
        public IMappingValidator Build(string value)
        {
            Contract.Requires(!string.IsNullOrEmpty(value));
            Contract.Ensures(Contract.Result<IMappingValidator>() != null);

            return Contract.Result<IMappingValidator>();
        }
    }
}
