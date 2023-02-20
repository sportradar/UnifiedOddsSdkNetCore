/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using App.Metrics;
using App.Metrics.Timer;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
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

        /// <summary>
        /// An <see cref="object"/> used to sync  access to shared members
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// A value indicating whether the data was already fetched
        /// </summary>
        private bool _dataFetched;

        private readonly TimerOptions _timerOptions = new TimerOptions { Context = "NamedValueCache", Name = "GetAsync", MeasurementUnit = Unit.Requests };

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedValueCache"/> class.
        /// </summary>
        /// <param name="dataProvider">A <see cref="IDataProvider{T}"/> used to get the named values</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how potential exceptions should be handled</param>
        public NamedValueCache(IDataProvider<EntityList<NamedValueDTO>> dataProvider, ExceptionHandlingStrategy exceptionStrategy)
        {
            Guard.Argument(dataProvider, nameof(dataProvider)).NotNull();

            _dataProvider = dataProvider;
            _namedValues = new Dictionary<int, string>();
            _exceptionStrategy = exceptionStrategy;
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
                    record = _dataProvider.GetDataAsync().Result;
                }
            }
            catch (AggregateException ex)
            {
                ExecutionLog.LogError(ex.InnerException, "An exception occurred while fetching named values");
                return false;
            }

            foreach (var item in record.Items)
            {
                _namedValues[item.Id] = item.Description;
            }

            var logMsg = $"{record.Items.Count()} items retrieved.";
            CacheLog.LogDebug(logMsg);

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
                throw new ArgumentOutOfRangeException($"Cache item missing for id={id}.");
            }

            return new NamedValue(id);
        }
    }
}
