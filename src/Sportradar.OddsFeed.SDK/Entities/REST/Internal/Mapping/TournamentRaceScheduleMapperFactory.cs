﻿// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// Creates mapper for race tournament schedule
    /// </summary>
    internal class TournamentRaceScheduleMapperFactory : ISingleTypeMapperFactory<raceScheduleEndpoint, EntityList<SportEventSummaryDto>>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance for tournament schedule
        /// </summary>
        /// <param name="data">A <see cref="raceScheduleEndpoint" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<EntityList<SportEventSummaryDto>> CreateMapper(raceScheduleEndpoint data)
        {
            return new TournamentRaceScheduleMapper(data);
        }
    }
}
