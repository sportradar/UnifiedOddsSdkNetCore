/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Creates mapper for sport event list
    /// </summary>
    public class ListSportEventsMapperFactory : ISingleTypeMapperFactory<scheduleEndpoint, EntityList<SportEventSummaryDTO>>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance for schedule
        /// </summary>
        /// <param name="data">A <see cref="scheduleEndpoint" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<EntityList<SportEventSummaryDTO>> CreateMapper(scheduleEndpoint data)
        {
            return new ListSportEventsMapper(data);
        }
    }
}
