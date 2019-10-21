/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object for tournament season
    /// </summary>
    public class TournamentSeasonsDTO
    {
        /// <summary>
        /// Gets the tournament
        /// </summary>
        /// <value>The tournament</value>
        public TournamentInfoDTO Tournament { get; }

        /// <summary>
        /// Gets the seasons
        /// </summary>
        /// <value>The seasons</value>
        public IEnumerable<SeasonDTO> Seasons { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying when the associated message was generated (on the server side)
        /// </summary>
        public DateTime? GeneratedAt { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentSeasonsDTO"/> class
        /// </summary>
        /// <param name="item">The item</param>
        internal TournamentSeasonsDTO(tournamentSeasons item)
        {
            Guard.Argument(item).NotNull();

            Tournament = new TournamentInfoDTO(item.tournament);

            if (item.seasons != null && item.seasons.Any())
            {
                Seasons = item.seasons.Select(s => new SeasonDTO(s));
            }

            GeneratedAt = item.generated_atSpecified ? item.generated_at : (DateTime?) null;
        }
    }
}
