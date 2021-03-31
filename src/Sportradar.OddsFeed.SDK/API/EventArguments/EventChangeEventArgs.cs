/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.EventArguments
{
    /// <summary>
    /// Event arguments for <see cref="IEventChangeManager.FixtureChange"/> and <see cref="IEventChangeManager.ResultChange"/> event
    /// </summary>
    public class EventChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="URN"/> specifying the sport event
        /// </summary>
        public URN SportEventId { get; }

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
        public EventChangeEventArgs(URN sportEventId, DateTime updateTime, ISportEvent sportEvent)
        {
            SportEventId = sportEventId;
            UpdateTime = updateTime;
            SportEvent = sportEvent;
        }
    }
}
