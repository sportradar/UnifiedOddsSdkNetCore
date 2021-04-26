/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.REST.MarketMapping
{
    /// <summary>
    /// Represents mapping information used to map market outcomes
    /// </summary>
    public interface IOutcomeMappingData
    {
        /// <summary>
        /// Gets the id of the outcome
        /// </summary>
        string OutcomeId { get; }

        /// <summary>
        /// Gets the producer outcome identifier
        /// </summary>
        string ProducerOutcomeId { get; }

        /// <summary>
        /// Gets the name of the producer outcome in specified language
        /// </summary>
        string GetProducerOutcomeName(CultureInfo culture);

        /// <summary>
        /// Gets the mapped market identifier
        /// </summary>
        /// <value>The mapped market identifier</value>
        string MarketId { get; }
    }
}
