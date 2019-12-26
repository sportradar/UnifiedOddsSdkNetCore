/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Event arguments describing the event which occurs when the tracker status is changed
    /// </summary>
    /// <seealso cref="EventArgs" />
    internal class TrackerStatusChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="long"/> specifying the id of the requestId of the recovery operation which caused this change,
        /// or a null reference if the change was not by a recovery request.
        /// </summary>
        public long? RequestId { get; }

        /// <summary>
        /// Gets the <see cref="ProducerRecoveryStatus"/> specifying the status of the tracker before the change
        /// </summary>
        public ProducerRecoveryStatus OldStatus { get; private set; }

        /// <summary>
        /// Gets the <see cref="ProducerRecoveryStatus"/> specifying the status of the tracker after the change
        /// </summary>
        public ProducerRecoveryStatus NewStatus { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerStatusChangeEventArgs"/> class.
        /// </summary>
        /// <param name="requestId">
        /// The <see cref="long"/> specifying the id of the requestId of the recovery operation which caused this change,
        /// or a null reference if the change was not by a recovery request.
        /// </param>
        /// <param name="oldStatus">The <see cref="ProducerRecoveryStatus"/> specifying the status of the tracker before the change.</param>
        /// <param name="newStatus">The <see cref="ProducerRecoveryStatus"/> specifying the status of the tracker after the change.</param>
        public TrackerStatusChangeEventArgs(long? requestId, ProducerRecoveryStatus oldStatus, ProducerRecoveryStatus newStatus)
        {
            RequestId = requestId;
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }
    }
}