/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// A <see cref="IDataProvider{ISportEventsSchedule}"/> used to retrieve sport events scheduled for a specified date
    /// or currently live sport events
    /// </summary>
    /// <seealso cref="DataProvider{scheduleType, EntityList}" />
    /// <seealso cref="IDataProvider{EntityList}" />
    public class DateScheduleProvider : DataProvider<scheduleEndpoint, EntityList<SportEventSummaryDTO>>
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
            :base(dateScheduleUriFormat, fetcher, deserializer, mapperFactory)
        {

            Contract.Requires(!string.IsNullOrEmpty(liveScheduleUriFormat));
            Contract.Requires(!string.IsNullOrWhiteSpace(dateScheduleUriFormat));
            Contract.Requires(fetcher != null);
            Contract.Requires(deserializer != null);
            Contract.Requires(mapperFactory != null);

            _liveScheduleUriFormat = liveScheduleUriFormat;
        }

        /// <summary>
        /// Defines object invariants used by the code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!string.IsNullOrWhiteSpace(_liveScheduleUriFormat));
        }


        /// <summary>
        /// Constructs and returns an <see cref="Uri"/> instance used to retrieve resource with specified <code>id</code>
        /// </summary>
        /// <param name="identifiers">Identifiers uniquely identifying the data to fetch</param>
        /// <returns>an <see cref="Uri"/> instance used to retrieve resource with specified <code>identifiers</code></returns>
        protected override Uri GetRequestUri(params object[] identifiers)
        {
            return identifiers.Length == 1
                ? new Uri(string.Format(_liveScheduleUriFormat, identifiers))
                : base.GetRequestUri(identifiers);
        }
    }
}
