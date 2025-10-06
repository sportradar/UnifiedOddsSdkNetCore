// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Entities.Rest.Market;
using Sportradar.OddsFeed.SDK.Entities.Rest.MarketMapping;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Managers
{
    /// <summary>
    /// The run-time implementation of the <see cref="IMarketDescriptionManager"/> interface
    /// </summary>
    internal class MarketDescriptionManager : IMarketDescriptionManager
    {
        private readonly ILogger _executionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(MarketDescriptionManager));

        private readonly IUofConfiguration _config;
        private readonly IMarketCacheProvider _marketCacheProvider;
        private readonly InvariantMarketDescriptionCache _invariantMarketDescriptionCache;
        private readonly IVariantDescriptionsCache _variantDescriptionListCache;
        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;
        private readonly IMarketDescriptionCache _variantDescriptionCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketDescriptionManager"/> class
        /// </summary>
        /// <param name="config">The <see cref="IUofConfiguration"/> representing feed configuration</param>
        /// <param name="marketCacheProvider">A <see cref="IMarketCacheProvider"/> used to get market descriptions</param>
        /// <param name="invariantMarketDescriptionCache">A <see cref="IMarketDescriptionCache"/> used to get invariant market descriptions</param>
        /// <param name="variantDescriptionListCache">A <see cref="IVariantDescriptionsCache"/> used to reload variant market descriptions</param>
        /// <param name="variantDescriptionCache">A <see cref="IMarketDescriptionCache"/> used to access market variant cache (singles)</param>
        public MarketDescriptionManager(IUofConfiguration config,
                                        IMarketCacheProvider marketCacheProvider,
                                        IMarketDescriptionsCache invariantMarketDescriptionCache,
                                        IVariantDescriptionsCache variantDescriptionListCache,
                                        IMarketDescriptionCache variantDescriptionCache)
        {
            if (invariantMarketDescriptionCache == null)
            {
                throw new ArgumentNullException(nameof(invariantMarketDescriptionCache));
            }
            if (!(invariantMarketDescriptionCache is InvariantMarketDescriptionCache cache))
            {
                throw new ArgumentException("Missing invariant market description cache", nameof(invariantMarketDescriptionCache));
            }

            _config = config ?? throw new ArgumentNullException(nameof(config));
            _marketCacheProvider = marketCacheProvider ?? throw new ArgumentNullException(nameof(marketCacheProvider));
            _invariantMarketDescriptionCache = cache;
            _variantDescriptionListCache = variantDescriptionListCache;
            _exceptionHandlingStrategy = config.ExceptionHandlingStrategy;
            _variantDescriptionCache = variantDescriptionCache ?? throw new ArgumentNullException(nameof(variantDescriptionCache));
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{IMarketDescription}"/> of all available static market descriptions
        /// </summary>
        /// <returns>A <see cref="IEnumerable{IMarketDescription}"/> of available static market descriptions</returns>
        public async Task<IEnumerable<IMarketDescription>> GetMarketDescriptionsAsync(CultureInfo culture = null)
        {
            try
            {
                if (culture == null)
                {
                    culture = _config.DefaultLanguage;
                }
                return await _invariantMarketDescriptionCache.GetAllInvariantMarketDescriptionsAsync(new[] { culture }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw new ObjectNotFoundException($"Market descriptions({culture}) could not be provided", ex);
                }

                _executionLog.LogWarning(ex, "Market descriptions for the {Language} language could not be provided", culture?.TwoLetterISOLanguageName);
                return null;
            }
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{IMarketMappingData}"/> of available mappings for the provided marketId/producer combination
        /// </summary>
        /// <param name="marketId">The id of the market for which you need the mapping</param>
        /// <param name="producer">The <see cref="IProducer"/> for which you need the mapping</param>
        /// <returns>A <see cref="IEnumerable{IMarketMappingData}"/> of available mappings for the provided marketId/producer combination</returns>
        public async Task<IEnumerable<IMarketMappingData>> GetMarketMappingAsync(int marketId, IProducer producer)
        {
            return await GetMarketMappingAsync(marketId, null, producer).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{IMarketMappingData}"/> of available mappings for the provided marketId/producer combination
        /// </summary>
        /// <param name="marketId">The id of the market for which you need the mapping</param>
        /// <param name="specifiers">The associated market specifiers</param>
        /// <param name="producer">The <see cref="IProducer"/> for which you need the mapping</param>
        /// <returns>A <see cref="IEnumerable{IMarketMappingData}"/> of available mappings for the provided marketId/producer combination</returns>
        public async Task<IEnumerable<IMarketMappingData>> GetMarketMappingAsync(int marketId, IReadOnlyDictionary<string, string> specifiers, IProducer producer)
        {
            IMarketDescription marketDescriptor;
            try
            {
                marketDescriptor = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, new[] { _config.DefaultLanguage }, false).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var specifiersMessage = specifiers != null
                                            ? $", specifiers: {string.Join("; ", specifiers)}"
                                            : string.Empty;
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw new ObjectNotFoundException($"Market mappings for {marketId} could not be provided{specifiersMessage}", ex);
                }

                _executionLog.LogWarning(ex, "Market mappings for the marketId: {MdId} could not be provided{MdSpecifiers}", marketId, specifiersMessage);
                return null;
            }

            return marketDescriptor.Mappings?.Where(m => m.ProducerIds.Contains(producer.Id)).ToList() ?? Enumerable.Empty<IMarketMappingData>();
        }

        /// <summary>
        /// Asynchronously loads the invariant and variant list of market descriptions from the Sports API
        /// </summary>
        /// <remarks>To be used when manually changed market data via betradar control</remarks>
        /// <returns>Returns true if the action succeeded</returns>
        public async Task<bool> LoadMarketDescriptionsAsync()
        {
            var tasks = new List<Task<bool>>
                            {
                                _variantDescriptionListCache.LoadMarketDescriptionsAsync(),
                                _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync()
                            };

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return tasks.All(a => a.GetAwaiter().GetResult());
        }

        /// <summary>
        /// Deletes the variant market description from cache
        /// </summary>
        /// <param name="marketId">The market identifier</param>
        /// <param name="variantValue">The variant value</param>
        public void DeleteVariantMarketDescriptionFromCache(int marketId, string variantValue)
        {
            _executionLog.LogInformation("Invokes DeleteVariantMarketDescriptionFromCache for market {MdId} and variant {MdVariant}", marketId, variantValue);
            var cacheId = VariantMarketDescriptionCache.GenerateCacheKey(marketId, variantValue);
            ((SdkCache)_variantDescriptionCache).CacheDeleteItem(cacheId, CacheItemType.MarketDescription);
        }
    }
}
