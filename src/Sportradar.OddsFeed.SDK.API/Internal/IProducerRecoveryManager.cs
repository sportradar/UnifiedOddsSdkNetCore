/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Represents contract for tracking <see cref="FeedMessage"/> per specific <see cref="IProducer"/>
    /// </summary>
    internal interface IProducerRecoveryManager
    {
        /// <summary>
        /// A <see cref="IProducer"/> for which <see cref="FeedMessage"/> are tracked
        /// </summary>
        IProducer Producer { get; }

        /// <summary>
        /// Gets the status of the recovery manager
        /// </summary>
        /// <value>The status.</value>
        ProducerRecoveryStatus Status { get; }

        /// <summary>
        /// Occurs when status of the associated manager has changed
        /// </summary>
        event EventHandler<TrackerStatusChangeEventArgs> StatusChanged;

        /// <summary>
        /// Occurs when a requested event recovery completes
        /// </summary>
        event EventHandler<EventRecoveryCompletedEventArgs> EventRecoveryCompleted;

        /// <summary>
        /// Checks the status of the current recovery manager
        /// </summary>
        /// <remarks>
        /// The method must:
        /// - Check whether current recovery is running and has expired
        /// - Whether there is an alive violation
        /// - Whether the user is behind with processing
        /// The method should not:
        /// - Update the processing delay - this is done on a message from a user's session
        /// - Start the recovery - this is done on alive from the system session
        /// - Complete non timed-out recovery - this is done on the snapshot_complete from user's session
        /// </remarks>
        void CheckStatus();

        /// <summary>
        /// Processes <see cref="IMessage" /> received on the user's session(s)
        /// </summary>
        /// <remarks>
        /// This method does:
        /// - collect timing info on odds_change(s), bet_stop(s) and alive(s)
        /// - attempt to complete running recoveries with snapshot_complete(s)
        /// This method does not:
        /// - determine if the user is behind (or not) with message processing - this is done in CheckStatus(..) method
        /// - attempt to determine whether the recovery has timed-out - this is done in CheckStatus(..) method
        /// - determine weather is alive violated. This should be only done based on system alive(s) and is done in ProcessSystemMessage(...) method
        /// - start recoveries - This should only be done based on system alive(s) and is done in ProcessSystemMessage(...) method
        /// </remarks>
        /// <param name="message">The <see cref="IMessage" /> message to be processed</param>
        /// <param name="interest">The <see cref="MessageInterest"/> describing the session from which the message originates </param>
        /// <exception cref="System.ArgumentException">The Producer.Id of the message and the Producer associated with this manager do not match</exception>
        void ProcessUserMessage(FeedMessage message, MessageInterest interest);

        /// <summary>
        /// Processes a message received on the system's session
        /// </summary>
        /// <remarks>
        /// This method does:
        /// - starts recovery operations if needed
        /// - interrupt running recoveries on non-subscribed alive(s) and alive violation(s)
        /// - set LastTimestampBeforeDisconnect property on the producer.
        /// This method does not:
        /// - determine if the user is behind (or not) with message processing - this is done in CheckStatus(..) method
        /// - attempt to determine whether the recovery has timed-out - this is done in CheckStatus(..) method
        /// </remarks>
        /// <param name="message">A <see cref="FeedMessage"/> received on the system session</param>
        void ProcessSystemMessage(FeedMessage message);

        /// <summary>
        /// Executes the steps required when the connection to the message broker is shutdown.
        /// </summary>
        void ConnectionShutdown();
    }
}
