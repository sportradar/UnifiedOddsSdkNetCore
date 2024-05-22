// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    internal class EventTimeline : IEventTimeline
    {
        public IEnumerable<ITimelineEvent> TimelineEvents { get; }

        public EventTimeline(EventTimelineCacheItem ci)
        {
            Guard.Argument(ci, nameof(ci)).NotNull();

            TimelineEvents = ci.Timeline?.Select(s => new TimelineEvent(s));
        }
    }
}
