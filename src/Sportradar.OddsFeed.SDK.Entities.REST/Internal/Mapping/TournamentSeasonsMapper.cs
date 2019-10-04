/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Maps <see cref="tournamentSeasons"/> instances to <see cref="TournamentSeasonsDTO" /> instance
    /// </summary>
    internal class TournamentSeasonsMapper : ISingleTypeMapper<TournamentSeasonsDTO>
    {
        /// <summary>
        /// A <see cref="tournamentSeasons"/> instance containing tournament seasons
        /// </summary>
        private readonly tournamentSeasons _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentSeasonsMapper"/> class
        /// </summary>
        /// <param name="data">>A <see cref="tournamentSeasons"/> instance containing tournament seasons</param>
        internal TournamentSeasonsMapper(tournamentSeasons data)
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
        /// Maps it's data to <see cref="TournamentSeasonsDTO"/> instance
        /// </summary>
        /// <returns>Constructed <see cref="TournamentSeasonsDTO"/> instance</returns>
        public TournamentSeasonsDTO Map()
        {
            return new TournamentSeasonsDTO(_data);
        }
    }
}
