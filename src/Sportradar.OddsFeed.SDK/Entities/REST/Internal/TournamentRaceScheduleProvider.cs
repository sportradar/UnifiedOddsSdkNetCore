﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
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
    /// <seealso cref="DataProvider{raceScheduleEndpoint, EntityList}" />
    /// <seealso cref="IDataProvider{EntityList}" />
    internal class TournamentRaceScheduleProvider : DataProvider<raceScheduleEndpoint, EntityList<SportEventSummaryDTO>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentRaceScheduleProvider"/> class
        /// </summary>
        /// <param name="dateScheduleUriFormat">An address format used to retrieve sport events for a specified date</param>
        /// <param name="fetcher">A <see cref="IDataFetcher" /> used to fetch the data</param>
        /// <param name="deserializer">A <see cref="IDeserializer{scheduleType}" /> used to deserialize the fetch data</param>
        /// <param name="mapperFactory">A <see cref="ISingleTypeMapperFactory{scheduleType, EntityList}" /> used to construct instances of <see cref="ISingleTypeMapper{ISportEventsSchedule}" /></param>
        public TournamentRaceScheduleProvider(string dateScheduleUriFormat,
                                              IDataFetcher fetcher,
                                              IDeserializer<raceScheduleEndpoint> deserializer,
                                              ISingleTypeMapperFactory<raceScheduleEndpoint, EntityList<SportEventSummaryDTO>> mapperFactory)
            : base(dateScheduleUriFormat, fetcher, deserializer, mapperFactory)
        {
            Guard.Argument(dateScheduleUriFormat, nameof(dateScheduleUriFormat)).NotNull().NotEmpty();
            Guard.Argument(fetcher, nameof(fetcher)).NotNull();
            Guard.Argument(deserializer, nameof(deserializer)).NotNull();
            Guard.Argument(mapperFactory, nameof(mapperFactory)).NotNull();
        }
    }
}
