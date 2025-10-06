// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.InternalEntities;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Entities.Rest.Market;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Caching
{
    /// <summary>
    /// A <see cref="IMarketDescriptionCache" /> implementation used to store market descriptors for variant markets (single - dynamic)
    /// </summary>
    /// <seealso cref="IDisposable" />
    /// <seealso cref="IMarketDescriptionCache" />
    internal class VariantMarketDescriptionCache : SdkCache, IMarketDescriptionCache
    {
        /// <summary>
        /// A <see cref="ICacheStore{T}"/> used to store market descriptors
        /// </summary>
        private readonly ICacheStore<string> _cache;

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

        internal readonly ConcurrentDictionary<string, DateTime> FetchedVariants = new ConcurrentDictionary<string, DateTime>();

        private DateTime _lastTimeFetchedVariantsWereCleared = DateTime.MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariantMarketDescriptionCache"/> class
        /// </summary>
        /// <param name="cache">A <see cref="ICacheStore{T}"/> used to store market descriptors</param>
        /// <param name="dataRouterManager">A <see cref="IDataRouterManager"/> used to fetch data</param>
        /// <param name="mappingValidatorFactory">A <see cref="IMappingValidatorFactory"/> used to construct <see cref="IMappingValidator"/> instances for market mappings</param>
        /// <param name="cacheManager">A <see cref="ICacheManager"/> used to interact among caches</param>
        /// <param name="loggerFactory">The logger factory for creating Cache and Execution logs</param>
        [SuppressMessage("ReSharper", "TooManyDependencies", Justification = "Needed here")]
        public VariantMarketDescriptionCache(ICacheStore<string> cache,
                                             IDataRouterManager dataRouterManager,
                                             IMappingValidatorFactory mappingValidatorFactory,
                                             ICacheManager cacheManager,
                                             ILoggerFactory loggerFactory)
            : base(cacheManager, loggerFactory)
        {
            Guard.Argument(cache, nameof(cache)).NotNull();
            Guard.Argument(dataRouterManager, nameof(dataRouterManager)).NotNull();
            Guard.Argument(mappingValidatorFactory, nameof(mappingValidatorFactory)).NotNull();

            _cache = cache;
            _dataRouterManager = dataRouterManager;
            _mappingValidatorFactory = mappingValidatorFactory;
        }

        /// <summary>
        /// Set the list of <see cref="DtoType"/> in this cache
        /// </summary>
        public override void SetDtoTypes()
        {
            RegisteredDtoTypes = new List<DtoType>
                                     {
                                         DtoType.MarketDescription
                                     };
        }

        /// <summary>
        /// Gets a cache key generated from the provided <c>id</c> and <c>variant</c>
        /// </summary>
        /// <param name="marketId">The id of the market</param>
        /// <param name="variant">The market variant</param>
        /// <returns>a cache key generated from the provided <c>id</c> and <c>variant</c></returns>
        public static string GenerateCacheKey(long marketId, string variant)
        {
            Guard.Argument(variant, nameof(variant)).NotNull().NotEmpty();

            return $"{marketId}_{variant}";
        }

        /// <summary>
        /// Does item exist in the cache
        /// </summary>
        /// <param name="id">A <see cref="Urn" /> representing the id of the item to be checked</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        [SuppressMessage("ReSharper", "FlagArgument", Justification = "Required here")]
        public override bool CacheHasItem(Urn id, CacheItemType cacheItemType)
        {
            if (id == null)
            {
                return false;
            }

            if (cacheItemType == CacheItemType.All || cacheItemType == CacheItemType.MarketDescription)
            {
                return _cache.Contains(id.Id.ToString(CultureInfo.InvariantCulture));
            }
            return false;
        }

        /// <summary>
        /// Does item exist in the cache
        /// </summary>
        /// <param name="id">A string representing the id of the item to be checked</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        [SuppressMessage("ReSharper", "FlagArgument", Justification = "Required here")]
        public override bool CacheHasItem(string id, CacheItemType cacheItemType)
        {
            if (cacheItemType == CacheItemType.All || cacheItemType == CacheItemType.MarketDescription)
            {
                return _cache.Contains(id);
            }

            return false;
        }

        /// <summary>
        /// Updates cache item fetch time
        /// </summary>
        public void UpdateCacheItem(int marketId, string variantValue)
        {
            if (_isDisposed)
            {
                return;
            }
            GetItemFromCache(marketId, variantValue)?.SetFetchInfo(CacheName);
        }

        /// <summary>
        /// Deletes the item from cache
        /// </summary>
        /// <param name="id">A <see cref="Urn" /> representing the id of the item in the cache to be deleted</param>
        /// <param name="cacheItemType">A cache item type</param>
        public override void CacheDeleteItem(Urn id, CacheItemType cacheItemType)
        {
            // ignored
        }

        /// <summary>
        /// Deletes the item from cache
        /// </summary>
        /// <param name="id">A string representing the id of the item in the cache to be deleted</param>
        /// <param name="cacheItemType">A cache item type</param>
        [SuppressMessage("ReSharper", "FlagArgument", Justification = "Required here")]
        public override void CacheDeleteItem(string id, CacheItemType cacheItemType)
        {
            if (cacheItemType == CacheItemType.All || cacheItemType == CacheItemType.MarketDescription)
            {
                foreach (var fetchedVariant in FetchedVariants)
                {
                    if (fetchedVariant.Key.StartsWith(id, StringComparison.InvariantCultureIgnoreCase))
                    {
                        FetchedVariants.TryRemove(fetchedVariant.Key, out _);
                    }
                }

                if (_cache.Contains(id))
                {
                    LogCacheDeleteItem(CacheLog, id, null);
                    _cache.Remove(id);
                }
            }
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "TooManyArguments", Justification = "Required for this method")]
        [SuppressMessage("ReSharper", "FlagArgument", Justification = "Required for this method")]
        protected override async Task<bool> CacheAddDtoItemAsync(Urn id, object item, CultureInfo culture, DtoType dtoType, ISportEventCacheItem requester)
        {
            if (_isDisposed)
            {
                return false;
            }

            if (dtoType != DtoType.MarketDescription)
            {
                return false;
            }

            if (item is MarketDescriptionDto marketDescription)
            {
                await MergeAsync(culture, marketDescription).ConfigureAwait(false);
                return true;
            }

            LogSavingDtoConflict(id, typeof(MarketDescriptionDto), item.GetType());

            return false;
        }

        /// <summary>
        /// Merges the provided descriptions with those found in cache
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the <c>descriptions</c></param>
        /// <param name="mdDto">A <see cref="MarketDescriptionDto"/> containing market description in specified language</param>
        private async Task MergeAsync(CultureInfo culture, MarketDescriptionDto mdDto)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();
            Guard.Argument(mdDto, nameof(mdDto)).NotNull();

            await MergeInternalAsync(culture, mdDto).ConfigureAwait(false);
        }

        private async Task MergeInternalAsync(CultureInfo culture, MarketDescriptionDto mdDto)
        {
            try
            {
                await _semaphoreCacheMerge.WaitAsync().ConfigureAwait(false);
                MergeMarketDescriptionDto(mdDto, culture);
            }
            catch (Exception e)
            {
                ExecutionLog.LogWarning(e, "Merging data for MarketDescriptionCacheItem failed");
                if (!(e is InvalidOperationException))
                {
                    throw;
                }
            }
            finally
            {
                _semaphoreCacheMerge.ReleaseSafe();
            }
        }

        private void MergeMarketDescriptionDto(MarketDescriptionDto mdDto, CultureInfo language)
        {
            var cacheId = GenerateCacheKey(mdDto.Id, mdDto.Variant);
            var mdCacheItem = (MarketDescriptionCacheItem)_cache.Get(cacheId);
            if (mdCacheItem == null)
            {
                mdCacheItem = MarketDescriptionCacheItem.Build(mdDto, _mappingValidatorFactory, language, CacheName);
                _cache.Add(cacheId, mdCacheItem);
            }
            else
            {
                var mergeResult = mdCacheItem.Merge(mdDto, language);
                mdCacheItem.HandleMarketMergeResult(ExecutionLog, mergeResult, mdDto, language);
            }
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
            var cacheItem = _cache.Get(GenerateCacheKey(id, variant));
            _semaphoreCacheMerge.ReleaseSafe();

            return (MarketDescriptionCacheItem)cacheItem;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IMarketDescription" /> instance for the market specified by <c>id</c> and <c>specifiers</c>
        /// </summary>
        /// <param name="marketId">The market identifier</param>
        /// <param name="variant">A <see cref="string" /> specifying market variant or a null reference if market is invariant</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}" /> specifying required translations</param>
        /// <returns>A <see cref="Task{T}" /> representing the async retrieval operation</returns>
        /// <exception cref="CacheItemNotFoundException">The requested key was not found in the cache and could not be loaded</exception>
        public Task<IMarketDescription> GetMarketDescriptionAsync(int marketId, string variant, IReadOnlyCollection<CultureInfo> cultures)
        {
            if (string.IsNullOrEmpty(variant))
            {
                throw new ArgumentException("Variant cannot be a null or empty string", nameof(variant));
            }

            return GetMarketDescriptionExecAsync(marketId, variant, cultures);
        }

        private async Task<IMarketDescription> GetMarketDescriptionExecAsync(int marketId, string variant, IReadOnlyCollection<CultureInfo> cultures)
        {
            MarketDescriptionCacheItem cacheItem;
            try
            {
                cacheItem = await GetMarketInternalAsync(marketId, variant, cultures).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is CommunicationException || ex is DeserializationException || ex is MappingException)
                {
                    throw new CacheItemNotFoundException("The requested key was not found in the cache", GenerateCacheKey(marketId, variant), ex);
                }
                throw;
            }

            return cacheItem == null
                       ? null
                       : new MarketDescription(cacheItem, cultures);
        }

        /// <summary>
        /// Asynchronously gets the specified <see cref="MarketDescriptionCacheItem"/>. If the item is not found in local cache, it is fetched and stored / merged into cache
        /// </summary>
        /// <param name="id">The id of the market associated with the requested descriptor</param>
        /// <param name="variant">A variation of the market associated with the requested descriptor</param>
        /// <param name="wantedLanguages">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying the languages which the returned item must contain</param>
        /// <returns>A <see cref="Task"/> representing the async operation</returns>
        /// <exception cref="CommunicationException">An error occurred while accessing the remote party</exception>
        /// <exception cref="DeserializationException">An error occurred while deserializing fetched data</exception>
        /// <exception cref="FormatException">An error occurred while mapping deserialized entities</exception>
        internal async Task<MarketDescriptionCacheItem> GetMarketInternalAsync(int id, string variant, IReadOnlyCollection<CultureInfo> wantedLanguages)
        {
            Guard.Argument(variant, nameof(variant)).NotNull().NotEmpty();
            Guard.Argument(wantedLanguages, nameof(wantedLanguages)).NotNull().NotEmpty();

            if (_isDisposed)
            {
                return null;
            }

            ClearFetchedVariants();

            if (!IsAnyWantedLanguageToBeFetched(wantedLanguages, id, variant, out var marketDescription, out var missingLanguages))
            {
                return marketDescription;
            }

            marketDescription = await FetchMissingDataForMarketDescriptionCacheItem(id, variant, new ReadOnlyCollection<CultureInfo>(missingLanguages.ToList())).ConfigureAwait(false);

            return marketDescription;
        }

        private async Task<MarketDescriptionCacheItem> FetchMissingDataForMarketDescriptionCacheItem(int id, string variant, IReadOnlyCollection<CultureInfo> wantedLanguages)
        {
            MarketDescriptionCacheItem marketDescription;
            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);

                if (!IsAnyWantedLanguageToBeFetched(wantedLanguages, id, variant, out marketDescription, out var missingLanguages))
                {
                    return marketDescription;
                }

                await FetchFromApiMissingMarketDescriptions(id, variant, missingLanguages).ConfigureAwait(false);

                var cachedItem = _cache.Get(GenerateCacheKey(id, variant));

                marketDescription = (MarketDescriptionCacheItem)cachedItem;
            }
            finally
            {
                _semaphore.ReleaseSafe();
            }

            return marketDescription;
        }

        private async Task FetchFromApiMissingMarketDescriptions(int id, string variant, ICollection<CultureInfo> missingLanguages)
        {
            //sometimes it may be null (aka Not Found), but we still do not re-fetch immediately
            foreach (var cultureInfo in missingLanguages)
            {
                FetchedVariants[GenerateFetchedVariantsKey(id, variant, cultureInfo)] = DateTime.Now;
            }

            var cultureTaskDictionary = missingLanguages.ToDictionary(c => c, c => _dataRouterManager.GetVariantMarketDescriptionAsync(id, variant, c));
            await Task.WhenAll(cultureTaskDictionary.Values).ConfigureAwait(false);
        }

        [SuppressMessage("ReSharper", "TooManyArguments", Justification = "Needed here")]
        private bool IsAnyWantedLanguageToBeFetched(IReadOnlyCollection<CultureInfo> wantedLanguages, int marketId, string variant, out MarketDescriptionCacheItem marketDescription, out ICollection<CultureInfo> missingLanguages)
        {
            // check: if it is missing, if fetched market description is faulty and can be fetched based on last fetch time
            marketDescription = GetItemFromCache(marketId, variant);
            missingLanguages = LanguageHelper.GetMissingCultures(wantedLanguages, marketDescription?.Names.Keys);

            if (marketDescription != null)
            {
                var faultyLanguages = marketDescription.GetFaultyLanguages();

                foreach (var faultyLanguage in faultyLanguages)
                {
                    if (wantedLanguages.Contains(faultyLanguage))
                    {
                        missingLanguages.AddUnique(faultyLanguage);
                    }
                }
            }

            RemoveRecentlyFetchedLanguages(marketId, variant, missingLanguages);

            return missingLanguages.Any();
        }

        private void RemoveRecentlyFetchedLanguages(int marketId, string variant, ICollection<CultureInfo> missingLanguages)
        {
            foreach (var missingLanguage in missingLanguages.ToList())
            {
                if (!IsFetchingAllowed(marketId, variant, missingLanguage))
                {
                    missingLanguages.Remove(missingLanguage);
                }
            }
        }

        public bool IsDisposed()
        {
            return _isDisposed;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources</param>
        [SuppressMessage("ReSharper", "FlagArgument", Justification = "Default one")]
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            if (!disposing)
            {
                return;
            }

            _semaphore.Dispose();
            _semaphoreCacheMerge.Dispose();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool IsFetchingAllowed(int marketId, string variant, CultureInfo culture)
        {
            var key = GenerateFetchedVariantsKey(marketId, variant, culture);
            if (!FetchedVariants.ContainsKey(key))
            {
                return true;
            }
            var date = FetchedVariants[key];
            var result = (DateTime.Now - date).TotalSeconds > ConfigLimit.MarketDescriptionMinFetchInterval;

            return result;
        }

        private static string GenerateFetchedVariantsKey(int marketId, string variant, CultureInfo culture)
        {
            return $"{marketId.ToString(CultureInfo.InvariantCulture)}_{variant}_{culture.TwoLetterISOLanguageName}";
        }

        private void ClearFetchedVariants()
        {
            if (FetchedVariants.Count < 1000)
            {
                return;
            }

            // clear records from _fetchedVariants once a min
            var timeSinceLastClearing = DateTime.Now - _lastTimeFetchedVariantsWereCleared;
            if (timeSinceLastClearing.TotalSeconds < 60)
            {
                return;
            }

            foreach (var variant in FetchedVariants)
            {
                if ((DateTime.Now - variant.Value).TotalSeconds > ConfigLimit.MarketDescriptionMinFetchInterval)
                {
                    FetchedVariants.TryRemove(variant.Key, out _);
                }
            }
            _lastTimeFetchedVariantsWereCleared = DateTime.Now;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(HealthCheckResult.Healthy($"Cache has {_cache.Count().ToString(CultureInfo.InvariantCulture)} items"));
        }

        private static readonly Action<ILogger, string, Exception> LogCacheDeleteItem =
            LoggerMessage.Define<string>(LogLevel.Debug,
                                         new EventId(1),
                                         "Delete variant market: {MdId}");
    }
}
