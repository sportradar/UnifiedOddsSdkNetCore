/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    /// <summary>
    /// Class TestProducersProvider for setting default producers list
    /// Implements the <see cref="IProducersProvider" />
    /// </summary>
    /// <seealso cref="IProducersProvider" />
    public class TestProducersProvider : IProducersProvider
    {
        /// <summary>
        /// Gets the producers
        /// </summary>
        /// <value>The producers</value>
        public List<IProducer> Producers { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestProducersProvider"/> class. Loads default list of producers.
        /// </summary>
        public TestProducersProvider()
        {
            Producers = GetProducers().ToList();
        }

        /// <summary>
        /// Gets the producers (instead of getting them from API)
        /// </summary>
        /// <returns>Gets the producers (instead of getting them from API)</returns>
        public Task<IEnumerable<IProducer>> GetProducersAsync()
        {
            return Task.FromResult(Producers.AsEnumerable());
        }

        /// <summary>
        /// Gets the available producers from api (default setup used in most tests)
        /// </summary>
        /// <returns>A list of <see cref="IProducer"/></returns>
        public IEnumerable<IProducer> GetProducers()
        {
            var maxInactivitySeconds = 20;
            var maxRecoveryTime = 3600;

            var producers = new List<IProducer>
            {
                new Producer(1, "LO", "Live Odds", "https://stgapi.betradar.com/v1/liveodds/", true, maxInactivitySeconds, maxRecoveryTime, "live", 600),
                new Producer(3, "Ctrl", "Betradar Ctrl", "https://stgapi.betradar.com/v1/pre/", true, maxInactivitySeconds, maxRecoveryTime, "prematch", 4320),
                new Producer(4, "BetPal", "BetPal", "https://stgapi.betradar.com/v1/betpal/", true, maxInactivitySeconds, maxRecoveryTime, "live", 4320),
                new Producer(5, "PremiumCricket", "Premium Cricket", "https://stgapi.betradar.com/v1/premium_cricket/", true, maxInactivitySeconds, maxRecoveryTime, "live|prematch", 4320),
                new Producer(6, "VF", "Virtual football", "https://stgapi.betradar.com/v1/vf/", true, maxInactivitySeconds, maxRecoveryTime, "virtual", 180),
                new Producer(7, "WNS", "Numbers Betting", "https://stgapi.betradar.com/v1/wns/", true, maxInactivitySeconds, maxRecoveryTime, "prematch", 4320),
                new Producer(8, "VBL", "Virtual Basketball League", "https://stgapi.betradar.com/v1/vbl/", false, maxInactivitySeconds, maxRecoveryTime, "virtual", 180)
            };

            return producers;
        }
    }
}
