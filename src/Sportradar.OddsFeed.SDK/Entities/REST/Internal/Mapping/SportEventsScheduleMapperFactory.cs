// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{T}" /> instances used to map <see cref="scheduleEndpoint"/> to instances of <see cref="EntityList{SportEventSummaryDto}"/>
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{ISportEventsSchedule,scheduleType}" />
    internal class SportEventsScheduleMapperFactory : ISingleTypeMapperFactory<scheduleEndpoint, EntityList<SportEventSummaryDto>>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance
        /// </summary>
        /// <param name="data">A <see cref="scheduleEndpoint" /> instance which the created <see cref="ISingleTypeMapper{ISportEventsSchedule}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{ISportEventsSchedule}" /> instance</returns>
        public ISingleTypeMapper<EntityList<SportEventSummaryDto>> CreateMapper(scheduleEndpoint data)
        {
            return new SportEventsScheduleMapper(data);
        }
    }
}
