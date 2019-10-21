/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// Provides information about tournament (when fetching the all tournaments for all sports)
    /// </summary>
    /// <seealso cref="TournamentInfoDTO"/>
    public class TournamentDTO : SportEntityDTO
    {
        /// <summary>
        /// Gets a <see cref="DateTime"/> indicating when the tournament is scheduled to start, or a null reference if value is not known.
        /// </summary>
        public DateTime? Scheduled { get; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> indicating when the tournament is scheduled to end, or a null reference if value is not known.
        /// </summary>
        public DateTime? ScheduledEnd { get; }

        /// <summary>
        /// A sport to which this tournament belongs to
        /// </summary>
        public SportEntityDTO Sport { get; }

        /// <summary>
        /// A category to which this tournament belongs to
        /// </summary>
        public CategorySummaryDTO Category { get; }

        /// <summary>
        /// Gets a <see cref="SportEntityDTO"/> representing the current season of the tournament
        /// </summary>
        public SeasonDTO CurrentSeason { get; }

        /// <summary>
        /// Gets a <see cref="SeasonCoverageDTO"/> containing information about the tournament coverage.
        /// </summary>
        public SeasonCoverageDTO SeasonCoverage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentDTO"/> class
        /// </summary>
        /// <param name="tournament">The <see cref="tournament"/> used for creating instance</param>
        internal TournamentDTO(tournament tournament)
            :base(tournament.id, tournament.name)
        {
            Guard.Argument(tournament).NotNull();

            Scheduled = tournament.scheduledSpecified
                ? (DateTime?) tournament.scheduled
                : null;

            ScheduledEnd = tournament.scheduled_endSpecified
                ? (DateTime?) tournament.scheduled_end
                : null;

            Sport = new SportEntityDTO(tournament.sport.id, tournament.sport.name);

            //TODO: check for 'vf': is it still required?
            Category = tournament.category == null && tournament.id.StartsWith("vf")
                ? CreateFakeCategory()
                : new CategorySummaryDTO(tournament.category?.id, tournament.category?.name, tournament.category?.country_code);

            CurrentSeason = null;
            SeasonCoverage = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentDTO"/> class.
        /// </summary>
        /// <param name="tournament">The <see cref="tournamentExtended"/> used for creating instance</param>
        internal TournamentDTO(tournamentExtended tournament)
            : base(tournament.id, tournament.name)
        {
            Guard.Argument(tournament).NotNull();

            Scheduled = tournament.scheduledSpecified
                ? (DateTime?)tournament.scheduled
                : null;

            ScheduledEnd = tournament.scheduled_endSpecified
                ? (DateTime?)tournament.scheduled_end
                : null;

            Sport = new SportEntityDTO(tournament.sport.id, tournament.sport.name);

            Category = tournament.category == null && tournament.id.StartsWith("vf")
                ? CreateFakeCategory()
                : new CategorySummaryDTO(tournament.category?.id, tournament.category?.name, tournament.category?.country_code);

            CurrentSeason = tournament.current_season == null
                ? null
                : new SeasonDTO(tournament.current_season);

            SeasonCoverage = tournament.season_coverage_info == null
                ? null
                : new SeasonCoverageDTO(tournament.season_coverage_info);
        }

        private CategorySummaryDTO CreateFakeCategory()
        {
            return new CategorySummaryDTO("vf:category:3816", "Soccer.VirtualFootballCup", null);
        }
    }
}
