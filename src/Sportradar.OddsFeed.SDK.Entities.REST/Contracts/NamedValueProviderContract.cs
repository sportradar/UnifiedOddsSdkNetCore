/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Contracts
{
    [ContractClassFor(typeof(INamedValuesProvider))]
    abstract class NamedValueProviderContract : INamedValuesProvider
    {
        public INamedValueCache VoidReasons
        {
            get
            {
                Contract.Ensures(Contract.Result<INamedValueCache>() != null);
                return Contract.Result<INamedValueCache>();
            }
        }

        public INamedValueCache BetStopReasons
        {
            get
            {
                Contract.Ensures(Contract.Result<INamedValueCache>() != null);
                return Contract.Result<INamedValueCache>();
            }
        }

        public INamedValueCache BettingStatuses
        {
            get
            {
                Contract.Ensures(Contract.Result<INamedValueCache>() != null);
                return Contract.Result<INamedValueCache>();
            }
        }

        public ILocalizedNamedValueCache MatchStatuses
        {
            get
            {
                Contract.Ensures(Contract.Result<ILocalizedNamedValueCache>() != null);
                return Contract.Result<ILocalizedNamedValueCache>();
            }
        }
    }
}
