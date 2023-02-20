/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.EventArguments
{
    /// <summary>
    /// Event arguments for the RecoveryInitiated events
    /// </summary>
    public class RecoveryInitiatedEventArgs : EventArgs
    {
        /// <summary>
        /// The identifier of the recovery request
        /// </summary>
        private readonly long _requestId;

        /// <summary>
        /// The after timestamp if applied
        /// </summary>
        private readonly long _after;

        /// <summary>
        /// The associated producer identifier
        /// </summary>
        private readonly int _producerId;

        /// <summary>
        /// The associated event identifier
        /// </summary>
        private readonly URN _eventId;

        /// <summary>
        /// The message
        /// </summary>
        private readonly string _message;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventRecoveryCompletedEventArgs"/> class
        /// </summary>
        /// <param name="requestId">The identifier of the recovery request</param>
        /// <param name="producerId">The associated producer identifier</param>
        /// <param name="after">The after attribute if applies</param>
        /// <param name="eventId">The associated event identifier</param>
        /// <param name="message">The message</param>
        internal RecoveryInitiatedEventArgs(long requestId, int producerId, long after, URN eventId, string message)
        {
            _requestId = requestId;
            _producerId = producerId;
            _after = after;
            _eventId = eventId;
            _message = message;
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
        /// Gets the associated producer identifier
        /// </summary>
        /// <returns>Returns the associated producer identifier</returns>
        public int GetProducerId()
        {
            return _producerId;
        }

        /// <summary>
        /// Gets the after timestamp if applied
        /// </summary>
        /// <returns>Returns the after timestamp if applied</returns>
        public long GetAfterTimestamp()
        {
            return _after;
        }

        /// <summary>
        /// Gets the associated event identifier
        /// </summary>
        /// <returns>Returns the associated event identifier</returns>
        public URN GetEventId()
        {
            return _eventId;
        }

        /// <summary>
        /// Gets the message associated with the recovery request
        /// </summary>
        /// <returns>Returns the message associated with the recovery request</returns>
        public string GetMessage()
        {
            return _message;
        }
    }
}
