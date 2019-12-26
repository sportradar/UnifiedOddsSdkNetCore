/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Entities.REST.Market;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a outcome definition
    /// </summary>
    internal class OutcomeDefinition : IOutcomeDefinition
    {
        /// <summary>
        /// The associated market descriptor
        /// </summary>
        private readonly IMarketDescription _marketDescription;

        private readonly IMarketCacheProvider _marketCacheProvider;
        private readonly string _outcomeId;
        private readonly IReadOnlyDictionary<string, string> _specifiers;
        private readonly IReadOnlyCollection<CultureInfo> _cultures;
        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;
        private readonly IDictionary<CultureInfo, string> _names = new Dictionary<CultureInfo, string>();

        /// <summary>
        /// Constructs a new market definition. The market definition represents additional market data which can be used for more advanced use cases
        /// </summary>
        /// <param name="marketDescription">The associated market descriptor</param>
        /// <param name="outcomeDescription">The associated outcome descriptor</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages the current instance supports</param>
        internal OutcomeDefinition(IMarketDescription marketDescription, IOutcomeDescription outcomeDescription, IEnumerable<CultureInfo> cultures)
        {
            Guard.Argument(cultures, nameof(cultures)).NotNull();

            _marketDescription = marketDescription;

            if (outcomeDescription != null)
            {
                _outcomeId = outcomeDescription.Id;
                foreach (var culture in cultures)
                {
                    _names[culture] = outcomeDescription.GetName(culture);
                }
            }
        }

        internal OutcomeDefinition(IMarketDescription marketDescription,
                                   string outcomeId,
                                   IMarketCacheProvider marketCacheProvider,
                                   IReadOnlyDictionary<string, string> specifiers,
                                   IEnumerable<CultureInfo> cultures,
                                   ExceptionHandlingStrategy exceptionHandlingStrategy)
        {
            Guard.Argument(cultures, nameof(cultures)).NotNull();

            _marketDescription = marketDescription;
            _marketCacheProvider = marketCacheProvider;
            _outcomeId = outcomeId;
            _specifiers = specifiers;
            _cultures = cultures as IReadOnlyCollection<CultureInfo>;
            _exceptionHandlingStrategy = exceptionHandlingStrategy;
        }

        /// <summary>
        /// Returns the unmodified market name template
        /// </summary>
        /// <param name="culture">The culture in which the name template should be provided</param>
        /// <returns>The unmodified market name template</returns>
        public string GetNameTemplate(CultureInfo culture)
        {
            if (_names.Any())
            {
                return _names.ContainsKey(culture)
                           ? _names[culture]
                           : null;
            }

            if (string.IsNullOrEmpty(_outcomeId))
            {
                return null;
            }

            try
            {
                var marketDescription = _marketCacheProvider.GetMarketDescriptionAsync((int) _marketDescription.Id, _specifiers, _cultures, true).Result;
                if (marketDescription?.Outcomes == null || !marketDescription.Outcomes.Any())
                {
                    return null;
                }

                var outcomeDescription = marketDescription.Outcomes.FirstOrDefault(s => s.Id.Equals(_outcomeId, StringComparison.InvariantCultureIgnoreCase));
                if (outcomeDescription == null)
                {
                    throw new CacheItemNotFoundException("Item not found", nameof(_outcomeId), null);
                }
                foreach (var cultureInfo in _cultures)
                {
                    _names[cultureInfo] = outcomeDescription.GetName(cultureInfo);
                }
            }
            catch (CacheItemNotFoundException e)
            {
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                {
                    throw new CacheItemNotFoundException("Could not provide the requested translated name", nameof(_outcomeId), e);
                }
            }

            return _names.ContainsKey(culture)
                       ? _names[culture]
                       : null;
        }
    }
}
