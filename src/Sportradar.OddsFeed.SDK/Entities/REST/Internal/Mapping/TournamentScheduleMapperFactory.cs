/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Creates mapper for tournament schedule
    /// </summary>
    public class TournamentScheduleMapperFactory : ISingleTypeMapperFactory<tournamentSchedule, EntityList<SportEventSummaryDTO>>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance for tournament schedule
        /// </summary>
        /// <param name="data">A <see cref="tournamentSchedule" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<EntityList<SportEventSummaryDTO>> CreateMapper(tournamentSchedule data)
        {
            return new TournamentScheduleMapper(data);
        }
    }
}
