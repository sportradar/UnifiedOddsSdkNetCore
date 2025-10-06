// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal
{
    /// <summary>
    /// A <see cref="IDataProvider{ISportEventsSchedule}"/> used to retrieve list of sport events
    /// </summary>
    /// <seealso cref="DataProvider{scheduleEndpoint, EntityList}" />
    /// <seealso cref="IDataProvider{EntityList}" />
    internal class ListSportEventsProvider : DataProvider<scheduleEndpoint, EntityList<SportEventSummaryDto>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListSportEventsProvider"/> class
        /// </summary>
        /// <param name="baseUriFormat">An address format used to retrieve list of sport events</param>
        /// <param name="fetcher">A <see cref="IDataFetcher" /> used to fetch the data</param>
        /// <param name="deserializer">A <see cref="IDeserializer{scheduleEndpoint}" /> used to deserialize the fetch data</param>
        /// <param name="mapperFactory">A <see cref="ISingleTypeMapperFactory{scheduleEndpoint,EntityList}" /> used to construct instances of <see cref="ISingleTypeMapper{ISportEventsSchedule}" /></param>
        public ListSportEventsProvider(string baseUriFormat,
                                       IDataFetcher fetcher,
                                       IDeserializer<scheduleEndpoint> deserializer,
                                       ISingleTypeMapperFactory<scheduleEndpoint, EntityList<SportEventSummaryDto>> mapperFactory)
            : base(baseUriFormat, fetcher, deserializer, mapperFactory)
        {
            Guard.Argument(baseUriFormat, nameof(baseUriFormat)).NotNull().NotEmpty();
            Guard.Argument(fetcher, nameof(fetcher)).NotNull();
            Guard.Argument(deserializer, nameof(deserializer)).NotNull();
            Guard.Argument(mapperFactory, nameof(mapperFactory)).NotNull();
        }
    }
}
