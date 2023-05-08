/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics.Health;
using Castle.Core.Internal;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.InternalEntities;
using Sportradar.OddsFeed.SDK.Entities.REST.Market;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// A <see cref="IMarketDescriptionCache" /> implementation used to store market descriptors for variant markets (single - dynamic)
    /// </summary>
    /// <seealso cref="IDisposable" />
    /// <seealso cref="IMarketDescriptionCache" />
    internal class VariantMarketDescriptionCache : SdkCache, IMarketDescriptionCache
    {
        /// <summary>
        /// A <see cref="MemoryCache"/> used to store market descriptors
        /// </summary>
        private readonly MemoryCache _cache;

        /// <summary>
        /// The <see cref="IDataRouterManager"/> used to obtain data via REST request
        /// </summary>
        private readonly IDataRouterManager _dataRouterManager;

        /// <summary>
        /// The <see cref="IMappingValidatorFactory"/> used to construct <see cref="IMappingValidator"/> instances for market mappings
        /// </summary>
        private readonly IMappingValidatorFactory _mappingValidatorFactory;

        /// <summary>
        /// A <see cref="SemaphoreSlim"/> instance to synchronize access from multiple threads
        /// </summary>
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        /// <summary>
        /// A <see cref="SemaphoreSlim"/> used to synchronize merging on cache item
        /// </summary>
        private readonly SemaphoreSlim _semaphoreCacheMerge = new SemaphoreSlim(1);

        /// <summary>
        /// Value indicating whether the current instance was already disposed
        /// </summary>
        private bool _isDisposed;

        private readonly ConcurrentDictionary<string, DateTime> _fetchedVariants = new ConcurrentDictionary<string, DateTime>();

        private DateTime _lastTimeFetchedVariantsWereCleared = DateTime.Now;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariantMarketDescriptionCache"/> class
        /// </summary>
        /// <param name="cache">A <see cref="MemoryCache"/> used to store market descriptors</param>
        /// <param name="dataRouterManager">A <see cref="IDataRouterManager"/> used to fetch data</param>
        /// <param name="mappingValidatorFactory">A <see cref="IMappingValidatorFactory"/> used to construct <see cref="IMappingValidator"/> instances for market mappings</param>
        /// <param name="cacheManager">A <see cref="ICacheManager"/> used to interact among caches</param>
        public VariantMarketDescriptionCache(MemoryCache cache,
                                             IDataRouterManager dataRouterManager,
                                             IMappingValidatorFactory mappingValidatorFactory,
                                             ICacheManager cacheManager)
            : base(cacheManager)
        {
            Guard.Argument(cache, nameof(cache)).NotNull();
            Guard.Argument(dataRouterManager, nameof(dataRouterManager)).NotNull();
            Guard.Argument(mappingValidatorFactory, nameof(mappingValidatorFactory)).NotNull();

            _cache = cache;
            _dataRouterManager = dataRouterManager;
            _mappingValidatorFactory = mappingValidatorFactory;
        }

        /// <summary>
        /// Gets a cache key generated from the provided <c>id</c> and <c>variant</c>
        /// </summary>
        /// <param name="id">The id of the market</param>
        /// <param name="variant">The market variant</param>
        /// <returns>a cache key generated from the provided <c>id</c> and <c>variant</c></returns>
        public static string GetCacheKey(long id, string variant)
        {
            Guard.Argument(variant, nameof(variant)).NotNull().NotEmpty();

            return $"{id}_{variant}";
        }

        /// <summary>
        /// Gets the <see cref="MarketDescriptionCacheItem"/> specified by it's id from the local cache
        /// </summary>
        /// <param name="id">The id of the <see cref="MarketDescriptionCacheItem"/> to get</param>
        /// <param name="variant">A <see cref="string"/> specifying the variation of the associated market descriptor</param>
        /// <returns>The <see cref="MarketDescriptionCacheItem"/> retrieved from the cache or a null reference if item is not found</returns>
        private MarketDescriptionCacheItem GetItemFromCache(int id, string variant)
        {
            Guard.Argument(variant, nameof(variant)).NotNull().NotEmpty();

            _semaphoreCacheMerge.Wait();
            var cacheItem = _cache.GetCacheItem(GetCacheKey(id, variant));
            _semaphoreCacheMerge.ReleaseSafe();
            return (MarketDescriptionCacheItem)cacheItem?.Value;
        }

        /// <summary>
        /// Gets a <see cref="IEnumerable{CultureInfo}"/> containing <see cref="CultureInfo"/> instances from provided <c>requiredTranslations</c>
        /// which translations are not found in the provided <see cref="MarketDescriptionCacheItem"/>
        /// </summary>
        /// <param name="item">The <see cref="MarketDescriptionCacheItem"/> instance, or a null reference</param>
        /// <param name="requiredTranslations">The <see cref="IReadOnlyCollection{CultureInfo}"/> specifying the required languages</param>
        /// <returns>A <see cref="IEnumerable{CultureInfo}"/> containing missing translations or a null reference if none of the translations are missing</returns>
        private static IEnumerable<CultureInfo> GetMissingTranslations(MarketDescriptionCacheItem item, IReadOnlyCollection<CultureInfo> requiredTranslations)
        {
            Guard.Argument(requiredTranslations, nameof(requiredTranslations)).NotNull().NotEmpty();

            if (item == null)
            {
                return requiredTranslations;
            }

            var missingCultures = requiredTranslations.Where(c => !item.HasTranslationsFor(c)).ToList();

            return missingCultures.Any()
                       ? missingCultures
                       : new List<CultureInfo>();
        }

        /// <summary>
        /// Asynchronously gets the specified <see cref="MarketDescriptionCacheItem"/>. If the item is not found in local cache, it is fetched and stored / merged into cache
        /// </summary>
        /// <param name="id">The id of the market associated with the requested descriptor</param>
        /// <param name="variant">A variation of the market associated with the requested descriptor</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying the languages which the returned item must contain</param>
        /// <returns>A <see cref="Task"/> representing the async operation</returns>
        /// <exception cref="CommunicationException">An error occurred while accessing the remote party</exception>
        /// <exception cref="DeserializationException">An error occurred while deserializing fetched data</exception>
        /// <exception cref="FormatException">An error occurred while mapping deserialized entities</exception>
        private async Task<MarketDescriptionCacheItem> GetMarketInternal(int id, string variant, IReadOnlyCollection<CultureInfo> cultures)
        {
            Guard.Argument(variant, nameof(variant)).NotNull().NotEmpty();
            Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();

            var description = GetItemFromCache(id, variant);
            if (GetMissingTranslations(description, cultures).IsNullOrEmpty())
            {
                return description;
            }

            if (_isDisposed)
            {
                return null;
            }

            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);

                description = GetItemFromCache(id, variant);
                var missingLanguages = LanguageHelper.GetMissingCultures(cultures, description?.FetchedLanguages);

                if (!missingLanguages.Any())
                {
                    return description;
                }

                if (!IsFetchingAllowed(id, variant, missingLanguages))
                {
                    return description;
                }

                var cultureTaskDictionary = missingLanguages.ToDictionary(c => c, c => _dataRouterManager.GetVariantMarketDescriptionAsync(id, variant, c));
                await Task.WhenAll(cultureTaskDictionary.Values).ConfigureAwait(false);

                var cachedItem = _cache.GetCacheItem(GetCacheKey(id, variant));

                description = (MarketDescriptionCacheItem)cachedItem?.Value;

                //sometimes it may be null (aka Not Found), but we still do not to re-fetch immediately
                foreach (var cultureInfo in missingLanguages)
                {
                    _fetchedVariants[GetFetchedVariantsKey(id, variant, cultureInfo)] = DateTime.Now;
                }
            }
            finally
            {
                _semaphore.ReleaseSafe();
            }
            return description;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            if (disposing)
            {
                _semaphore.Dispose();
                _semaphoreCacheMerge.Dispose();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IMarketDescription" /> instance for the market specified by <c>id</c> and <c>specifiers</c>
        /// </summary>
        /// <param name="marketId">The market identifier</param>
        /// <param name="variant">A <see cref="string" /> specifying market variant or a null reference if market is invariant</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}" /> specifying required translations</param>
        /// <returns>A <see cref="Task{T}" /> representing the async retrieval operation</returns>
        /// <exception cref="CacheItemNotFoundException">The requested key was not found in the cache and could not be loaded</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S4457:Parameter validation in \"async\"/\"await\" methods should be wrapped", Justification = "Allowed")]
        public async Task<IMarketDescription> GetMarketDescriptionAsync(int marketId, string variant, IReadOnlyCollection<CultureInfo> cultures)
        {
            if (string.IsNullOrEmpty(variant))
            {
                throw new ArgumentException("Value cannot be a null reference or empty string", nameof(variant));
            }

            MarketDescriptionCacheItem cacheItem;
            try
            {
                cacheItem = await GetMarketInternal(marketId, variant, cultures).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is CommunicationException || ex is DeserializationException || ex is MappingException)
                {
                    throw new CacheItemNotFoundException("The requested key was not found in the cache", GetCacheKey(marketId, variant), ex);
                }
                throw;
            }

            return cacheItem == null
                ? null
                : new MarketDescription(cacheItem, cultures);
        }

        /// <summary>
        /// Asynchronously loads the list of market descriptions from the Sports API
        /// </summary>
        /// <returns>Returns true if the action succeeded</returns>
        public Task<bool> LoadMarketDescriptionsAsync()
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Updates cache item fetch time
        /// </summary>
        public void UpdateCacheItem(int marketId, string variantValue)
        {
            GetItemFromCache(marketId, variantValue)?.SetFetchInfo(null, DateTime.Now);
        }

        /// <summary>
        /// Registers the health check which will be periodically triggered
        /// </summary>
        public void RegisterHealthCheck()
        {
            //HealthChecks.RegisterHealthCheck("VariantMarketDescriptorCache", new Func<HealthCheckResult>(StartHealthCheck));
        }

        /// <summary>
        /// Starts the health check and returns <see cref="HealthCheckResult"/>
        /// </summary>
        public HealthCheckResult StartHealthCheck()
        {
            return _cache.Any() ? HealthCheckResult.Healthy($"Cache has {_cache.Count()} items.") : HealthCheckResult.Healthy("Cache is empty.");
        }

        /// <summary>
        /// Set the list of <see cref="DtoType"/> in the this cache
        /// </summary>
        public override void SetDtoTypes()
        {
            RegisteredDtoTypes = new List<DtoType>
                                 {
                                     DtoType.MarketDescription
                                 };
        }

        /// <summary>
        /// Deletes the item from cache
        /// </summary>
        /// <param name="id">A <see cref="URN" /> representing the id of the item in the cache to be deleted</param>
        /// <param name="cacheItemType">A cache item type</param>
        public override void CacheDeleteItem(URN id, CacheItemType cacheItemType)
        {
            if (id != null && (cacheItemType == CacheItemType.All || cacheItemType == CacheItemType.MarketDescription))
            {
                CacheLog.LogDebug($"Delete variant market: {id}");
                _cache.Remove(id.Id.ToString());
            }
        }

        /// <summary>
        /// Deletes the item from cache
        /// </summary>
        /// <param name="id">A string representing the id of the item in the cache to be deleted</param>
        /// <param name="cacheItemType">A cache item type</param>
        public override void CacheDeleteItem(string id, CacheItemType cacheItemType)
        {
            if (cacheItemType == CacheItemType.All || cacheItemType == CacheItemType.MarketDescription)
            {
                try
                {
                    foreach (var fetchedVariant in _fetchedVariants)
                    {
                        if (fetchedVariant.Key.StartsWith(id, StringComparison.InvariantCultureIgnoreCase))
                        {
                            _fetchedVariants.TryRemove(id, out _);
                        }
                    }
                }
                catch (Exception e)
                {
                    ExecutionLog.LogWarning(e, $"Error deleting fetchedVariants for {id}");
                }

                if (_cache.Contains(id))
                {
                    CacheLog.LogDebug($"Delete variant market: {id}");
                    _cache.Remove(id);
                }
            }
        }

        /// <summary>
        /// Does item exists in the cache
        /// </summary>
        /// <param name="id">A <see cref="URN" /> representing the id of the item to be checked</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        public override bool CacheHasItem(URN id, CacheItemType cacheItemType)
        {
            if (id != null && (cacheItemType == CacheItemType.All || cacheItemType == CacheItemType.MarketDescription))
            {
                return _cache.Contains(id.Id.ToString());
            }
            return false;
        }

        /// <summary>
        /// Does item exists in the cache
        /// </summary>
        /// <param name="id">A string representing the id of the item to be checked</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        public override bool CacheHasItem(string id, CacheItemType cacheItemType)
        {
            if (cacheItemType == CacheItemType.All || cacheItemType == CacheItemType.MarketDescription)
            {
                return _cache.Contains(id);
            }

            return false;
        }

        /// <inheritdoc />
        protected override async Task<bool> CacheAddDtoItemAsync(URN id, object item, CultureInfo culture, DtoType dtoType, ISportEventCI requester)
        {
            if (_isDisposed)
            {
                return false;
            }

            var saved = false;
            switch (dtoType)
            {
                case DtoType.Category:
                    break;
                case DtoType.Competitor:
                    break;
                case DtoType.CompetitorProfile:
                    break;
                case DtoType.SimpleTeamProfile:
                    break;
                case DtoType.Fixture:
                    break;
                case DtoType.MarketDescription:
                    if (item is MarketDescriptionDTO marketDescription)
                    {
                        await MergeAsync(culture, marketDescription).ConfigureAwait(false);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(MarketDescriptionDTO), item.GetType(), ExecutionLog);
                    }
                    break;
                case DtoType.MarketDescriptionList:
                    break;
                case DtoType.MatchSummary:
                    break;
                case DtoType.MatchTimeline:
                    break;
                case DtoType.PlayerProfile:
                    break;
                case DtoType.RaceSummary:
                    break;
                case DtoType.Sport:
                    break;
                case DtoType.SportList:
                    break;
                case DtoType.SportEventStatus:
                    break;
                case DtoType.SportEventSummary:
                    break;
                case DtoType.SportEventSummaryList:
                    break;
                case DtoType.Tournament:
                    break;
                case DtoType.TournamentInfo:
                    break;
                case DtoType.TournamentSeasons:
                    break;
                case DtoType.VariantDescription:
                    break;
                case DtoType.VariantDescriptionList:
                    break;
                case DtoType.Lottery:
                    break;
                case DtoType.LotteryDraw:
                    break;
                case DtoType.LotteryList:
                    break;
                case DtoType.BookingStatus:
                    break;
                case DtoType.SportCategories:
                    break;
                case DtoType.AvailableSelections:
                    break;
                case DtoType.TournamentInfoList:
                    break;
                case DtoType.PeriodSummary:
                    break;
                case DtoType.Calculation:
                    break;
                default:
                    ExecutionLog.LogWarning($"Trying to add unchecked dto type: {dtoType} for id: {id}.");
                    break;
            }
            return saved;
        }

        /// <summary>
        /// Merges the provided descriptions with those found in cache
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the <c>descriptions</c></param>
        /// <param name="description">A <see cref="MarketDescriptionDTO"/> containing market description in specified language</param>
        private async Task MergeAsync(CultureInfo culture, MarketDescriptionDTO description)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();
            Guard.Argument(description, nameof(description)).NotNull();

            if (_isDisposed)
            {
                return;
            }

            try
            {
                await _semaphoreCacheMerge.WaitAsync().ConfigureAwait(false);
                var cacheId = GetCacheKey(description.Id, description.Variant);
                var cachedItem = _cache.GetCacheItem(cacheId);
                if (cachedItem == null)
                {
                    cachedItem = new CacheItem(cacheId, MarketDescriptionCacheItem.Build(description, _mappingValidatorFactory, culture, CacheName));
                    _cache.Add(cachedItem, new CacheItemPolicy { SlidingExpiration = OperationManager.VariantMarketDescriptionCacheTimeout });
                }
                else
                {
                    ((MarketDescriptionCacheItem)cachedItem.Value).Merge(description, culture);
                }
            }
            catch (Exception e)
            {
                if (!(e is InvalidOperationException))
                {
                    throw;
                }
                ExecutionLog.LogWarning(e, "Mapping validation for MarketDescriptionCacheItem failed.");
            }
            finally
            {
                _semaphoreCacheMerge.ReleaseSafe();
            }
        }

        private bool IsFetchingAllowed(int marketId, string variant, IEnumerable<CultureInfo> culture)
        {
            foreach (var cultureInfo in culture)
            {
                if (IsFetchingAllowed(marketId, variant, cultureInfo))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsFetchingAllowed(int marketId, string variant, CultureInfo culture)
        {
            if (_fetchedVariants.Count > 1000)
            {
                ClearFetchedVariants();
            }

            var key = GetFetchedVariantsKey(marketId, variant, culture);
            if (!_fetchedVariants.ContainsKey(key))
            {
                return true;
            }
            var date = _fetchedVariants[key];
            var result = (DateTime.Now - date).TotalSeconds > SdkInfo.MarketDescriptionMinFetchInterval;

            ClearFetchedVariants();

            return result;
        }

        private static string GetFetchedVariantsKey(int marketId, string variant, CultureInfo culture)
        {
            return $"{marketId}_{variant}_{culture.TwoLetterISOLanguageName}";
        }

        private void ClearFetchedVariants()
        {
            // clear records from _fetchedVariants once a min
            if ((DateTime.Now - _lastTimeFetchedVariantsWereCleared).TotalSeconds > 60)
            {
                foreach (var variant in _fetchedVariants)
                {
                    if ((DateTime.Now - variant.Value).TotalSeconds > SdkInfo.MarketDescriptionMinFetchInterval)
                    {
                        _fetchedVariants.TryRemove(variant.Key, out _);
                    }
                }
                _lastTimeFetchedVariantsWereCleared = DateTime.Now;
            }
        }
    }
}
