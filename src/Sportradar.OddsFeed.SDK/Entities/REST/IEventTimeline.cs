/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing event timeline for specific <see cref="ISportEvent"/>
    /// </summary>
    public interface IEventTimeline
    {
        /// <summary>
        /// Gets the chronological list of events
        /// </summary>
        /// <value>The chronological list of events</value>
        IEnumerable<ITimelineEvent> TimelineEvents { get; }
    }
}
