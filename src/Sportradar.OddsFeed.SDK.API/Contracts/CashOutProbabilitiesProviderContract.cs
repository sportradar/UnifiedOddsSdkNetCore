/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Contracts
{
    [ContractClassFor(typeof(ICashOutProbabilitiesProvider))]
    internal abstract class CashOutProbabilitiesProviderContract : ICashOutProbabilitiesProvider
    {
        public Task<ICashOutProbabilities<T>> GetCashOutProbabilitiesAsync<T>(URN eventId, CultureInfo culture = null) where T : ISportEvent
        {
            Contract.Requires(eventId != null);
            return Contract.Result<Task<ICashOutProbabilities<T>>>();
        }

        public Task<ICashOutProbabilities<T>> GetCashOutProbabilitiesAsync<T>(URN eventId, int marketId, IReadOnlyDictionary<string, string> specifiers, CultureInfo culture = null) where T : ISportEvent
        {
            Contract.Requires(eventId != null);
            return Contract.Result<Task<ICashOutProbabilities<T>>>();
        }
    }
}
