// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{ITournamentDetails}" /> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{tournamentInfoType, ITournamentDetails}" />
    internal class TournamentInfoMapperFactory : ISingleTypeMapperFactory<tournamentInfoEndpoint, TournamentInfoDto>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{TournamentInfoDto}" /> instance
        /// </summary>
        /// <param name="data">A <see cref="tournamentInfoEndpoint" /> instance which the created <see cref="ISingleTypeMapper{TournamentInfoDto}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{TournamentInfoDto}" /> instance</returns>
        public ISingleTypeMapper<TournamentInfoDto> CreateMapper(tournamentInfoEndpoint data)
        {
            return new TournamentInfoMapper(data);
        }
    }
}
