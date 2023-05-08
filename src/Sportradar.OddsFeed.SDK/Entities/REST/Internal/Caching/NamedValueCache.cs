/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using App.Metrics;
using App.Metrics.Timer;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching
{
    /// <summary>
    /// A default implementation of the <see cref="INamedValueCache"/>
    /// </summary>
    /// <seealso cref="INamedValueCache" />
    internal class NamedValueCache : INamedValueCache
    {
        /// <summary>
        /// A <see cref="ILogger"/> instance used for logging
        /// </summary>
        private static readonly ILogger CacheLog = SdkLoggerFactory.GetLoggerForCache(typeof(NamedValueCache));

        /// <summary>
        /// The <see cref="ILogger"/> instance used for execution logging.
        /// </summary>
        private static readonly ILogger ExecutionLog = SdkLoggerFactory.GetLogger(typeof(NamedValueCache));

        /// <summary>
        /// A <see cref="IDataProvider{T}"/> used to get the named values
        /// </summary>
        private readonly IDataProvider<EntityList<NamedValueDTO>> _dataProvider;

        /// <summary>
        /// A <see cref="IDictionary{TKey,TValue}"/> containing the match status translations
        /// </summary>
        private readonly IDictionary<int, string> _namedValues;

        /// <summary>
        /// A <see cref="ExceptionHandlingStrategy"/> enum member specifying how potential exceptions should be handled
        /// </summary>
        private readonly ExceptionHandlingStrategy _exceptionStrategy;

        private readonly string _cacheName;

        /// <summary>
        /// An <see cref="object"/> used to sync  access to shared members
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// A value indicating whether the data was already fetched
        /// </summary>
        private bool _dataFetched;

        private readonly TimerOptions _timerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedValueCache"/> class.
        /// </summary>
        /// <param name="dataProvider">A <see cref="IDataProvider{T}"/> used to get the named values</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how potential exceptions should be handled</param>
        /// <param name="cacheName">A name of the cache or the name of the values contained in this cache</param>
        /// <param name="sdkTimer">A <see cref="SdkTimer"/> to schedule load of initial values</param>
        public NamedValueCache(IDataProvider<EntityList<NamedValueDTO>> dataProvider, ExceptionHandlingStrategy exceptionStrategy, string cacheName, SdkTimer sdkTimer)
        {
            if (sdkTimer == null)
            {
                throw new ArgumentNullException(nameof(sdkTimer));
            }

            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _namedValues = new Dictionary<int, string>();
            _exceptionStrategy = exceptionStrategy;
            _cacheName = cacheName;
            _timerOptions = new TimerOptions { Context = $"NamedValueCache-{_cacheName}", Name = "FetchAndMerge", MeasurementUnit = Unit.Requests };

            sdkTimer.Elapsed += LoadInitialValues;
            sdkTimer.FireOnce(sdkTimer.DueTime);
        }

        private void LoadInitialValues(object sender, EventArgs e)
        {
            IsValueDefined(1);
        }

        /// <summary>
        /// Asynchronously gets a match stats descriptions specified by the language specified by <c>culture</c>
        /// </summary>
        /// <returns>A value indicating whether the data was successfully fetched</returns>
        private bool FetchAndMerge()
        {
            EntityList<NamedValueDTO> record;
            try
            {
                using (SdkMetricsFactory.MetricsRoot.Measure.Timer.Time(_timerOptions))
                {
                    record = _dataProvider.GetDataAsync().GetAwaiter().GetResult();
                }
            }
            catch (AggregateException ex)
            {
                ExecutionLog.LogError(ex.InnerException, $"{_cacheName}: An exception occurred while fetching named values");
                return false;
            }

            foreach (var item in record.Items)
            {
                _namedValues[item.Id] = item.Description;
            }

            var logMsg = $"{_cacheName}: {record.Items.Count()} items retrieved.";
            CacheLog.LogInformation(logMsg);

            return true;
        }

        /// <summary>
        /// Determines whether specified id is present int the cache
        /// </summary>
        /// <param name="id">The id to be tested</param>
        /// <returns>True if the value is defined in the cache; False otherwise</returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsValueDefined(int id)
        {
            lock (_lock)
            {
                if (!_dataFetched)
                {
                    _dataFetched = FetchAndMerge();
                }
            }
            return _namedValues.ContainsKey(id);
        }

        /// <summary>
        /// Gets the <see cref="INamedValue" /> specified by it's id
        /// </summary>
        /// <param name="id">The id of the <see cref="INamedValue" /> to retrieve</param>
        /// <returns>The specified <see cref="INamedValue" /></returns>
        public INamedValue GetNamedValue(int id)
        {
            lock (_lock)
            {
                if (!_dataFetched)
                {
                    _dataFetched = FetchAndMerge();
                }
            }

            if (_namedValues.TryGetValue(id, out var description))
            {
                return new NamedValue(id, description);
            }

            if (_exceptionStrategy == ExceptionHandlingStrategy.THROW)
            {
                throw new ArgumentOutOfRangeException($"{_cacheName}: Cache item missing for id={id}.");
            }

            return new NamedValue(id);
        }
    }
}
