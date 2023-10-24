/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.Api.EventArguments
{
    /// <summary>
    /// Event arguments for <see cref="IEventChangeManager.FixtureChange"/> and <see cref="IEventChangeManager.ResultChange"/> event
    /// </summary>
    public class EventChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="Urn"/> specifying the sport event
        /// </summary>
        public Urn SportEventId { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying the last update time
        /// </summary>
        public DateTime UpdateTime { get; }

        /// <summary>
        /// Gets the <see cref="ISportEvent"/>
        /// </summary>
        public ISportEvent SportEvent { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventChangeEventArgs"/> class
        /// </summary>
        public EventChangeEventArgs(Urn sportEventId, DateTime updateTime, ISportEvent sportEvent)
        {
            SportEventId = sportEventId;
            UpdateTime = updateTime;
            SportEvent = sportEvent;
        }
    }
}
