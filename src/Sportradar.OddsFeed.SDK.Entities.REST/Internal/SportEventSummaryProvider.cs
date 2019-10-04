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
    /// A <see cref="IDataProvider{SportEventSummaryDTO}"/> used to retrieve sport event summary
    /// </summary>
    /// <seealso cref="DataProvider{RestMessage, EntityList}" />
    /// <seealso cref="IDataProvider{EntityList}" />
    public class SportEventSummaryProvider : DataProvider<RestMessage, EntityList<SportEventSummaryDTO>>
    {
        /// <summary>
        /// An address format used to retrieve sport event summary
        /// </summary>
        private readonly string _sportEventSummaryUriFormat;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventSummaryProvider"/> class
        /// </summary>
        /// <param name="sportEventSummaryUriFormat">An address format used to retrieve sport event summary</param>
        /// <param name="fetcher">A <see cref="IDataFetcher" /> used to fetch the data</param>
        /// <param name="deserializer">A <see cref="IDeserializer{scheduleType}" /> used to deserialize the fetch data</param>
        /// <param name="mapperFactory">A <see cref="ISingleTypeMapperFactory{scheduleType, EntityList}" /> used to construct instances of <see cref="ISingleTypeMapper{ISportEventsSchedule}" /></param>
        public SportEventSummaryProvider(
            string sportEventSummaryUriFormat,
            IDataFetcher fetcher,
            IDeserializer<RestMessage> deserializer,
            ISingleTypeMapperFactory<RestMessage, EntityList<SportEventSummaryDTO>> mapperFactory)
            :base(sportEventSummaryUriFormat, fetcher, deserializer, mapperFactory)
        {

            Contract.Requires(!string.IsNullOrEmpty(sportEventSummaryUriFormat));
            Contract.Requires(fetcher != null);
            Contract.Requires(deserializer != null);
            Contract.Requires(mapperFactory != null);

            _sportEventSummaryUriFormat = sportEventSummaryUriFormat;
        }

        /// <summary>
        /// Defines object invariants used by the code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!string.IsNullOrWhiteSpace(_sportEventSummaryUriFormat));
        }


        /// <summary>
        /// Constructs and returns an <see cref="Uri"/> instance used to retrieve resource with specified <code>id</code>
        /// </summary>
        /// <param name="identifiers">Identifiers uniquely identifying the data to fetch</param>
        /// <returns>an <see cref="Uri"/> instance used to retrieve resource with specified <code>identifiers</code></returns>
        protected override Uri GetRequestUri(params object[] identifiers)
        {
            return identifiers.Length == 1
                ? new Uri(string.Format(_sportEventSummaryUriFormat, identifiers))
                : base.GetRequestUri(identifiers);
        }
    }
}
