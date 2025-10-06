// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Messages.Rest;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object containing basic information about a competition
    /// </summary>
    internal class CompetitionDto : SportEventSummaryDto
    {
        /// <summary>
        /// Gets the sport event status
        /// </summary>
        /// <value>The sport event status</value>
        public SportEventStatusDto SportEventStatus { get; }

        /// <summary>
        /// Gets a <see cref="BookingStatus"/> enum member specifying the booking status of the associated sport event
        /// </summary>
        /// <remarks>For more information see <see cref="BookingStatus"/> enumeration</remarks>
        public BookingStatus? BookingStatus { get; }

        /// <summary>
        /// Gets the venue info
        /// </summary>
        public VenueDto Venue { get; internal set; }

        /// <summary>
        /// Gets the sport event conditions
        /// </summary>
        public SportEventConditionsDto Conditions { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> representing the competitors of the associated sport event
        /// </summary>
        public IEnumerable<TeamCompetitorDto> Competitors { get; }

        /// <summary>
        /// Gets the home away competitors
        /// </summary>
        /// <value>The home away competitors</value>
        internal IDictionary<HomeAway, Urn> HomeAwayCompetitors { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying when the associated message was generated (on the server side)
        /// </summary>
        public DateTime? GeneratedAt { get; protected set; }

        /// <summary>
        /// Gets a <see cref="SportEventType"/> specifying the type of the associated sport event or a null reference if property is not applicable
        /// for the associated sport event
        /// </summary>
        /// <seealso cref="SportEventType"/>
        public SportEventType? Type { get; }

        /// <summary>
        /// Gets a <see cref="StageType"/> specifying the stage type of the associated sport event or a null reference if property is not applicable
        /// for the associated sport event
        /// </summary>
        /// <seealso cref="StageType"/>
        public StageType? StageType { get; }

        /// <summary>
        /// Gets the live odds property
        /// </summary>
        /// <value>The live odds property</value>
        public string LiveOdds { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionDto"/> class
        /// </summary>
        /// <param name="sportEvent">A <see cref="sportEvent"/> instance containing basic information about the sport event</param>
        internal CompetitionDto(sportEvent sportEvent)
            : base(sportEvent)
        {
            if (RestMapperHelper.TryGetBookingStatus(sportEvent.liveodds, out var bookingStatus))
            {
                BookingStatus = bookingStatus;
            }

            if (sportEvent.competitors != null && sportEvent.competitors.Any())
            {
                Competitors = new ReadOnlyCollection<TeamCompetitorDto>(sportEvent.competitors.Select(c => new TeamCompetitorDto(c)).ToList());
                HomeAwayCompetitors = RestMapperHelper.FillHomeAwayCompetitors(sportEvent.competitors);
            }

            Conditions = sportEvent.sport_event_conditions == null
                             ? null
                             : new SportEventConditionsDto(sportEvent.sport_event_conditions);

            Venue = sportEvent.sport_event_conditions?.venue == null
                        ? null
                        : new VenueDto(sportEvent.sport_event_conditions.venue);

            if (Venue == null && sportEvent.venue != null)
            {
                Venue = new VenueDto(sportEvent.venue);
            }

            if (RestMapperHelper.TryGetSportEventType(sportEvent.type, out var type))
            {
                Type = type;
            }

            if (!string.IsNullOrEmpty(sportEvent.liveodds))
            {
                LiveOdds = sportEvent.liveodds;
            }

            if (RestMapperHelper.TryGetStageType(sportEvent.stage_type, out var stageType))
            {
                StageType = stageType;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionDto"/> class
        /// </summary>
        /// <param name="matchSummary">A <see cref="matchSummaryEndpoint"/> instance containing basic information about the sport event</param>
        internal CompetitionDto(matchSummaryEndpoint matchSummary)
            : this(matchSummary.sport_event)
        {
            Guard.Argument(matchSummary, nameof(matchSummary)).NotNull();

            Conditions = matchSummary.sport_event_conditions == null
                             ? Conditions
                             : new SportEventConditionsDto(matchSummary.sport_event_conditions);

            SportEventStatus = matchSummary.sport_event_status == null
                                   ? null
                                   : new SportEventStatusDto(matchSummary.sport_event_status, matchSummary.statistics, HomeAwayCompetitors);

            Venue = matchSummary.sport_event_conditions?.venue == null
                        ? Venue
                        : new VenueDto(matchSummary.sport_event_conditions.venue);

            if (Venue == null && matchSummary.venue != null)
            {
                Venue = new VenueDto(matchSummary.venue);
            }

            GeneratedAt = matchSummary.generated_atSpecified
                              ? matchSummary.generated_at.ToLocalTime()
                              : (DateTime?)null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionDto"/> class
        /// </summary>
        /// <param name="stageSummary">A <see cref="stageSummaryEndpoint"/> instance containing basic information about the sport event</param>
        internal CompetitionDto(stageSummaryEndpoint stageSummary)
            : this(stageSummary.sport_event)
        {
            Guard.Argument(stageSummary, nameof(stageSummary)).NotNull();

            SportEventStatus = stageSummary.sport_event_status == null
                                   ? null
                                   : new SportEventStatusDto(stageSummary.sport_event_status);

            GeneratedAt = stageSummary.generated_atSpecified
                              ? stageSummary.generated_at.ToLocalTime()
                              : (DateTime?)null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionDto"/> class
        /// </summary>
        /// <param name="stageSummary">A <see cref="sportEventChildrenSport_event"/> instance containing basic information about the sport event</param>
        /// <remarks>LiveOdds = null</remarks>
        internal CompetitionDto(sportEventChildrenSport_event stageSummary)
            : base(stageSummary)
        {
            if (RestMapperHelper.TryGetSportEventType(stageSummary.type, out var type))
            {
                Type = type;
            }

            if (RestMapperHelper.TryGetStageType(stageSummary.stage_type, out var stageType))
            {
                StageType = stageType;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionDto"/> class
        /// </summary>
        /// <param name="parentStage">A <see cref="parentStage"/> instance containing basic information about the sport event</param>
        /// <remarks>LiveOdds = null</remarks>
        protected CompetitionDto(parentStage parentStage)
            : base(parentStage)
        {
            if (RestMapperHelper.TryGetSportEventType(parentStage.type, out var type))
            {
                Type = type;
            }

            if (RestMapperHelper.TryGetStageType(parentStage.stage_type, out var stageType))
            {
                StageType = stageType;
            }
        }
    }
}
