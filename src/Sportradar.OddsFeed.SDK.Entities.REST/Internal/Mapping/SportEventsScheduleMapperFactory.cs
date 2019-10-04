/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{T}" /> instances used to map <see cref="scheduleEndpoint"/> to instances of <see cref="EntityList{SportEventSummaryDTO}"/>
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{ISportEventsSchedule,scheduleType}" />
    public class SportEventsScheduleMapperFactory : ISingleTypeMapperFactory<scheduleEndpoint, EntityList<SportEventSummaryDTO>>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance
        /// </summary>
        /// <param name="data">A <see cref="scheduleEndpoint" /> instance which the created <see cref="ISingleTypeMapper{ISportEventsSchedule}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{ISportEventsSchedule}" /> instance</returns>
        public ISingleTypeMapper<EntityList<SportEventSummaryDTO>> CreateMapper(scheduleEndpoint data)
        {
            return new SportEventsScheduleMapper(data);
        }
    }
}
