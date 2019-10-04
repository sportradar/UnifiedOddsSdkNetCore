/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.InternalEntities;
using Sportradar.OddsFeed.SDK.Entities.REST.Market;
using Sportradar.OddsFeed.SDK.Entities.REST.MarketMapping;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// A <see cref="IMarketDescriptionCache"/> implementation which provides access to correct cache by examining the market specifiers
    /// </summary>
    /// <seealso cref="IMarketDescriptionCache" />
    public class MarketCacheProvider : IMarketCacheProvider
    {
        private readonly ILog _executionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(MarketCacheProvider));

        /// <summary>
        /// A <see cref="IMarketDescriptionCache"/> used to cache market descriptors for invariant markets
        /// </summary>
        private readonly IMarketDescriptionCache _invariantMarketsCache;

        /// <summary>
        /// A <see cref="IMarketDescriptionCache"/> used to cache market descriptors for variant markets
        /// </summary>
        private readonly IMarketDescriptionCache _variantMarketsCache;

        /// <summary>
        /// A <see cref="IVariantDescriptionCache"/> used to cache variant descriptions
        /// </summary>
        private readonly IVariantDescriptionCache _variantDescriptionListCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketCacheProvider"/> class
        /// </summary>
        /// <param name="invariantMarketsCache">A <see cref="IMarketDescriptionCache"/> used to cache market descriptors for invariant markets</param>
        /// <param name="variantMarketsCache">A <see cref="IMarketDescriptionCache"/> used to cache market descriptors for variant markets</param>
        /// <param name="variantDescriptionListCache">A <see cref="IVariantDescriptionCache"/> used to cache variant descriptions</param>
        public MarketCacheProvider(IMarketDescriptionCache invariantMarketsCache,
                                   IMarketDescriptionCache variantMarketsCache,
                                   IVariantDescriptionCache variantDescriptionListCache)
        {
            Contract.Requires(invariantMarketsCache != null);
            Contract.Requires(variantMarketsCache != null);
            Contract.Requires(variantDescriptionListCache != null);

            _invariantMarketsCache = invariantMarketsCache;
            _variantMarketsCache = variantMarketsCache;
            _variantDescriptionListCache = variantDescriptionListCache;
        }

        /// <summary>
        /// Defines object invariants as required by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_invariantMarketsCache != null);
            Contract.Invariant(_variantMarketsCache != null);
            Contract.Invariant(_variantDescriptionListCache != null);
        }

        /// <summary>
        /// Gets a <see cref="IMarketDescription" /> instance for the market specified by <code>id</code> and <code>specifiers</code>
        /// </summary>
        /// <param name="marketId">The market identifier</param>
        /// <param name="specifiers">A dictionary specifying market specifiers or a null reference if market is invariant</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying required translations</param>
        /// <param name="fetchVariantDescriptions"></param>
        /// <returns>A <see cref="IMarketDescription" /> instance describing the specified markets</returns>
        /// <exception cref="CacheItemNotFoundException">The requested key was not found in the cache and could not be loaded</exception>
        public async Task<IMarketDescription> GetMarketDescriptionAsync(int marketId, IReadOnlyDictionary<string, string> specifiers, IEnumerable<CultureInfo> cultures, bool fetchVariantDescriptions)
        {
            IMarketDescription marketDescriptor;
            var cultureInfos = cultures as IList<CultureInfo> ?? cultures.ToList();
            try
            {
                //_executionLog.Debug($"Fetching invariant market description for id={marketId} and langs: [{string.Join(",", cultureInfos.Select(s => s.TwoLetterISOLanguageName))}].");
                marketDescriptor = await _invariantMarketsCache.GetMarketDescriptionAsync(marketId, null, cultureInfos).ConfigureAwait(false);
                //_executionLog.Debug($"Fetching invariant market description for id={marketId} and langs: [{string.Join(",", cultureInfos.Select(s => s.TwoLetterISOLanguageName))}] COMPLETED.");
            }
            catch (Exception e)
            {
                throw new CacheItemNotFoundException($"Market description with market id {marketId} could not be found", marketId.ToString(), e);
            }

            string variantValue = null;

            specifiers?.TryGetValue(SdkInfo.VariantDescriptionName, out variantValue);

            // case 1: if its not a variant market, return the static market descriptor as is
            if (string.IsNullOrEmpty(variantValue))
            {
                return marketDescriptor;
            }

            if (fetchVariantDescriptions)
            {
                // case 2: defined/known dynamic variant market => (pre:outcometext market) || (market is player props)
                if (IsMarketOutcomeText(marketDescriptor) || IsMarketPlayerProps(marketDescriptor))
                {
                    return await ProvideDynamicVariantEndpointMarketAsync(marketId, cultureInfos, marketDescriptor, variantValue).ConfigureAwait(false);
                }

                // case 3: "normal" variant market available on the full variant market list (static)
                var marketDesc = await ProvideFullVariantListEndpointMarketAsync(marketId, cultureInfos, marketDescriptor, variantValue).ConfigureAwait(false);
                if (marketDesc == null)
                {
                    // case 4: dynamic market which is not defined
                    marketDesc = await ProvideDynamicVariantEndpointMarketAsync(marketId, cultureInfos, marketDescriptor, variantValue).ConfigureAwait(false);
                }

                if (marketDesc?.Mappings != null && marketDesc.Mappings.Any())
                {
                    var marketDescImpl = (MarketDescription) marketDesc;
                    marketDescImpl.Mappings = marketDescImpl.Mappings.Where(s => s.MarketId.Equals(marketId.ToString()) || s.MarketId.StartsWith($"{marketId}:")).ToList();
                    marketDesc = marketDescImpl;
                }

                return marketDesc;
            }

            return marketDescriptor;
        }

        private async Task<IMarketDescription> ProvideFullVariantListEndpointMarketAsync(int marketId, IList<CultureInfo> locales, IMarketDescription marketDescriptor, string variantValue)
        {
            try
            {
                var variantDescription = await _variantDescriptionListCache.GetVariantDescriptorAsync(variantValue, locales).ConfigureAwait(false);

                if (variantDescription == null)
                {
                    return null;
                }

                ((MarketDescription) marketDescriptor).SetOutcomes(variantDescription.Outcomes as IReadOnlyCollection<IOutcomeDescription>);
                ((MarketDescription) marketDescriptor).SetMappings(variantDescription.Mappings as IReadOnlyCollection<IMarketMappingData>);
                var variantCI = ((VariantDescription) variantDescription).VariantDescriptionCacheItem;
                ((MarketDescription)marketDescriptor).SetFetchInfo(variantCI.SourceCache, variantCI.LastDataReceived);

                return marketDescriptor;
            }
            catch (Exception e)
            {
                var langs = string.Join(",", locales.Select(s=>s.TwoLetterISOLanguageName));
                _executionLog.Warn($"There was an error providing the variant market description -> marketId:{marketId}, variantValue: {variantValue}, locales: [{langs}]", e);
            }
            return null;
        }

        private async Task<IMarketDescription> ProvideDynamicVariantEndpointMarketAsync(int marketId, IList<CultureInfo> locales, IMarketDescription marketDescriptor, string variantValue)
        {
            IMarketDescription variantDescriptor = null;
            try
            {
                variantDescriptor = await _variantMarketsCache.GetMarketDescriptionAsync(marketId, variantValue, locales).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var langs = string.Join(",", locales.Select(s => s.TwoLetterISOLanguageName));
                _executionLog.Warn($"There was an error providing the explicit variant market description -> marketId:{marketId}, variantValue: {variantValue}, locales: [{langs}]", e);
            }

            return variantDescriptor ?? marketDescriptor;
        }

        private static bool IsMarketPlayerProps(IMarketDescription marketDescriptor)
        {
            return marketDescriptor?.Groups != null && marketDescriptor.Groups.Contains(SdkInfo.PlayerPropsMarketGroup);
        }

        private static bool IsMarketOutcomeText(IMarketDescription marketDescriptor)
        {
            return !string.IsNullOrEmpty(marketDescriptor?.OutcomeType) && marketDescriptor.OutcomeType.Equals(SdkInfo.FreeTextVariantValue);
        }

        /// <summary>
        /// Reload data for market descriptions
        /// </summary>
        /// <param name="marketId">The market identifier</param>
        /// <param name="specifiers">A dictionary specifying market specifiers or a null reference if market is invariant</param>
        /// <param name="sourceCache">The source cache <see cref="MarketDescriptionCacheItem"/> belongs to</param>
        /// <returns>True if succeeded, false otherwise</returns>
        public async Task<bool> ReloadMarketDescriptionAsync(int marketId, IReadOnlyDictionary<string, string> specifiers, string sourceCache)
        {
            if (string.IsNullOrEmpty(sourceCache))
            {
                _executionLog.Warn($"Calling ReloadMarketDescriptionAsync without sourceCache. (marketId={marketId})");
                return false;
            }
            try
            {

                if (sourceCache.Equals("InvariantMarketDescriptionsCache", StringComparison.InvariantCultureIgnoreCase))
                {
                    _executionLog.Debug("Reloading invariant market description list");
                    return await _invariantMarketsCache.LoadMarketDescriptionsAsync().ConfigureAwait(false);
                }
                if (sourceCache.Equals("VariantDescriptionListCache", StringComparison.InvariantCultureIgnoreCase))
                {
                    _executionLog.Debug("Reloading variant market description list");
                    return await _variantDescriptionListCache.LoadMarketDescriptionsAsync().ConfigureAwait(false);
                }
                if (sourceCache.Equals("VariantMarketDescriptionCache", StringComparison.InvariantCultureIgnoreCase))
                {
                    string variantValue = null;
                    specifiers?.TryGetValue(SdkInfo.VariantDescriptionName, out variantValue);
                    _executionLog.Debug($"Reloading variant market description for market={marketId} and variant={variantValue}");
                    var variantMarketDescriptionCache = (VariantMarketDescriptionCache) _variantMarketsCache;
                    variantMarketDescriptionCache.CacheDeleteItem(VariantMarketDescriptionCache.GetCacheKey(marketId, variantValue),
                                                                  CacheItemType.MarketDescription);
                    return true;
                }
            }
            catch (Exception e)
            {
                _executionLog.Warn("Error reloading market description(s).", e);
            }
            _executionLog.Warn($"Calling ReloadMarketDescriptionAsync with unknown sourceCache={sourceCache}. (marketId={marketId})");
            return false;
        }
    }
}