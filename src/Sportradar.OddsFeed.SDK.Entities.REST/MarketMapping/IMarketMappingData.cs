/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.MarketMapping
{
    /// <summary>
    /// Represents mapping information used to map markets and outright to the sport / producer to which they belong
    /// </summary>
    public interface IMarketMappingData {

        /// <summary>
        /// Gets the id of the producer to which the associated market / outright belongs to
        /// </summary>
        [Obsolete("Changed with ProducerIds property")]
        int ProducerId { get; }

        /// <summary>
        /// Gets the ids of the producers to which the associated market / outright belongs to
        /// </summary>
        /// <value>The producer ids</value>
        IEnumerable<int> ProducerIds { get; }

        /// <summary>
        /// Gets the id of the sport to which the associated market / outright belongs to
        /// </summary>
        URN SportId { get; }

        /// <summary>
        /// Gets the id of the market associated with the current instance
        /// </summary>
        string MarketId { get; }

        /// <summary>
        /// Gets the market type identifier
        /// </summary>
        int MarketTypeId { get; }

        /// <summary>
        /// Gets the market sub type identifier
        /// </summary>
        int? MarketSubTypeId { get; }

        /// <summary>
        /// Gets the special odds value template value
        /// </summary>
        string SovTemplate { get; }

        /// <summary>
        /// Gets the valid_for value
        /// </summary>
        string ValidFor { get; }

        /// <summary>
        /// Gets the outcome mappings
        /// </summary>
        IEnumerable<IOutcomeMappingData> OutcomeMappings { get; }

        /// <summary>
        /// Determines whether the current mapping can map market with provided specifiers associated with provided producer and sport
        /// </summary>
        /// <param name="producer">The <see cref="IProducer"/> associated with the market</param>
        /// <param name="sportId">The <see cref="URN"/> specifying the sport associated with the market</param>
        /// <param name="specifiers">The market specifiers</param>
        /// <returns>True if the current mapping can be used to map the specified market. False otherwise</returns>
        /// <exception cref="InvalidOperationException">The provided specifiers are not valid</exception>
        bool CanMap(IProducer producer, URN sportId, IReadOnlyDictionary<string, string> specifiers);
    }
}