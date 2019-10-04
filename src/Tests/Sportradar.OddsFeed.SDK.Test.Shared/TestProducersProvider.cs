/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    public class TestProducersProvider : IProducersProvider
    {
        public Task<IEnumerable<IProducer>> GetProducersAsync()
        {
            return Task.FromResult(GetProducers());
        }

        public IEnumerable<IProducer> GetProducers()
        {
            var maxInactivitySeconds = 20;
            var maxRecoveryTime = 3600;

            var producers = new List<IProducer>
            {
                new Producer(1, "LO", "Live Odds", "lo", true, maxInactivitySeconds, maxRecoveryTime, "live"),
                new Producer(3, "Ctrl", "Betradar Ctrl", "pre", true, maxInactivitySeconds, maxRecoveryTime, "prematch"),
                new Producer(4, "BetPal", "BetPal", "betpal", true, maxInactivitySeconds, maxRecoveryTime, "live"),
                new Producer(5, "PremiumCricket", "Premium Cricket", "premium_cricket", true, maxInactivitySeconds, maxRecoveryTime, "live|prematch"),
                new Producer(6, "VF", "Virtual football", "vf", false, maxInactivitySeconds, maxRecoveryTime, "virtual")
            };

            return producers;
        }
    }
}
