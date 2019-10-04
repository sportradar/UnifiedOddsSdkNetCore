/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{ITournamentDetails}" /> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{tournamentInfoType, ITournamentDetails}" />
    public class TournamentInfoMapperFactory : ISingleTypeMapperFactory<tournamentInfoEndpoint, TournamentInfoDTO>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{TournamentInfoDTO}" /> instance
        /// </summary>
        /// <param name="data">A <see cref="tournamentInfoEndpoint" /> instance which the created <see cref="ISingleTypeMapper{TournamentInfoDTO}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{TournamentInfoDTO}" /> instance</returns>
        public ISingleTypeMapper<TournamentInfoDTO> CreateMapper(tournamentInfoEndpoint data)
        {
            return new TournamentInfoMapper(data);
        }
    }
}
