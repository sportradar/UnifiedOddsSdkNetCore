/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Caching
{
    /// <summary>
    /// A default implementation of the <see cref="INamedValueCache"/>
    /// </summary>
    /// <seealso cref="INamedValueCache" />
    internal class NamedValueCache : INamedValueCache
    {
        private static readonly ILogger CacheLog = SdkLoggerFactory.GetLoggerForCache(typeof(NamedValueCache));

        private static readonly ILogger ExecutionLog = SdkLoggerFactory.GetLogger(typeof(NamedValueCache));

        public string CacheName { get; }

        private readonly ExceptionHandlingStrategy _exceptionStrategy;

        private readonly IDataProvider<EntityList<NamedValueDto>> _dataProvider;

        private readonly IDictionary<int, string> _namedValues;

        private readonly object _lock = new object();

        private bool _dataFetched;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedValueCache"/> class.
        /// </summary>
        /// <param name="cacheName">A name of the cache or the name of the values contained in this cache</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how potential exceptions should be handled</param>
        /// <param name="dataProvider">A <see cref="IDataProvider{T}"/> used to get the named values</param>
        /// <param name="sdkTimer">A <see cref="SdkTimer"/> to schedule load of initial values</param>
        public NamedValueCache(string cacheName, ExceptionHandlingStrategy exceptionStrategy, IDataProvider<EntityList<NamedValueDto>> dataProvider, ISdkTimer sdkTimer)
        {
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
            _namedValues = new Dictionary<int, string>();

            sdkTimer.Elapsed += LoadInitialValues;
            sdkTimer.FireOnce(((SdkTimer)sdkTimer).DueTime);
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
            EntityList<NamedValueDto> record;
            try
            {
                using (new TelemetryTracker(UofSdkTelemetry.NamedValueCache, "cache_name", CacheName))
                {
                    record = _dataProvider.GetDataAsync().GetAwaiter().GetResult();
                }
            }
            catch (AggregateException ex)
            {
                ExecutionLog.LogError(ex.InnerException, "{CacheName}: An exception occurred while fetching named values", CacheName);
                return false;
            }

            foreach (var item in record.Items)
            {
                _namedValues[item.Id] = item.Description;
            }

            CacheLog.LogInformation("{CacheName}: {Size} items retrieved", CacheName, record.Items.Count().ToString(CultureInfo.InvariantCulture));

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

            if (_exceptionStrategy == ExceptionHandlingStrategy.Throw)
            {
                throw new ArgumentOutOfRangeException($"{CacheName}: Cache item missing for id={id}");
            }

            return new NamedValue(id);
        }
    }
}
