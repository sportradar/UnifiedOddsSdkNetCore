// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    internal class TournamentInfoMapper : ISingleTypeMapper<TournamentInfoDto>
    {
        /// <summary>
        /// A <see cref="tournamentInfoEndpoint"/> instance containing detailed tournament info
        /// </summary>
        private readonly tournamentInfoEndpoint _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentInfoMapper"/> class
        /// </summary>
        /// <param name="data">A <see cref="tournamentInfoEndpoint"/> instance containing tournament detail information</param>
        internal TournamentInfoMapper(tournamentInfoEndpoint data)
        {
            Guard.Argument(data, nameof(data)).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to instance of <see cref="TournamentInfoDto"/>
        /// </summary>
        /// <returns>The created <see cref="TournamentInfoDto"/> instance</returns>
        TournamentInfoDto ISingleTypeMapper<TournamentInfoDto>.Map()
        {
            return new TournamentInfoDto(_data);
        }
    }
}
