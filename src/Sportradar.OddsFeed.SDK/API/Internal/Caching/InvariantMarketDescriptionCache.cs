// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
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
    ///     An <see cref="IMarketDescriptionCache" /> implementation used to store market descriptors for invariant markets (static)
    /// </summary>
    /// <seealso cref="IDisposable" />
    /// <seealso cref="IMarketDescriptionCache" />
    internal sealed class InvariantMarketDescriptionCache : SdkCache, IMarketDescriptionsCache
    {
        /// <summary>
        ///     A <see cref="ICacheStore{T}" /> used to store market descriptors
        /// </summary>
        private readonly ICacheStore<string> _cacheStore;

        /// <summary>
        ///     The <see cref="IDataRouterManager" /> used to obtain data via REST request
        /// </summary>
        private readonly IDataRouterManager _dataRouterManager;

        /// <summary>
        ///     The <see cref="IMappingValidatorFactory" /> used to construct <see cref="IMappingValidator" /> instances for market mappings
        /// </summary>
        private readonly IMappingValidatorFactory _mappingValidatorFactory;

        /// <summary>
        ///     The <see cref="ISdkTimer" /> instance used to periodically fetch market descriptors
        /// </summary>
        private readonly ISdkTimer _timer;

        /// <summary>
        ///     A <see cref="IReadOnlyCollection{CultureInfo}" /> specifying the languages for which the data should be pre-fetched
        /// </summary>
        private readonly IReadOnlyCollection<CultureInfo> _prefetchLanguages;

        /// <summary>
        ///     A <see cref="IList{CultureInfo}" /> used to store languages for which the data was already fetched (at least once)
        /// </summary>
        private readonly IList<CultureInfo> _fetchedLanguages = new List<CultureInfo>();

        /// <summary>
        ///     An <see cref="SemaphoreSlim" /> instance to synchronize access from multiple threads
        /// </summary>
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        /// <summary>
        ///     A <see cref="SemaphoreSlim" /> used to synchronize merging on cache item
        /// </summary>
        private readonly SemaphoreSlim _semaphoreCacheMerge = new SemaphoreSlim(1);

        /// <summary>
        ///     Value indicating whether at least one fetch caused by the timer was done.
        /// </summary>
        private bool _hasTimerElapsedOnce;

        /// <summary>
        ///     Value indicating whether the current instance was already disposed
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvariantMarketDescriptionCache" /> class
        /// </summary>
        /// <param name="cacheStore">A <see cref="ICacheStore{T}" /> used to store market descriptors</param>
        /// <param name="dataRouterManager">A <see cref="IDataRouterManager" /> used to fetch data</param>
        /// <param name="mappingValidatorFactory">A <see cref="IMappingValidatorFactory" /> used to construct <see cref="IMappingValidator" /> instances for market mappings</param>
        /// <param name="timer">The <see cref="ISdkTimer" /> instance used to periodically fetch market descriptors</param>
        /// <param name="prefetchLanguages">A <see cref="IReadOnlyCollection{CultureInfo}" /> specifying the languages for which the data should be pre-fetched</param>
        /// <param name="cacheManager">A <see cref="ICacheManager" /> used to interact among caches</param>
        /// <param name="loggerFactory">The logger factory for creating Cache and Execution logs</param>
        [SuppressMessage("ReSharper", "TooManyDependencies", Justification = "Required")]
        public InvariantMarketDescriptionCache(ICacheStore<string> cacheStore,
                                               IDataRouterManager dataRouterManager,
                                               IMappingValidatorFactory mappingValidatorFactory,
                                               ISdkTimer timer,
                                               IReadOnlyCollection<CultureInfo> prefetchLanguages,
                                               ICacheManager cacheManager,
                                               ILoggerFactory loggerFactory)
            : base(cacheManager, loggerFactory)
        {
            Guard.Argument(cacheStore, nameof(cacheStore)).NotNull();
            Guard.Argument(dataRouterManager, nameof(dataRouterManager)).NotNull();
            Guard.Argument(mappingValidatorFactory, nameof(mappingValidatorFactory)).NotNull();
            Guard.Argument(timer, nameof(timer)).NotNull();
            Guard.Argument(prefetchLanguages, nameof(prefetchLanguages)).NotNull().NotEmpty();

            _cacheStore = cacheStore;
            _dataRouterManager = dataRouterManager;
            _mappingValidatorFactory = mappingValidatorFactory;
            _timer = timer;
            _prefetchLanguages = prefetchLanguages;
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();
        }

        /// <summary>
        ///     Invoked when the <c>_timer</c> ticks in order to periodically fetch market descriptions for configured languages
        /// </summary>
        /// <param name="sender">The <see cref="ISdkTimer" /> raising the event</param>
        /// <param name="e">An <see cref="EventArgs" /> instance containing the event data</param>
        private async void OnTimerElapsed(object sender, EventArgs e)
        {
            if (_isDisposed)
            {
                return;
            }

            //when the timer first elapses fetch data only for languages which were not yet fetched. On subsequent timer elapses fetch data for all configured languages
            var missingLanguages = _prefetchLanguages.Where(language => !_fetchedLanguages.Contains(language) || _hasTimerElapsedOnce).ToList();

            if (!missingLanguages.Any())
            {
                _hasTimerElapsedOnce = true;
                return;
            }

            try
            {
                _fetchedLanguages.Clear();
                ExecutionLog.LogDebug("Loading invariant market descriptions for [{Languages}] (timer)", LanguageHelper.GetCultureList(missingLanguages));
                await GetMarketInternalAsync(0, missingLanguages).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ExecutionLog.LogWarning(ex, "An error occurred while periodically loading market descriptions");
            }

            _hasTimerElapsedOnce = true;
        }

        /// <summary>
        ///     Gets the <see cref="MarketDescriptionCacheItem" /> specified by it's id from the local cache
        /// </summary>
        /// <param name="id">The id of the <see cref="MarketDescriptionCacheItem" /> to get</param>
        /// <returns>The <see cref="MarketDescriptionCacheItem" /> retrieved from the cache or a null reference if item is not found</returns>
        private MarketDescriptionCacheItem GetItemFromCache(int id)
        {
            if (!_isDisposed)
            {
                _semaphoreCacheMerge.Wait();
            }

            var cacheItem = _cacheStore.Get(id.ToString());
            if (!_isDisposed)
            {
                _semaphoreCacheMerge.ReleaseSafe();
            }

            return (MarketDescriptionCacheItem)cacheItem;
        }

        /// <summary>
        /// Asynchronously gets the <see cref="MarketDescriptionCacheItem" /> specified by it's id. If the item is not found in local cache, all items for specified
        /// language are fetched from the service and stored/merged into the local cache.
        /// </summary>
        /// <param name="id">The id of the <see cref="MarketDescriptionCacheItem" /> instance to get</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}" /> specifying the languages which the returned item must contain</param>
        /// <returns>A <see cref="Task" /> representing the async operation</returns>
        /// <exception cref="CommunicationException">An error occurred while accessing the remote party</exception>
        /// <exception cref="DeserializationException">An error occurred while deserializing fetched data</exception>
        /// <exception cref="FormatException">An error occurred while mapping deserialized entities</exception>
        internal async Task<MarketDescriptionCacheItem> GetMarketInternalAsync(int id,
                                                                               IReadOnlyCollection<CultureInfo> cultures)
        {
            Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();

            var description = GetItemFromCache(id);
            if (description != null && !LanguageHelper.GetMissingCultures(cultures, description.Names.Keys).Any())
            {
                return description;
            }

            ICollection<CultureInfo> missingLanguages = new Collection<CultureInfo>();
            try
            {
                if (_isDisposed)
                {
                    return null;
                }

                await _semaphore.WaitAsync().ConfigureAwait(false);

                description = GetItemFromCache(id);
                missingLanguages = LanguageHelper.GetMissingCultures(cultures, description?.Names.Keys);

                if (missingLanguages.Any())
                {
                    missingLanguages = LanguageHelper.GetMissingCultures(missingLanguages, _fetchedLanguages);
                }

                if (!missingLanguages.Any())
                {
                    return description;
                }

                var cultureTaskDictionary = missingLanguages.ToDictionary(l => l, l => _dataRouterManager.GetMarketDescriptionsAsync(l));
                await Task.WhenAll(cultureTaskDictionary.Values).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogFetchException(ex, missingLanguages);
                throw;
            }
            finally
            {
                if (!_isDisposed)
                {
                    _semaphore.ReleaseSafe();
                }
            }

            description = GetItemFromCache(id);

            return description != null && !LanguageHelper.GetMissingCultures(cultures, description.Names.Keys).Any()
                       ? description
                       : null;
        }

        private void LogFetchException(Exception ex, ICollection<CultureInfo> missingLanguages)
        {
            if (ex is CommunicationException || ex is DeserializationException || ex is FormatException)
            {
                ExecutionLog.LogWarning(ex, "An error occurred while fetching market descriptions of languages {Languages}", LanguageHelper.GetCultureList(missingLanguages));
            }
            else if (ex is ObjectDisposedException disposedException)
            {
                ExecutionLog.LogWarning("Fetching market descriptions failed because the object graph is being disposed. Object causing the exception:{DisposedObjectName}",
                                        disposedException.ObjectName);
            }
            else if (ex is TaskCanceledException)
            {
                ExecutionLog.LogWarning(ex, "Fetching market descriptions took too long");
            }
        }

        public bool IsDisposed()
        {
            return _isDisposed;
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources</param>
        private void Dispose(bool disposing)
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

            _timer.Stop();
            _timer.Elapsed -= OnTimerElapsed;
            _timer.Dispose();

            _semaphore.Dispose();
            _semaphoreCacheMerge.Dispose();
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        ///     Gets the market descriptor
        /// </summary>
        /// <param name="marketId">The market identifier</param>
        /// <param name="variant">A <see cref="string" /> specifying market variant or a null reference if market is invariant</param>
        /// <param name="cultures">The cultures</param>
        /// <exception cref="CacheItemNotFoundException">The requested key was not found in the cache and could not be loaded</exception>
        public async Task<IMarketDescription> GetMarketDescriptionAsync(int marketId, string variant, IReadOnlyCollection<CultureInfo> cultures)
        {
            Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();

            MarketDescriptionCacheItem cacheItem;

            try
            {
                cacheItem = await GetMarketInternalAsync(marketId, cultures).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new CacheItemNotFoundException("The requested market description could not be retrieved from cache", marketId.ToString(), ex);
            }

            return cacheItem == null
                       ? null
                       : new MarketDescription(cacheItem, cultures.ToList());
        }

        /// <summary>
        ///     Asynchronously loads the list of market descriptions from the Sports API
        /// </summary>
        /// <returns>Returns true if the action succeeded</returns>
        public async Task<bool> LoadMarketDescriptionsAsync()
        {
            try
            {
                ExecutionLog.LogDebug("Loading invariant market descriptions for [{Languages}] (user request)", LanguageHelper.GetCultureList(_prefetchLanguages));
                _fetchedLanguages.Clear();
                await GetMarketInternalAsync(0, _prefetchLanguages).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ExecutionLog.LogWarning(ex, "An error occurred while fetching market descriptions");
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Updates cache item fetch time
        /// </summary>
        public void UpdateCacheItem(int marketId, string variantValue)
        {
            if (_isDisposed)
            {
                return;
            }

            GetItemFromCache(marketId)?.SetFetchInfo(CacheName);
        }

        public Task<IEnumerable<IMarketDescription>> GetAllInvariantMarketDescriptionsAsync(IReadOnlyCollection<CultureInfo> cultures)
        {
            Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();

            return Task.FromResult<IEnumerable<IMarketDescription>>(_cacheStore.GetValues().Select(s => new MarketDescription(s as MarketDescriptionCacheItem, cultures)));
        }

        /// <summary>
        ///     Set the list of <see cref="DtoType" /> in this cache
        /// </summary>
        public override void SetDtoTypes()
        {
            RegisteredDtoTypes = new List<DtoType> { DtoType.MarketDescriptionList };
        }

        /// <summary>
        ///     Deletes the item from cache
        /// </summary>
        /// <param name="id">A <see cref="Urn" /> representing the id of the item in the cache to be deleted</param>
        /// <param name="cacheItemType">A cache item type</param>
        public override void CacheDeleteItem(Urn id, CacheItemType cacheItemType)
        {
            // ignored; nothing to delete from fixed list
        }

        /// <summary>
        ///     Deletes the item from cache
        /// </summary>
        /// <param name="id">A string representing the id of the item in the cache to be deleted</param>
        /// <param name="cacheItemType">A cache item type</param>
        [SuppressMessage("ReSharper", "FlagArgument", Justification = "Needed here")]
        public override void CacheDeleteItem(string id, CacheItemType cacheItemType)
        {
            // ignored; nothing to delete from fixed list
        }

        /// <summary>
        ///     Does item exist in the cache
        /// </summary>
        /// <param name="id">A <see cref="Urn" /> representing the id of the item to be checked</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        [SuppressMessage("ReSharper", "ComplexConditionExpression", Justification = "Simple enough")]
        [SuppressMessage("ReSharper", "FlagArgument", Justification = "Needed here")]
        public override bool CacheHasItem(Urn id, CacheItemType cacheItemType)
        {
            return id != null && CacheHasItem(id.Id.ToString(CultureInfo.InvariantCulture), cacheItemType);
        }

        /// <summary>
        ///     Does item exist in the cache
        /// </summary>
        /// <param name="id">A string representing the id of the item to be checked</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        [SuppressMessage("ReSharper", "ComplexConditionExpression", Justification = "Simple enough")]
        [SuppressMessage("ReSharper", "FlagArgument", Justification = "Needed here")]
        public override bool CacheHasItem(string id, CacheItemType cacheItemType)
        {
            if (cacheItemType == CacheItemType.All || cacheItemType == CacheItemType.MarketDescription)
            {
                return _cacheStore.GetKeys().Contains(id);
            }

            return false;
        }

        /// <inheritdoc />
        protected override async Task<bool> CacheAddDtoItemAsync(Urn id, object item, CultureInfo culture, DtoType dtoType, ISportEventCacheItem requester)
        {
            if (_isDisposed)
            {
                return false;
            }

            if (dtoType != DtoType.MarketDescriptionList)
            {
                return false;
            }

            if (item is EntityList<MarketDescriptionDto> marketDescriptionList)
            {
                await MergeAsync(culture, marketDescriptionList.Items.ToList()).ConfigureAwait(false);
                return true;
            }

            LogSavingDtoConflict(id, typeof(EntityList<MarketDescriptionDto>), item.GetType());

            return false;
        }

        /// <summary>
        ///     Merges the provided descriptions with those found in cache
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo" /> specifying the language of the <c>descriptions</c></param>
        /// <param name="descriptions">A <see cref="IReadOnlyCollection{MarketDescriptionDto}" /> containing market descriptions in specified language</param>
        private async Task MergeAsync(CultureInfo culture, IReadOnlyCollection<MarketDescriptionDto> descriptions)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();
            Guard.Argument(descriptions, nameof(descriptions)).NotNull().NotEmpty();

            try
            {
                await _semaphoreCacheMerge.WaitAsync().ConfigureAwait(false);
                foreach (var marketDescriptionDto in descriptions)
                {
                    try
                    {
                        var marketDescriptionCacheItem = (MarketDescriptionCacheItem)_cacheStore.Get(marketDescriptionDto.Id.ToString());
                        if (marketDescriptionCacheItem == null)
                        {
                            var newCacheItem = MarketDescriptionCacheItem.Build(marketDescriptionDto, _mappingValidatorFactory, culture, CacheName);
                            _cacheStore.Add(newCacheItem.Id.ToString(), newCacheItem, CacheItemPriority.NeverRemove);
                        }
                        else
                        {
                            var mergeResult = marketDescriptionCacheItem.Merge(marketDescriptionDto, culture);
                            marketDescriptionCacheItem.HandleMarketMergeResult(ExecutionLog, mergeResult, marketDescriptionDto, culture);
                        }
                    }
                    catch (Exception e)
                    {
                        if (!(e is InvalidOperationException))
                        {
                            throw;
                        }

                        ExecutionLog.LogWarning(e, "Mapping validation for MarketDescriptionCacheItem failed. Id={MarketDescriptionId}", marketDescriptionDto.Id.ToString());
                    }
                }

                _fetchedLanguages.Add(culture);
            }
            finally
            {
                if (!_isDisposed)
                {
                    _semaphoreCacheMerge.ReleaseSafe();
                }
            }
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var memoryCount = _cacheStore.Count();
            return Task.FromResult(HealthCheckResult.Healthy($"Cache has {memoryCount.ToString()} items"));
        }
    }
}
