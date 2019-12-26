/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Creates mapper for tournament seasons
    /// </summary>
    public class TournamentSeasonsMapperFactory : ISingleTypeMapperFactory<tournamentSeasons, TournamentSeasonsDTO>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance for tournament seasons
        /// </summary>
        /// <param name="data">A <see cref="tournamentSeasons" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<TournamentSeasonsDTO> CreateMapper(tournamentSeasons data)
        {
            return new TournamentSeasonsMapper(data);
        }
    }
}
