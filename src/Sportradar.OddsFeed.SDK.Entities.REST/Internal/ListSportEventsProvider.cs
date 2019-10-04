/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// A <see cref="IDataProvider{ISportEventsSchedule}"/> used to retrieve list of sport events
    /// </summary>
    /// <seealso cref="DataProvider{scheduleEndpoint, EntityList}" />
    /// <seealso cref="IDataProvider{EntityList}" />
    public class ListSportEventsProvider : DataProvider<scheduleEndpoint, EntityList<SportEventSummaryDTO>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListSportEventsProvider"/> class
        /// </summary>
        /// <param name="baseUriFormat">An address format used to retrieve list of sport events</param>
        /// <param name="fetcher">A <see cref="IDataFetcher" /> used to fetch the data</param>
        /// <param name="deserializer">A <see cref="IDeserializer{scheduleEndpoint}" /> used to deserialize the fetch data</param>
        /// <param name="mapperFactory">A <see cref="ISingleTypeMapperFactory{scheduleEndpoint, EntityList}" /> used to construct instances of <see cref="ISingleTypeMapper{ISportEventsSchedule}" /></param>
        public ListSportEventsProvider(
            string baseUriFormat,
            IDataFetcher fetcher,
            IDeserializer<scheduleEndpoint> deserializer,
            ISingleTypeMapperFactory<scheduleEndpoint, EntityList<SportEventSummaryDTO>> mapperFactory)
            : base(baseUriFormat, fetcher, deserializer, mapperFactory)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(baseUriFormat));
            Contract.Requires(fetcher != null);
            Contract.Requires(deserializer != null);
            Contract.Requires(mapperFactory != null);
        }
    }
}
