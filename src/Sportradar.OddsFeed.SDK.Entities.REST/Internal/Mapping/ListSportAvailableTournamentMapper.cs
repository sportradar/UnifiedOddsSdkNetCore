/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Maps <see cref="sportTournamentsEndpoint"/> instances to <see cref="TournamentInfoDTO" /> instance
    /// </summary>
    internal class ListSportAvailableTournamentMapper : ISingleTypeMapper<EntityList<TournamentInfoDTO>>
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
            Contract.Requires(data != null);

            _data = data;
        }

        /// <summary>
        /// Defines object invariants used by the code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_data != null);
        }

        /// <summary>
        /// Maps it's data to <see cref="EntityList{TournamentInfoDTO}"/> instance
        /// </summary>
        /// <returns>Constructed <see cref="EntityList{TournamentInfoDTO}"/> instance</returns>
        public EntityList<TournamentInfoDTO> Map()
        {
            var events = _data.tournaments.Select(s => new TournamentInfoDTO(s)).ToList();
            return new EntityList<TournamentInfoDTO>(events);
        }
    }
}
