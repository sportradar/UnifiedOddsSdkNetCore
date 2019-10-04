/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities;

namespace Sportradar.OddsFeed.SDK.API.Contracts
{
    [ContractClassFor(typeof(IOddsFeedSessionBuilder))]
    abstract class OddsFeedSessionBuilderContract : IOddsFeedSessionBuilder
    {
        public ISessionBuilder SetMessageInterest(MessageInterest msgInterest)
        {
            Contract.Requires(msgInterest != null);
            Contract.Ensures(Contract.Result<ISessionBuilder>() != null);
            return Contract.Result<ISessionBuilder>();
        }
    }

    [ContractClassFor(typeof(ISessionBuilder))]
    abstract class SessionBuilderContract : ISessionBuilder
    {
        public IOddsFeedSession Build()
        {
            Contract.Ensures(Contract.Result<IOddsFeedSession>() != null);
            return Contract.Result<IOddsFeedSession>();
        }
    }
}
