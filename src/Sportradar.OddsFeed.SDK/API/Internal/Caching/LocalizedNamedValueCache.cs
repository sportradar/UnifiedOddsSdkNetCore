/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Caching
{
    /// <summary>
    /// A implementation of the interface <see cref="ILocalizedNamedValueCache"/>
    /// </summary>
    /// <seealso cref="ILocalizedNamedValueCache" />
    internal class LocalizedNamedValueCache : ILocalizedNamedValueCache, IDisposable
    {
        private static readonly ILogger CacheLog = SdkLoggerFactory.GetLoggerForCache(typeof(LocalizedNamedValueCache));

        private static readonly ILogger ExecutionLog = SdkLoggerFactory.GetLogger(typeof(LocalizedNamedValueCache));

        public string CacheName { get; }

        private readonly ExceptionHandlingStrategy _exceptionStrategy;

        private readonly IDataProvider<EntityList<NamedValueDto>> _dataProvider;

        private readonly IEnumerable<CultureInfo> _defaultCultures;

        private readonly List<CultureInfo> _loadedCultures;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private readonly IDictionary<int, IDictionary<CultureInfo, string>> _namedValues;

        private readonly object _lock = new object();

        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ILocalizedNamedValueCache"/> class.
        /// </summary>
        /// <param name="cacheName">A name of the cache or the name of the values contained in this cache</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how potential exceptions should be handled</param>
        /// <param name="dataProvider">A <see cref="IDataProvider{T}"/> to retrieve match status descriptions</param>
        /// <param name="sdkTimer">A <see cref="SdkTimer"/> to schedule load of initial values</param>
        /// <param name="cultures">A list of all supported languages</param>
        public LocalizedNamedValueCache(string cacheName, ExceptionHandlingStrategy exceptionStrategy, IDataProvider<EntityList<NamedValueDto>> dataProvider, ISdkTimer sdkTimer, ICollection<CultureInfo> cultures)
        {
            if (cultures.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(cultures));
            }
            if (cacheName.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(cacheName));
            }
            if (sdkTimer == null)
            {
                throw new ArgumentNullException(nameof(sdkTimer));
            }

            CacheName = cacheName;
            _exceptionStrategy = exceptionStrategy;
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _defaultCultures = cultures;

            _namedValues = new ConcurrentDictionary<int, IDictionary<CultureInfo, string>>();
            _loadedCultures = new List<CultureInfo>();

            sdkTimer.Elapsed += LoadInitialValues;
            sdkTimer.FireOnce(((SdkTimer)sdkTimer).DueTime);
        }

        private void LoadInitialValues(object sender, EventArgs e)
        {
            GetAsync(0, _defaultCultures).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously gets a match stats descriptions specified by the language specified by <c>culture</c>
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the retrieved match statuses</param>
        /// <returns>A <see cref="Task" /> representing the retrieval operation</returns>
        private async Task FetchAndMerge(CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            EntityList<NamedValueDto> record;
            using (new TelemetryTracker(UofSdkTelemetry.LocalizedNamedValueCache, "cache_name", CacheName))
            {
                record = await _dataProvider.GetDataAsync(culture.TwoLetterISOLanguageName).ConfigureAwait(false);
            }

            lock (_lock)
            {
                foreach (var item in record.Items)
                {
                    if (_namedValues.TryGetValue(item.Id, out var trans))
                    {
                        trans[culture] = item.Description;
                    }
                    else
                    {
                        trans = new Dictionary<CultureInfo, string> { { culture, item.Description } };
                        _namedValues.Add(item.Id, trans);
                    }
                }
                _loadedCultures.Add(culture);
            }
            CacheLog.LogInformation("{CacheName}: {Size} items retrieved for language '{Language}'", CacheName, record.Items.Count().ToString(CultureInfo.InvariantCulture), culture.TwoLetterISOLanguageName);
        }

        /// <summary>
        /// Asynchronously gets a match status descriptions specified by the language specified by <c>culture</c>
        /// </summary>
        /// <param name="cultures">A array of <see cref="CultureInfo"/> specifying the language of the retrieved match statuses</param>
        /// <returns>A <see cref="Task" /> representing the retrieval operation</returns>
        private async Task GetInternalAsync(IEnumerable<CultureInfo> cultures)
        {
            try
            {
                var tasks = cultures.Select(FetchAndMerge).ToList();
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (FeedSdkException ex)
            {
                ExecutionLog.LogWarning(ex, "{CacheName}: An exception occurred while attempting to retrieve match statuses", CacheName);
            }
            catch (ObjectDisposedException ex)
            {
                ExecutionLog.LogWarning(ex, "{CacheName}: failed because the instance {DisposedObject} is being disposed", CacheName, ex.ObjectName);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                _isDisposed = true;
                _semaphore.Dispose();
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
        /// Determines whether specified id is present int the cache
        /// </summary>
        /// <param name="id">The id to be tested.</param>
        /// <returns>True if the value is defined in the cache; False otherwise.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsValueDefined(int id)
        {
            _semaphore.Wait(-1);
            if (!_loadedCultures.Any())
            {
                GetInternalAsync(new[] { _defaultCultures.First() }).GetAwaiter().GetResult();
            }
            var exists = _namedValues.ContainsKey(id);
            if (!_isDisposed)
            {
                _semaphore.ReleaseSafe();
            }
            return exists;
        }

        /// <summary>
        /// get as an asynchronous operation.
        /// </summary>
        /// <param name="id">The id of the <see cref="ILocalizedNamedValue" /> to retrieve</param>
        /// <param name="cultures">The cultures to be used to retrieve descriptions</param>
        /// <returns>A <see cref="Task{ILocalizedNamedValue}"/> representing the async operation</returns>
        public async Task<ILocalizedNamedValue> GetAsync(int id, IEnumerable<CultureInfo> cultures = null)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }
            if (cultures == null)
            {
                cultures = _defaultCultures;
            }

            var cultureList = cultures as IList<CultureInfo> ?? cultures.ToList();
            if (!cultureList.Any())
            {
                cultureList = _defaultCultures.ToList();
            }

            IDictionary<CultureInfo, string> itemDictionary = null;

            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                var missingCultures = cultureList.Where(c => !_loadedCultures.Contains(c)).ToList();

                if (missingCultures.Any())
                {
                    await GetInternalAsync(missingCultures).ConfigureAwait(false);
                }

                _namedValues.TryGetValue(id, out itemDictionary);
            }
            catch (ObjectDisposedException)
            {
                ExecutionLog.LogWarning("{CacheName}: Retrieval of item with id={CacheItemId} failed because the cache is being disposed", CacheName, id.ToString());
            }
            finally
            {
                if (!_isDisposed)
                {
                    _semaphore.ReleaseSafe();
                }
            }

            if (itemDictionary == null)
            {
                if (_exceptionStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw new ArgumentOutOfRangeException($"{CacheName}: item missing for id={id}.");
                }

                return new LocalizedNamedValue(id, null, null);
            }

            var dic = itemDictionary.Where(s => cultureList.Contains(s.Key)).ToDictionary(t => t.Key, t => t.Value);
            return new LocalizedNamedValue(id, dic, cultureList.First());
        }
    }
}
