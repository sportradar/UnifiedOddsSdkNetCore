// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// Creates mapper for schedule for a specific date
    /// </summary>
    internal class DateScheduleMapperFactory : ISingleTypeMapperFactory<scheduleEndpoint, EntityList<SportEventSummaryDto>>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance for schedule for a date
        /// </summary>
        /// <param name="data">A <see cref="tournamentSchedule" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<EntityList<SportEventSummaryDto>> CreateMapper(scheduleEndpoint data)
        {
            return new DateScheduleMapper(data);
        }
    }
}
