/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Entities.REST.MarketMapping;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.InternalEntities
{
    internal class MarketMapping : IMarketMappingData
    {
        /// <summary>
        /// The <see cref="IMappingValidator"/> used to determine whether the current mapping can map specific market
        /// or a null reference if no restrictions apply to this mapping
        /// </summary>
        private readonly IMappingValidator _validator;

        /// <summary>
        /// Gets the id of the producer to which the associated market / outright belongs to
        /// </summary>
        [Obsolete("Changed with ProducerIds property")]
        public int ProducerId { get; }

        /// <summary>
        /// Gets the ids of the producers to which the associated market / outright belongs to
        /// </summary>
        /// <value>The producer ids</value>
        public IEnumerable<int> ProducerIds { get; }

        /// <summary>
        /// Gets the id of the sport to which the associated market / outright belongs to
        /// </summary>
        public URN SportId { get; }

        /// <summary>
        /// Gets the id of the market associated with the current instance
        /// </summary>
        public string MarketId { get; }

        /// <summary>
        /// Gets the market type identifier
        /// </summary>
        public int MarketTypeId { get; }

        /// <summary>
        /// Gets the market sub type identifier
        /// </summary>
        public int? MarketSubTypeId { get; }

        /// <summary>
        /// Gets the special odds value template value
        /// </summary>
        public string SovTemplate { get; }

        /// <summary>
        /// Gets the valid_for value
        /// </summary>
        public string ValidFor { get; }

        /// <summary>
        /// Gets the outcome mappings
        /// </summary>
        public IEnumerable<IOutcomeMappingData> OutcomeMappings { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketMapping"/> class
        /// </summary>
        /// <param name="cacheItem">A <see cref="MarketMappingCacheItem"/> containing mapping info</param>
        internal MarketMapping(MarketMappingCacheItem cacheItem)
        {
            Guard.Argument(cacheItem).NotNull();

            ProducerId = cacheItem.ProducerId;
            ProducerIds = cacheItem.ProducerIds;
            SportId = cacheItem.SportId;
            MarketTypeId = cacheItem.MarketTypeId;
            MarketSubTypeId = cacheItem.MarketSubTypeId;
            MarketId = string.IsNullOrEmpty(cacheItem.OrgMarketId)
                           ? MarketSubTypeId == null
                                 ? MarketTypeId.ToString()
                                 : $"{MarketTypeId}:{MarketSubTypeId}"
                           : cacheItem.OrgMarketId;
            SovTemplate = cacheItem.SovTemplate;
            ValidFor = cacheItem.ValidFor;
            OutcomeMappings = cacheItem.OutcomeMappings?.Select(o => new OutcomeMapping(o));

            _validator = cacheItem.Validator;
        }

        /// <summary>
        /// Determines whether the current mapping can map market with provided specifiers associated with provided producer and sport
        /// </summary>
        /// <param name="producer">The <see cref="IProducer" /> associated with the market</param>
        /// <param name="sportId">The <see cref="URN" /> specifying the sport associated with the market</param>
        /// <param name="specifiers">The market specifiers</param>
        /// <returns>True if the current mapping can be used to map the specified market. False otherwise</returns>
        /// <exception cref="InvalidOperationException">The provided specifiers are not valid</exception>
        public bool CanMap(IProducer producer, URN sportId, IReadOnlyDictionary<string, string> specifiers)
        {
            if (!ProducerIds.Contains(producer.Id) || SportId != null && !SportId.Equals(sportId))
            {
                return false;
            }

            return _validator?.Validate(specifiers) ?? true;
        }
    }
}