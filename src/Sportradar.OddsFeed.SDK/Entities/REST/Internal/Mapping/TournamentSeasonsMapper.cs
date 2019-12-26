/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
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
            Guard.Argument(data, nameof(data)).NotNull();

            _data = data;
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
