/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    public class BasicTournamentDTO : SportEventSummaryDTO
    {
        /// <summary>
        /// Gets the tournament coverage
        /// </summary>
        /// <value>The tournament coverage</value>
        public TournamentCoverageDTO TournamentCoverage { get; }

        /// <summary>
        /// Gets the category
        /// </summary>
        /// <value>The category</value>
        public URN Category { get; }

        /// <summary>
        /// Gets the competitors
        /// </summary>
        /// <value>The competitors</value>
        public IEnumerable<CompetitorDTO> Competitors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicTournamentDTO"/> class.
        /// </summary>
        /// <param name="sportEvent">The <see cref="sportEvent"/> used for creating instance</param>
        internal BasicTournamentDTO(sportEvent sportEvent)
            : base(sportEvent)
        {
            Guard.Argument(sportEvent, nameof(sportEvent)).NotNull();

            TournamentCoverage = null;
            Category = sportEvent.tournament.category == null
                ? null
                : URN.Parse(sportEvent.tournament.category.id);
            Competitors = sportEvent.competitors.Select(s => new CompetitorDTO(s));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicTournamentDTO"/> class.
        /// </summary>
        /// <param name="tournamentInfo">The <see cref="tournament"/> used for creating instance</param>
        internal BasicTournamentDTO(tournamentInfoEndpoint tournamentInfo)
            : base(new sportEvent
            {
                id = tournamentInfo.tournament.id,
                name = tournamentInfo.tournament.name,
                scheduled = tournamentInfo.tournament.scheduled,
                scheduledSpecified =  tournamentInfo.tournament.scheduledSpecified,
                scheduled_end =  tournamentInfo.tournament.scheduled_end,
                scheduled_endSpecified = tournamentInfo.tournament.scheduled_endSpecified,
                tournament = tournamentInfo.tournament,
                type =  null

            })
        {
            Guard.Argument(tournamentInfo, nameof(tournamentInfo)).NotNull();

            TournamentCoverage = new TournamentCoverageDTO(tournamentInfo.coverage_info);
            Category = tournamentInfo.tournament.category == null
                ? null
                : URN.Parse(tournamentInfo.tournament.category.id);
            Competitors = tournamentInfo.competitors.Select(s => new CompetitorDTO(s));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicTournamentDTO"/> class.
        /// </summary>
        /// <param name="tournament">The <see cref="tournamentExtended"/> used for creating instance</param>
        internal BasicTournamentDTO(tournamentExtended tournament)
            : base(new sportEvent
            {
                id = tournament.id,
                name = tournament.name,
                scheduled = tournament.scheduled,
                scheduledSpecified = tournament.scheduledSpecified,
                scheduled_end = tournament.scheduled_end,
                scheduled_endSpecified = tournament.scheduled_endSpecified,
                tournament = tournament,
                type = null
            })
        {
            Guard.Argument(tournament, nameof(tournament)).NotNull();

            //TournamentCoverage = new TournamentCoverageDTO(tournament.season_coverage_info);
            Category = tournament.category == null
                ? null
                : URN.Parse(tournament.category.id);
            Competitors = tournament.competitors.Select(s => new CompetitorDTO(s));
        }
    }
}
