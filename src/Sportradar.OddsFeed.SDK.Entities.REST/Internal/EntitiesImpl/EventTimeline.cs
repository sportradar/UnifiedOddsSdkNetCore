/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    public class EventTimeline : IEventTimeline
    {
        public IEnumerable<ITimelineEvent> TimelineEvents { get; }

        public EventTimeline(EventTimelineCI ci)
        {
            Guard.Argument(ci).NotNull();

            TimelineEvents = ci.Timeline?.Select(s => new TimelineEvent(s));
        }
    }
}
