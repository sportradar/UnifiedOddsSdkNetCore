/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object for match timeline
    /// </summary>
    internal class MatchTimelineDto
    {
        public SportEventSummaryDto SportEvent { get; }

        public CoverageInfoDto CoverageInfo { get; }

        public SportEventConditionsDto SportEventConditions { get; }

        public SportEventStatusDto SportEventStatus { get; }

        public DateTime? GeneratedAt { get; }

        public IEnumerable<BasicEventDto> BasicEvents { get; }

        internal MatchTimelineDto(matchTimelineEndpoint timeline)
        {
            Guard.Argument(timeline, nameof(timeline)).NotNull();
            Guard.Argument(timeline.sport_event, nameof(timeline.sport_event)).NotNull();

            SportEvent = RestMapperHelper.MapSportEvent(timeline.sport_event);

            if (timeline.coverage_info != null)
            {
                CoverageInfo = new CoverageInfoDto(timeline.coverage_info);
            }

            if (timeline.sport_event_conditions != null)
            {
                SportEventConditions = new SportEventConditionsDto(timeline.sport_event_conditions);
            }

            if (timeline.sport_event_status != null)
            {
                SportEventStatus = new SportEventStatusDto(timeline.sport_event_status, null, RestMapperHelper.FillHomeAwayCompetitors(timeline.sport_event.competitors));
            }

            if (timeline.timeline != null && timeline.timeline.Length > 0)
            {
                BasicEvents = timeline.timeline.Select(s => new BasicEventDto(s));
            }

            GeneratedAt = timeline.generated_atSpecified ? timeline.generated_at : (DateTime?)null;
        }
    }
}
