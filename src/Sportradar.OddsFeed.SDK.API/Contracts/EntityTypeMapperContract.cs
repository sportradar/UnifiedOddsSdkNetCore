/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Contracts
{
    [ContractClassFor(typeof(IEntityTypeMapper))]
    internal abstract class EntityTypeMapperContract : IEntityTypeMapper
    {
        public Type Map(URN id, int sportId)
        {
            Contract.Requires(id != null);
            Contract.Requires(sportId > 0);
            Contract.Ensures(Contract.Result<Type>() != null);

            return Contract.Result<Type>();
        }
    }
}
