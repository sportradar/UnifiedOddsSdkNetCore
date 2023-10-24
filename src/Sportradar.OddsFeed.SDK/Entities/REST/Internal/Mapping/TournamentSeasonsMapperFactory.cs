/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// Creates mapper for tournament seasons
    /// </summary>
    internal class TournamentSeasonsMapperFactory : ISingleTypeMapperFactory<tournamentSeasons, TournamentSeasonsDto>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance for tournament seasons
        /// </summary>
        /// <param name="data">A <see cref="tournamentSeasons" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<TournamentSeasonsDto> CreateMapper(tournamentSeasons data)
        {
            return new TournamentSeasonsMapper(data);
        }
    }
}
