/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Castle.Core.Internal;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Entities.REST.Market;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a outcome definition
    /// </summary>
    internal class OutcomeDefinition : IOutcomeDefinition
    {
        private readonly IMarketCacheProvider _marketCacheProvider;
        private readonly int _marketId;
        private readonly string _outcomeId;
        private readonly IReadOnlyDictionary<string, string> _specifiers;
        private readonly IReadOnlyCollection<CultureInfo> _cultures;
        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;
        private readonly IDictionary<CultureInfo, string> _names = new Dictionary<CultureInfo, string>();
        private readonly object _lock = new object();

        internal OutcomeDefinition(int marketId,
                                   string outcomeId,
                                   IMarketCacheProvider marketCacheProvider,
                                   IReadOnlyDictionary<string, string> specifiers,
                                   IEnumerable<CultureInfo> cultures,
                                   ExceptionHandlingStrategy exceptionHandlingStrategy)
        {
            Guard.Argument(marketId, nameof(marketId)).Positive();
            Guard.Argument(cultures, nameof(cultures)).NotNull();

            _marketCacheProvider = marketCacheProvider;
            _marketId = marketId;
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

            lock (_lock)
            {
                if (_names.Any())
                {
                    return _names.ContainsKey(culture)
                        ? _names[culture]
                        : null;
                }
                try
                {
                    var marketDescription = _marketCacheProvider.GetMarketDescriptionAsync(_marketId, _specifiers, _cultures, true).GetAwaiter().GetResult();
                    if (marketDescription?.Outcomes == null || !marketDescription.Outcomes.Any())
                    {
                        return null;
                    }

                    var outcomeDescription = marketDescription.Outcomes.FirstOrDefault(s => s.Id.Equals(_outcomeId, StringComparison.InvariantCultureIgnoreCase));
                    if (outcomeDescription == null)
                    {
                        if (!string.IsNullOrEmpty(marketDescription.OutcomeType)
                            &&
                            (marketDescription.OutcomeType.Equals(SdkInfo.CompetitorsMarketOutcomeType)
                             || marketDescription.OutcomeType.Equals(SdkInfo.CompetitorMarketOutcomeType)
                             || marketDescription.OutcomeType.Equals(SdkInfo.PlayerMarketOutcomeType)))
                        {
                            foreach (var cultureInfo in _cultures)
                            {
                                _names[cultureInfo] = _outcomeId;
                            }

                            return _outcomeId;
                        }

                        var outcomesString = GetOutcomes(marketDescription.Outcomes, culture);
                        throw new CacheItemNotFoundException($"OutcomeDescription in marketDescription for market={_marketId} not found. Existing outcomes: {outcomesString}", _outcomeId, null);
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
                        throw new CacheItemNotFoundException($"OutcomeDescription in marketDescription for market={_marketId} not found. Could not provide the requested translated name ({culture.TwoLetterISOLanguageName})", _outcomeId, e);
                    }
                }
                catch (Exception ex)
                {
                    if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                    {
                        throw new CacheItemNotFoundException($"OutcomeDescription in marketDescription for market={_marketId} could not provide the requested translated name ({culture.TwoLetterISOLanguageName})", _outcomeId, ex);
                    }
                }
            }

            return _names.ContainsKey(culture)
                       ? _names[culture]
                       : null;
        }

        private static string GetOutcomes(IEnumerable<IOutcomeDescription> outcomes, CultureInfo culture)
        {
            var outcomeDescriptions = outcomes.ToList();
            if (outcomeDescriptions.IsNullOrEmpty())
            {
                return null;
            }

            return string.Join(",", outcomeDescriptions.Select(s => $"{s.Id}={s.GetName(culture)}"));
        }
    }
}
