/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.EventArguments
{
    /// <summary>
    /// Event arguments for the EventRecoveryCompleted events
    /// </summary>
    public class EventRecoveryCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// The identifier of the recovery request
        /// </summary>
        private readonly long _requestId;

        /// <summary>
        /// The associated event identifier
        /// </summary>
        private readonly URN _eventId;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventRecoveryCompletedEventArgs"/> class
        /// </summary>
        /// <param name="requestId">The identifier of the recovery request</param>
        /// <param name="eventId">The associated event identifier</param>
        internal EventRecoveryCompletedEventArgs(long requestId, URN eventId)
        {
            if (eventId == null)
                throw new ArgumentNullException(nameof(eventId));

            _requestId = requestId;
            _eventId = eventId;
        }

        /// <summary>
        /// Gets the identifier of the recovery request
        /// </summary>
        /// <returns>Returns the identifier of the recovery request</returns>
        public long GetRequestId()
        {
            return _requestId;
        }

        /// <summary>
        /// Gets the associated event identifier
        /// </summary>
        /// <returns>Returns the associated event identifier</returns>
        public URN GetEventId()
        {
            return _eventId;
        }
    }
}
