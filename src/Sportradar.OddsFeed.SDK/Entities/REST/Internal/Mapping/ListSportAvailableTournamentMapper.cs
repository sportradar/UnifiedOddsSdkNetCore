// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// Maps <see cref="sportTournamentsEndpoint"/> instances to <see cref="TournamentInfoDto" /> instance
    /// </summary>
    internal class ListSportAvailableTournamentMapper : ISingleTypeMapper<EntityList<TournamentInfoDto>>
    {
        /// <summary>
        /// A <see cref="sportTournamentsEndpoint"/> instance containing schedule info
        /// </summary>
        private readonly sportTournamentsEndpoint _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListSportAvailableTournamentMapper"/> class
        /// </summary>
        /// <param name="data">>A <see cref="sportTournamentsEndpoint"/> instance containing schedule info</param>
        internal ListSportAvailableTournamentMapper(sportTournamentsEndpoint data)
        {
            Guard.Argument(data, nameof(data)).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to <see cref="EntityList{TournamentInfoDto}"/> instance
        /// </summary>
        /// <returns>Constructed <see cref="EntityList{TournamentInfoDto}"/> instance</returns>
        public EntityList<TournamentInfoDto> Map()
        {
            var events = _data.tournaments.Select(s => new TournamentInfoDto(s)).ToList();
            return new EntityList<TournamentInfoDto>(events);
        }
    }
}
