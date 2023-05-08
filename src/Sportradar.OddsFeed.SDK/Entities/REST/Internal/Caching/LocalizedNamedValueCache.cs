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
using App.Metrics;
using App.Metrics.Timer;
using Castle.Core.Internal;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching
{
    /// <summary>
    /// A implementation of the interface <see cref="ILocalizedNamedValueCache"/>
    /// </summary>
    /// <seealso cref="ILocalizedNamedValueCache" />
    internal class LocalizedNamedValueCache : ILocalizedNamedValueCache, IDisposable
    {
        /// <summary>
        /// A <see cref="ILogger"/> instance used for logging
        /// </summary>
        private static readonly ILogger CacheLog = SdkLoggerFactory.GetLoggerForCache(typeof(LocalizedNamedValueCache));

        /// <summary>
        /// The <see cref="ILogger"/> instance used for execution logging.
        /// </summary>
        private static readonly ILogger ExecutionLog = SdkLoggerFactory.GetLogger(typeof(LocalizedNamedValueCache));

        /// <summary>
        /// A <see cref="IDataProvider{T}"/> used to get match status descriptions
        /// </summary>
        private readonly IDataProvider<EntityList<NamedValueDTO>> _dataProvider;

        /// <summary>
        /// The list of all supported <see cref="CultureInfo"/>
        /// </summary>
        private readonly IEnumerable<CultureInfo> _defaultCultures;

        /// <summary>
        /// A <see cref="List{CultureInfo}"/> containing already fetched locales
        /// </summary>
        private readonly List<CultureInfo> _loadedCultures;

        /// <summary>
        /// A <see cref="SemaphoreSlim"/> used to synchronize access to shared members
        /// </summary>
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        /// <summary>
        /// A <see cref="IDictionary{TKey,TValue}"/> containing the match status translations
        /// </summary>
        private readonly IDictionary<int, IDictionary<CultureInfo, string>> _namedValues;

        /// <summary>
        /// A <see cref="ExceptionHandlingStrategy"/> enum member specifying how potential exceptions should be handled
        /// </summary>
        private readonly ExceptionHandlingStrategy _exceptionStrategy;

        private readonly string _cacheName;

        /// <summary>
        /// A <see cref="object"/> used to synchronize access to <see cref="_namedValues"/>
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// Value indicating whether the current instance is already disposed
        /// </summary>
        private bool _isDisposed;

        private readonly TimerOptions _timerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ILocalizedNamedValueCache"/> class.
        /// </summary>
        /// <param name="dataProvider">A <see cref="IDataProvider{T}"/> to retrieve match status descriptions</param>
        /// <param name="cultures">A list of all supported languages</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how potential exceptions should be handled</param>
        /// <param name="cacheName">A name of the cache or the name of the values contained in this cache</param>
        /// <param name="sdkTimer">A <see cref="SdkTimer"/> to schedule load of initial values</param>
        public LocalizedNamedValueCache(IDataProvider<EntityList<NamedValueDTO>> dataProvider, ICollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy, string cacheName, SdkTimer sdkTimer)
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

            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _defaultCultures = cultures;
            _exceptionStrategy = exceptionStrategy;
            _cacheName = cacheName;

            _namedValues = new ConcurrentDictionary<int, IDictionary<CultureInfo, string>>();
            _loadedCultures = new List<CultureInfo>();

            _timerOptions = new TimerOptions { Context = $"LocalizedNamedValueCache-{_cacheName}", Name = "FetchAndMerge", MeasurementUnit = Unit.Requests };

            sdkTimer.Elapsed += LoadInitialValues;
            sdkTimer.FireOnce(sdkTimer.DueTime);
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

            EntityList<NamedValueDTO> record;
            using (SdkMetricsFactory.MetricsRoot.Measure.Timer.Time(_timerOptions, $"{culture.TwoLetterISOLanguageName}"))
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
            CacheLog.LogInformation($"{_cacheName}: {record.Items.Count()} items retrieved for locale '{culture.TwoLetterISOLanguageName}'.");
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
                ExecutionLog.LogWarning(ex, $"{_cacheName}: An exception occurred while attempting to retrieve match statuses.");
            }
            catch (ObjectDisposedException ex)
            {
                ExecutionLog.LogWarning($"{_cacheName}: failed because the instance {ex.ObjectName} is being disposed.");
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
                ExecutionLog.LogWarning($"{_cacheName}: Retrieval of item with id={id} failed because the cache is being disposed");
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
                if (_exceptionStrategy == ExceptionHandlingStrategy.THROW)
                {
                    throw new ArgumentOutOfRangeException($"{_cacheName}: item missing for id={id}.");
                }

                return new LocalizedNamedValue(id, null, null);
            }

            var dic = itemDictionary.Where(s => cultureList.Contains(s.Key)).ToDictionary(t => t.Key, t => t.Value);
            return new LocalizedNamedValue(id, dic, cultureList.First());
        }
    }
}
