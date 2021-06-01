﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;
using System;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// A <see cref="IDataProvider{ISportEventsSchedule}"/> used to retrieve sport events scheduled for a specified date
    /// or currently live sport events
    /// </summary>
    /// <seealso cref="DataProvider{scheduleType, EntityList}" />
    /// <seealso cref="IDataProvider{EntityList}" />
    internal class DateScheduleProvider : DataProvider<scheduleEndpoint, EntityList<SportEventSummaryDTO>>
    {
        /// <summary>
        /// An address format used to retrieve live sport events
        /// </summary>
        private readonly string _liveScheduleUriFormat;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateScheduleProvider"/> class.
        /// </summary>
        /// <param name="liveScheduleUriFormat">An address format used to retrieve live sport events</param>
        /// <param name="dateScheduleUriFormat">An address format used to retrieve sport events for a specified date</param>
        /// <param name="fetcher">A <see cref="IDataFetcher" /> used to fetch the data</param>
        /// <param name="deserializer">A <see cref="IDeserializer{scheduleType}" /> used to deserialize the fetch data</param>
        /// <param name="mapperFactory">A <see cref="ISingleTypeMapperFactory{scheduleType, EntityList}" /> used to construct instances of <see cref="ISingleTypeMapper{ISportEventsSchedule}" /></param>
        public DateScheduleProvider(
            string liveScheduleUriFormat,
            string dateScheduleUriFormat,
            IDataFetcher fetcher,
            IDeserializer<scheduleEndpoint> deserializer,
            ISingleTypeMapperFactory<scheduleEndpoint, EntityList<SportEventSummaryDTO>> mapperFactory)
            : base(dateScheduleUriFormat, fetcher, deserializer, mapperFactory)
        {

            Guard.Argument(liveScheduleUriFormat, nameof(liveScheduleUriFormat)).NotNull().NotEmpty();
            Guard.Argument(dateScheduleUriFormat, nameof(dateScheduleUriFormat)).NotNull().NotEmpty();
            Guard.Argument(fetcher, nameof(fetcher)).NotNull();
            Guard.Argument(deserializer, nameof(deserializer)).NotNull();
            Guard.Argument(mapperFactory, nameof(mapperFactory)).NotNull();

            _liveScheduleUriFormat = liveScheduleUriFormat;
        }

        /// <summary>
        /// Constructs and returns an <see cref="Uri"/> instance used to retrieve resource with specified <code>id</code>
        /// </summary>
        /// <param name="identifiers">Identifiers uniquely identifying the data to fetch</param>
        /// <returns>an <see cref="Uri"/> instance used to retrieve resource with specified <code>identifiers</code></returns>
        protected override Uri GetRequestUri(params object[] identifiers)
        {
            return identifiers.Length == 1
                ? new Uri(string.Format(_liveScheduleUriFormat, identifiers[0], "live"))
                : base.GetRequestUri(identifiers);
        }
    }
}
