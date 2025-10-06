// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Entities.Rest.MarketMapping;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.InternalEntities
{
    internal class MarketMapping : IMarketMappingData
    {
        /// <summary>
        /// The <see cref="IMappingValidator"/> used to determine whether the current mapping can map specific market
        /// or a null reference if no restrictions apply to this mapping
        /// </summary>
        private readonly IMappingValidator _validator;

        /// <summary>
        /// Gets the ids of the producers to which the associated market / outright belongs to
        /// </summary>
        /// <value>The producer ids</value>
        public IEnumerable<int> ProducerIds { get; }

        /// <summary>
        /// Gets the id of the sport to which the associated market / outright belongs to
        /// </summary>
        public Urn SportId { get; }

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
            Guard.Argument(cacheItem, nameof(cacheItem)).NotNull();

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
        /// <param name="producerId">The id of the <see cref="IProducer" /> associated with the market</param>
        /// <param name="sportId">The <see cref="Urn" /> specifying the sport associated with the market</param>
        /// <param name="specifiers">The market specifiers</param>
        /// <returns>True if the current mapping can be used to map the specified market. False otherwise</returns>
        /// <exception cref="InvalidOperationException">The provided specifiers are not valid</exception>
        public bool CanMap(int producerId, Urn sportId, IReadOnlyDictionary<string, string> specifiers)
        {
            if (!ProducerIds.Contains(producerId) || (SportId != null && !SportId.Equals(sportId)))
            {
                return false;
            }

            return _validator?.Validate(specifiers) ?? true;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"MarketId={MarketId}, SportId={SportId}, ProducersIds={string.Join(",", ProducerIds)}, SovTemplate={SovTemplate}, ValidFor={ValidFor}, TypeId={MarketTypeId}, SubTypeId={MarketSubTypeId}";
        }
    }
}
