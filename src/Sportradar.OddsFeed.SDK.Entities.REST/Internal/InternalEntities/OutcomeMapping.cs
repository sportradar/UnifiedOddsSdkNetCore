/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Entities.REST.MarketMapping;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.InternalEntities
{
    internal class OutcomeMapping : IOutcomeMappingData
    {
        /// <summary>
        /// Gets the id of the outcome
        /// </summary>
        /// <value>The outcome identifier</value>
        public string OutcomeId { get; }

        /// <summary>
        /// Gets the producer outcome identifier
        /// </summary>
        /// <value>The producer outcome identifier</value>
        public string ProducerOutcomeId { get; }

        /// <summary>
        /// The producer outcome names
        /// </summary>
        private readonly IDictionary<CultureInfo, string> _producerOutcomeNames;

        /// <summary>
        /// Gets the name of the producer outcome in specified language
        /// </summary>
        public string GetProducerOutcomeName(CultureInfo culture)
        {
            if (_producerOutcomeNames == null)
            {
                return null;
            }

            string name;
            return _producerOutcomeNames.TryGetValue(culture, out name) ? name : null;
        }

        /// <summary>
        /// Gets the mapped market identifier
        /// </summary>
        /// <value>The mapped market identifier</value>
        public string MarketId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutcomeMapping"/> class
        /// </summary>
        /// <param name="cacheItem">The cache item</param>
        internal OutcomeMapping(OutcomeMappingCacheItem cacheItem)
        {
            Contract.Requires(cacheItem != null);

            OutcomeId = cacheItem.OutcomeId;
            ProducerOutcomeId = cacheItem.ProducerOutcomeId;
            _producerOutcomeNames = cacheItem.ProducerOutcomeNames;
            MarketId = cacheItem.MarketId;
        }
    }
}
