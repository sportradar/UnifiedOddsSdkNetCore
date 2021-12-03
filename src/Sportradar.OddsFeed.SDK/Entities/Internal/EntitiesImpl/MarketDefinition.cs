/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Castle.Core.Internal;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
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
        private IMarketDescription _marketDescription;

        /// <summary>
        /// The market identifier
        /// </summary>
        private readonly int _marketId;

        /// <summary>
        /// The associated event sport identifier
        /// </summary>
        private readonly URN _sportId;

        /// <summary>
        /// The id of the producer which generated the associated market
        /// </summary>
        private readonly int _producerId;

        /// <summary>
        /// The associated market specifiers
        /// </summary>
        private readonly IReadOnlyDictionary<string, string> _specifiers;

        private readonly IEnumerable<CultureInfo> _cultures;

        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;

        private readonly IMarketCacheProvider _marketCacheProvider;

        private readonly object _lock = new object();

        /// <summary>
        /// Constructs a new market definition. The market definition represents additional market data which can be used for more advanced use cases
        /// </summary>
        /// <param name="marketId">The id of the market</param>
        /// <param name="marketCacheProvider">The market cache provider used to retrieve name templates</param>
        /// <param name="sportId">The associated event sport identifier</param>
        /// <param name="producerId">The producer of the producer which generated the market</param>
        /// <param name="specifiers">The associated market specifiers</param>
        /// <param name="cultures">The cultures</param>
        /// <param name="exceptionHandlingStrategy">The exception handling strategy</param>
        internal MarketDefinition(int marketId, 
            IMarketCacheProvider marketCacheProvider, 
            URN sportId, 
            int producerId, 
            IReadOnlyDictionary<string, string> specifiers,
            IEnumerable<CultureInfo> cultures,
            ExceptionHandlingStrategy exceptionHandlingStrategy)
        {
            _marketId = marketId;
            _sportId = sportId;
            _producerId = producerId;
            _specifiers = specifiers;
            _cultures = cultures;
            _exceptionHandlingStrategy = exceptionHandlingStrategy;
            _marketCacheProvider = marketCacheProvider;
        }

        /// <summary>
        /// Returns the unmodified market name template
        /// </summary>
        /// <param name="culture">The culture in which the name template should be provided</param>
        /// <returns>The unmodified market name template</returns>
        public string GetNameTemplate(CultureInfo culture)
        {
            //// name templates need to be always fetched from the cache because of the variant markets (they are not being fetched on market definition creation)
            //var marketDescription = _marketCacheProvider.GetMarketDescriptionAsync((int) _marketDescription.Id, _specifiers, new[] {culture}, true).Result;
            //return marketDescription?.GetName(culture);

            GetMarketDefinition();
            return _marketDescription?.GetName(culture);
        }

        /// <summary>
        /// Returns an indication of which kind of outcomes the associated market includes
        /// </summary>
        /// <returns>An indication of which kind of outcomes the associated market includes</returns>
        public string GetOutcomeType()
        {
            GetMarketDefinition();
            return _marketDescription?.OutcomeType;
        }

        /// <summary>
        /// Returns a list of groups to which the associated market belongs to
        /// </summary>
        /// <returns>a list of groups to which the associated market belongs to</returns>
        public IList<string> GetGroups()
        {
            GetMarketDefinition();
            return _marketDescription?.Groups == null || _marketDescription.Groups.IsNullOrEmpty()
                ? null
                : new ReadOnlyCollection<string>(_marketDescription.Groups.ToList());
        }

        /// <summary>
        /// Returns a dictionary of associated market attributes
        /// </summary>
        /// <returns>A dictionary of associated market attributes</returns>
        public IDictionary<string, string> GetAttributes()
        {
            GetMarketDefinition();
            return _marketDescription?.Attributes == null || _marketDescription.Attributes.IsNullOrEmpty()
                ? null
                : new ReadOnlyDictionary <string, string> (_marketDescription.Attributes.ToDictionary(k => k.Name, v => v.Description));
        }

        /// <summary>
        /// Returns a list of valid market mappings
        /// </summary>
        /// <returns>a list of valid market mappings</returns>
        public IEnumerable<IMarketMappingData> GetValidMappings()
        {
            GetMarketDefinition();
            return _producerId == 5 || _marketDescription?.Mappings == null || _marketDescription.Mappings.IsNullOrEmpty()
                ? Enumerable.Empty<IMarketMappingData>()
                : _marketDescription.Mappings.Where(m => m.CanMap(_producerId, _sportId, _specifiers));
        }

        /// <summary>
        /// Gets a <see cref="IMarketDefinition" /> associated with the provided data
        /// </summary>
        private void GetMarketDefinition()
        {
            // market descriptor should exist or else the market generation would fail
            // variant always null because we are interested only in the general market attributes

            if (_marketDescription != null)
            {
                return;
            }

            lock (_lock)
            {
                if (_marketDescription != null)
                {
                    return;
                }
                try
                {
                    _marketDescription = _marketCacheProvider.GetMarketDescriptionAsync(_marketId, _specifiers, _cultures, true).Result; // was false and true in GetNameTemplate
                }
                catch (CacheItemNotFoundException ci)
                {
                    if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                    {
                        throw new CacheItemNotFoundException($"Market description for market={_marketId}, sport={_sportId} and producer={_producerId} not found.", _marketId.ToString(), ci);
                    }
                }
                catch (Exception ex)
                {
                    if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                    {
                        throw new CacheItemNotFoundException($"Market description for market={_marketId} not found.", _marketId.ToString(), ex);
                    }
                }  
            }
        }
    }
}
