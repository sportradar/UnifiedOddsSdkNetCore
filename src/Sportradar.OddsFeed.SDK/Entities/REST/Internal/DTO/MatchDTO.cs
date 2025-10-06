// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object containing basic information about a match
    /// </summary>
    internal class MatchDto : CompetitionDto
    {
        /// <summary>
        /// Gets a <see cref="SportEntityDto"/> instance specifying the season to which the sport event associated with the current instance belongs to.
        /// </summary>
        public SeasonDto Season { get; }

        /// <summary>
        /// Gets a <see cref="RoundDto"/> representing the tournament round to which the associated sport event belongs to.
        /// </summary>
        public RoundDto Round { get; }

        /// <summary>
        /// Gets a <see cref="TournamentDto"/> representing the tournament to which the associated sport event belongs to.
        /// </summary>
        public TournamentDto Tournament { get; }

        /// <summary>
        /// Gets a <see cref="CoverageInfoDto"/>
        /// </summary>
        public CoverageInfoDto Coverage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchDto"/> class
        /// </summary>
        /// <param name="sportEvent">A <see cref="sportEvent"/> instance containing basic information about the sport event</param>
        internal MatchDto(sportEvent sportEvent)
            : base(sportEvent)
        {
            Guard.Argument(sportEvent, nameof(sportEvent)).NotNull();

            if (sportEvent.season != null)
            {
                Season = new SeasonDto(sportEvent.season);
            }
            if (sportEvent.tournament_round != null)
            {
                Round = new RoundDto(sportEvent.tournament_round);
            }
            if (sportEvent.tournament != null)
            {
                Guard.Argument(sportEvent.tournament.id, nameof(sportEvent.tournament.id)).NotNull().NotEmpty();
                Guard.Argument(sportEvent.tournament.name, nameof(sportEvent.tournament.name)).NotNull().NotEmpty();
                Tournament = new TournamentDto(sportEvent.tournament);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchDto"/> class
        /// </summary>
        /// <param name="matchSummary">A <see cref="matchSummaryEndpoint"/> instance containing basic information about the sport event</param>
        internal MatchDto(matchSummaryEndpoint matchSummary)
            : base(matchSummary)
        {
            Guard.Argument(matchSummary, nameof(matchSummary)).NotNull();

            if (matchSummary.sport_event.season != null)
            {
                Guard.Argument(matchSummary.sport_event.season.id, nameof(matchSummary.sport_event.season.id)).NotNull().NotEmpty();
                Guard.Argument(matchSummary.sport_event.season.name, nameof(matchSummary.sport_event.season.name)).NotNull().NotEmpty();
                Season = new SeasonDto(matchSummary.sport_event.season);
            }
            if (matchSummary.sport_event.tournament_round != null)
            {
                Round = new RoundDto(matchSummary.sport_event.tournament_round);
            }
            if (matchSummary.sport_event.tournament != null)
            {
                Guard.Argument(matchSummary.sport_event.tournament.id, nameof(matchSummary.sport_event.tournament.id)).NotNull().NotEmpty();
                Guard.Argument(matchSummary.sport_event.tournament.name, nameof(matchSummary.sport_event.tournament.name)).NotNull().NotEmpty();
                Tournament = new TournamentDto(matchSummary.sport_event.tournament);
            }
            if (matchSummary.coverage_info != null)
            {
                Coverage = new CoverageInfoDto(matchSummary.coverage_info);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchDto"/> class
        /// </summary>
        /// <param name="fixture">A <see cref="fixture"/> instance containing basic information about the sport event</param>
        /// <remarks>Not all properties are filled via fixture (i.e.Venue, Conditions,..)</remarks>
        internal MatchDto(fixture fixture)
            : this(new matchSummaryEndpoint
            {
                sport_event = new sportEvent
                {
                    id = fixture.id,
                    name = fixture.name,
                    type = fixture.type,
                    stage_type = fixture.stage_type,
                    scheduledSpecified = fixture.scheduledSpecified,
                    scheduled = fixture.scheduled,
                    scheduled_endSpecified = fixture.scheduled_endSpecified,
                    scheduled_end = fixture.scheduled_end,
                    liveodds = fixture.liveodds,
                    season = fixture.season,
                    tournament = fixture.tournament,
                    tournament_round = fixture.tournament_round,
                    competitors = fixture.competitors?.Select(t => new teamCompetitor
                    {
                        abbreviation = t.abbreviation,
                        country = t.country,
                        id = t.id,
                        name = t.name,
                        qualifier = t.qualifier,
                        @virtual = t.@virtual,
                        virtualSpecified = t.virtualSpecified,
                        country_code = t.country_code,
                        reference_ids = t.reference_ids,
                        division = t.division,
                        divisionSpecified = t.divisionSpecified,
                        state = t.state
                    })
                                                                      .ToArray(),
                    parent = fixture.parent,
                    races = fixture.races,
                    status = fixture.status,
                    replaced_by = fixture.replaced_by,
                    next_live_time = fixture.next_live_time,
                    sport_event_conditions = fixture.sport_event_conditions,
                    start_time_tbdSpecified = fixture.start_time_tbdSpecified,
                    start_time_tbd = fixture.start_time_tbd
                }
            })
        {
            Venue = fixture.venue == null
                        ? null
                        : new VenueDto(fixture.venue);

            if (fixture.coverage_info != null)
            {
                Coverage = new CoverageInfoDto(fixture.coverage_info);
            }
        }
    }
}
