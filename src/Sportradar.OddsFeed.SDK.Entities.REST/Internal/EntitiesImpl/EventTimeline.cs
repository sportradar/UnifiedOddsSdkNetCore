/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    public class EventTimeline : IEventTimeline
    {
        public IEnumerable<ITimelineEvent> TimelineEvents { get; }

        public EventTimeline(EventTimelineCI ci)
        {
            Contract.Requires(ci != null);

            TimelineEvents = ci.Timeline?.Select(s => new TimelineEvent(s));
        }
    }
}
