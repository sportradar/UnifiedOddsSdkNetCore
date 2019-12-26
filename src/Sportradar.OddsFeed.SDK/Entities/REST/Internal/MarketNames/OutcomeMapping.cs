/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST.MarketMapping;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// A implementation of outcome mapping for the market
    /// </summary>
    /// <seealso cref="IOutcomeMapping" />
    public class OutcomeMapping : IOutcomeMapping
    {
        private readonly IReadOnlyDictionary<CultureInfo, string> _producerOutcomeNames;

        /// <summary>
        /// Gets the identifier
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the name of the outcome in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language in which to get the name</param>
        /// <returns>The name in the specific language</returns>
        public string GetName(CultureInfo culture)
        {
            if (_producerOutcomeNames == null)
            {
                return null;
            }
            string name;
            return _producerOutcomeNames.TryGetValue(culture, out name) ? name : null;
        }

        /// <summary>
        /// Gets the id of the mapped market
        /// </summary>
        /// <value>The id of the mapped market</value>
        public string MarketId { get; }

        /// <summary>
        /// Constructs the <see cref="IOutcomeMapping"/>
        /// </summary>
        /// <param name="id">The Id used for the construction</param>
        /// <param name="names">The translatable name used for the construction</param>
        /// <param name="marketId">The id of the mapped market</param>
        public OutcomeMapping(string id, IDictionary<CultureInfo, string> names, string marketId)
        {
            Id = id;
            _producerOutcomeNames = names as IReadOnlyDictionary<CultureInfo, string>;
            MarketId = marketId;
        }
    }
}
