// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object for tournament season
    /// </summary>
    internal class TournamentSeasonsDto
    {
        /// <summary>
        /// Gets the tournament
        /// </summary>
        /// <value>The tournament</value>
        public TournamentInfoDto Tournament { get; }

        /// <summary>
        /// Gets the seasons
        /// </summary>
        /// <value>The seasons</value>
        public IEnumerable<SeasonDto> Seasons { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying when the associated message was generated (on the server side)
        /// </summary>
        public DateTime? GeneratedAt { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentSeasonsDto"/> class
        /// </summary>
        /// <param name="item">The item</param>
        internal TournamentSeasonsDto(tournamentSeasons item)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            Tournament = new TournamentInfoDto(item.tournament);

            if (item.seasons != null && item.seasons.Any())
            {
                Seasons = item.seasons.Select(s => new SeasonDto(s));
            }

            GeneratedAt = item.generated_atSpecified ? item.generated_at : (DateTime?)null;
        }
    }
}
