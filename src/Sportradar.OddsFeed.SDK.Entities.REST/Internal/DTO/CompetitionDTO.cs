/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object containing basic information about a competition
    /// </summary>
    public class CompetitionDTO : SportEventSummaryDTO
    {
        /// <summary>
        /// Gets the sport event status
        /// </summary>
        /// <value>The sport event status</value>
        public SportEventStatusDTO SportEventStatus { get; }

        /// <summary>
        /// Gets a <see cref="BookingStatus"/> enum member specifying the booking status of the associated sport event
        /// </summary>
        /// <remarks>For more information see <see cref="BookingStatus"/> enumeration</remarks>
        public BookingStatus? BookingStatus { get; }

        /// <summary>
        /// Gets the venue info
        /// </summary>
        public VenueDTO Venue { get; internal set; }

        /// <summary>
        /// Gets the sport event conditions
        /// </summary>
        public SportEventConditionsDTO Conditions { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> representing the competitors of the associated sport event
        /// </summary>
        public IEnumerable<TeamCompetitorDTO> Competitors { get; }

        /// <summary>
        /// Gets the home away competitors
        /// </summary>
        /// <value>The home away competitors</value>
        internal IDictionary<HomeAway, URN> HomeAwayCompetitors { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying when the associated message was generated (on the server side)
        /// </summary>
        public DateTime? GeneratedAt { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionDTO"/> class
        /// </summary>
        /// <param name="sportEvent">A <see cref="sportEvent"/> instance containing basic information about the sport event</param>
        internal CompetitionDTO(sportEvent sportEvent)
            : base(sportEvent)
        {
            BookingStatus? bookingStatus;
            if (RestMapperHelper.TryGetBookingStatus(sportEvent.liveodds, out bookingStatus))
            {
                BookingStatus = bookingStatus;
            }

            if (sportEvent.competitors != null && sportEvent.competitors.Any())
            {
                Competitors = new ReadOnlyCollection<TeamCompetitorDTO>(sportEvent.competitors.Select(c => new TeamCompetitorDTO(c)).ToList());
                HomeAwayCompetitors = RestMapperHelper.FillHomeAwayCompetitors(sportEvent.competitors);
            }

            Conditions = sportEvent.sport_event_conditions == null
                             ? null
                             : new SportEventConditionsDTO(sportEvent.sport_event_conditions);

            Venue = sportEvent.sport_event_conditions?.venue == null
                        ? null
                        : new VenueDTO(sportEvent.sport_event_conditions.venue);

            if (Venue == null && sportEvent.venue != null)
            {
                Venue = new VenueDTO(sportEvent.venue);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionDTO"/> class
        /// </summary>
        /// <param name="matchSummary">A <see cref="matchSummaryEndpoint"/> instance containing basic information about the sport event</param>
        internal CompetitionDTO(matchSummaryEndpoint matchSummary)
            : this(matchSummary.sport_event)
        {
            Contract.Requires(matchSummary != null);

            Conditions = matchSummary.sport_event_conditions == null
                ? Conditions
                : new SportEventConditionsDTO(matchSummary.sport_event_conditions);

            SportEventStatus = matchSummary.sport_event_status == null
                ? null
                : new SportEventStatusDTO(matchSummary.sport_event_status, matchSummary.statistics, HomeAwayCompetitors);

            Venue = matchSummary.sport_event_conditions?.venue == null
                ? Venue
                : new VenueDTO(matchSummary.sport_event_conditions.venue);

            if (Venue == null && matchSummary.venue != null)
            {
                Venue = new VenueDTO(matchSummary.venue);
            }
            GeneratedAt = matchSummary.generated_atSpecified
                              ? matchSummary.generated_at.ToLocalTime()
                              : (DateTime?) null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionDTO"/> class
        /// </summary>
        /// <param name="stageSummary">A <see cref="stageSummaryEndpoint"/> instance containing basic information about the sport event</param>
        internal CompetitionDTO(stageSummaryEndpoint stageSummary)
            : this(stageSummary.sport_event)
        {
            Contract.Requires(stageSummary != null);

            SportEventStatus = stageSummary.sport_event_status == null
                ? null
                : new SportEventStatusDTO(stageSummary.sport_event_status);

            GeneratedAt = stageSummary.generated_atSpecified
                              ? stageSummary.generated_at.ToLocalTime()
                              : (DateTime?) null;
        }
    }
}
