/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object for match timeline
    /// </summary>
    internal class MatchTimelineDTO
    {
        public SportEventSummaryDTO SportEvent { get; }

        public CoverageInfoDTO CoverageInfo { get; }

        public SportEventConditionsDTO SportEventConditions { get; }

        public SportEventStatusDTO SportEventStatus { get; }

        public DateTime? GeneratedAt { get; }

        public IEnumerable<BasicEventDTO> BasicEvents { get; }

        internal MatchTimelineDTO(matchTimelineEndpoint timeline)
        {
            Guard.Argument(timeline, nameof(timeline)).NotNull();
            Guard.Argument(timeline.sport_event, nameof(timeline.sport_event)).NotNull();

            SportEvent = RestMapperHelper.MapSportEvent(timeline.sport_event);

            if (timeline.coverage_info != null)
            {
                CoverageInfo = new CoverageInfoDTO(timeline.coverage_info);
            }

            if (timeline.sport_event_conditions != null)
            {
                SportEventConditions = new SportEventConditionsDTO(timeline.sport_event_conditions);
            }

            if (timeline.sport_event_status != null)
            {
                SportEventStatus = new SportEventStatusDTO(timeline.sport_event_status, null, RestMapperHelper.FillHomeAwayCompetitors(timeline.sport_event.competitors));
            }

            if (timeline.timeline != null && timeline.timeline.Length > 0)
            {
                BasicEvents = timeline.timeline.Select(s => new BasicEventDTO(s));
            }

            GeneratedAt = timeline.generated_atSpecified ? timeline.generated_at : (DateTime?)null;
        }
    }
}
