/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    internal class TournamentInfoMapper : ISingleTypeMapper<TournamentInfoDTO>
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
            Contract.Requires(data != null);

            _data = data;
        }

        /// <summary>
        /// Maps it's data to instance of <see cref="TournamentInfoDTO"/>
        /// </summary>
        /// <returns>The created <see cref="TournamentInfoDTO"/> instance</returns>
        TournamentInfoDTO ISingleTypeMapper<TournamentInfoDTO>.Map()
        {
            return new TournamentInfoDTO(_data);
        }
    }
}
