/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Entities.REST.Market;
using Sportradar.OddsFeed.SDK.Entities.REST.MarketMapping;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a market definition
    /// </summary>
    internal class MarketDefinition : IMarketDefinition
    {
        /// <summary>
        /// The associated market descriptor
        /// </summary>
        private readonly IMarketDescription _marketDescription;

        /// <summary>
        /// The associated event sport identifier
        /// </summary>
        private readonly URN _sportId;

        /// <summary>
        /// The producer which generated the associated market
        /// </summary>
        private readonly IProducer _producer;

        /// <summary>
        /// The associated market specifiers
        /// </summary>
        private readonly IReadOnlyDictionary<string, string> _specifiers;

        private readonly IMarketCacheProvider _marketCacheProvider;

        /// <summary>
        /// Constructs a new market definition. The market definition represents additional market data which can be used for more advanced use cases
        /// </summary>
        /// <param name="marketDescription">The associated market descriptor</param>
        /// <param name="marketCacheProvider">The market cache provider used to retrieve name templates</param>
        /// <param name="sportId">The associated event sport identifier</param>
        /// <param name="producer">The producer which generated the market</param>
        /// <param name="specifiers">The associated market specifiers</param>
        internal MarketDefinition(IMarketDescription marketDescription, IMarketCacheProvider marketCacheProvider, URN sportId, IProducer producer, IReadOnlyDictionary<string, string> specifiers)
        {
            _marketDescription = marketDescription;
            _sportId = sportId;
            _producer = producer;
            _specifiers = specifiers;
            _marketCacheProvider = marketCacheProvider;
        }

        /// <summary>
        /// Returns the unmodified market name template
        /// </summary>
        /// <param name="culture">The culture in which the name template should be provided</param>
        /// <returns>The unmodified market name template</returns>
        public string GetNameTemplate(CultureInfo culture)
        {
            // name templates need to be always fetched from the cache because of the variant markets (they are not being fetched on market definition creation)
            //string variant = null;
            //_specifiers?.TryGetValue("variant", out variant);

            var marketDescription = _marketCacheProvider.GetMarketDescriptionAsync((int) _marketDescription.Id, _specifiers, new[] {culture}, true).Result;

            return marketDescription?.GetName(culture);
        }

        /// <summary>
        /// Returns an indication of which kind of outcomes the associated market includes
        /// </summary>
        /// <returns>An indication of which kind of outcomes the associated market includes</returns>
        [Obsolete("Use OutcomeType")]
        public string GetIncludesOutcomesOfType()
        {
            return _marketDescription.IncludesOutcomesOfType;
        }

        /// <summary>
        /// Returns an indication of which kind of outcomes the associated market includes
        /// </summary>
        /// <returns>An indication of which kind of outcomes the associated market includes</returns>
        public string GetOutcomeType()
        {
            return _marketDescription.OutcomeType;
        }

        /// <summary>
        /// Returns a list of groups to which the associated market belongs to
        /// </summary>
        /// <returns>a list of groups to which the associated market belongs to</returns>
        public IList<string> GetGroups()
        {
            return _marketDescription.Groups == null
                ? null
                : new ReadOnlyCollection<string>(_marketDescription.Groups.ToList());
        }

        /// <summary>
        /// Returns a dictionary of associated market attributes
        /// </summary>
        /// <returns>A dictionary of associated market attributes</returns>
        public IDictionary<string, string> GetAttributes()
        {
            return _marketDescription.Attributes == null
                ? null
                : new ReadOnlyDictionary <string, string> (_marketDescription.Attributes.ToDictionary(k => k.Name, v => v.Description));
        }

        /// <summary>
        /// Returns a list of valid market mappings
        /// </summary>
        /// <returns>a list of valid market mappings</returns>
        public IEnumerable<IMarketMappingData> GetValidMappings()
        {
            return _producer.Id == 5 || _marketDescription?.Mappings == null
                ? Enumerable.Empty<IMarketMappingData>()
                : _marketDescription.Mappings.Where(m => m.CanMap(_producer, _sportId, _specifiers));
        }
    }
}
