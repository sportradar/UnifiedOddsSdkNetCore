/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// Provides information about tournament (when fetching the all tournaments for all sports)
    /// </summary>
    /// <seealso cref="TournamentInfoDto"/>
    internal class TournamentDto : SportEntityDto
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
        public SportEntityDto Sport { get; }

        /// <summary>
        /// A category to which this tournament belongs to
        /// </summary>
        public CategorySummaryDto Category { get; }

        /// <summary>
        /// Gets a <see cref="SportEntityDto"/> representing the current season of the tournament
        /// </summary>
        public SeasonDto CurrentSeason { get; }

        /// <summary>
        /// Gets a <see cref="SeasonCoverageDto"/> containing information about the tournament coverage.
        /// </summary>
        public SeasonCoverageDto SeasonCoverage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentDto"/> class
        /// </summary>
        /// <param name="tournament">The <see cref="tournament"/> used for creating instance</param>
        internal TournamentDto(tournament tournament)
            : base(tournament.id, tournament.name)
        {
            Guard.Argument(tournament, nameof(tournament)).NotNull();

            Scheduled = tournament.scheduledSpecified
                ? (DateTime?)tournament.scheduled
                : null;

            ScheduledEnd = tournament.scheduled_endSpecified
                ? (DateTime?)tournament.scheduled_end
                : null;

            Sport = new SportEntityDto(tournament.sport.id, tournament.sport.name);

            //TODO: check for 'vf': is it still required?
            Category = tournament.category == null && tournament.id.StartsWith("vf", StringComparison.InvariantCultureIgnoreCase)
                ? CreateFakeCategory()
                : new CategorySummaryDto(tournament.category?.id, tournament.category?.name, tournament.category?.country_code);

            CurrentSeason = null;
            SeasonCoverage = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentDto"/> class.
        /// </summary>
        /// <param name="tournament">The <see cref="tournamentExtended"/> used for creating instance</param>
        internal TournamentDto(tournamentExtended tournament)
            : base(tournament.id, tournament.name)
        {
            Guard.Argument(tournament, nameof(tournament)).NotNull();

            Scheduled = tournament.scheduledSpecified
                ? (DateTime?)tournament.scheduled
                : null;

            ScheduledEnd = tournament.scheduled_endSpecified
                ? (DateTime?)tournament.scheduled_end
                : null;

            Sport = new SportEntityDto(tournament.sport.id, tournament.sport.name);

            Category = tournament.category == null && tournament.id.StartsWith("vf", StringComparison.InvariantCultureIgnoreCase)
                ? CreateFakeCategory()
                : new CategorySummaryDto(tournament.category?.id, tournament.category?.name, tournament.category?.country_code);

            CurrentSeason = tournament.current_season == null
                ? null
                : new SeasonDto(tournament.current_season);

            SeasonCoverage = tournament.season_coverage_info == null
                ? null
                : new SeasonCoverageDto(tournament.season_coverage_info);
        }

        private CategorySummaryDto CreateFakeCategory()
        {
            return new CategorySummaryDto("vf:category:3816", "Soccer.VirtualFootballCup", null);
        }
    }
}
