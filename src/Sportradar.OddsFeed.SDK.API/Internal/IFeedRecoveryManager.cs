/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal.Metrics;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Contract for interacting with <see cref="FeedRecoveryManager" /></summary>
    /// <seealso cref="IOpenable" />
    /// <seealso cref="IHealthStatusProvider" />
    internal interface IFeedRecoveryManager
    {
        /// <summary>
        /// Creates new <see cref="ISessionMessageManager"/>
        /// </summary>
        /// <returns>Newly created <see cref="ISessionMessageManager"/></returns>
        ISessionMessageManager CreateSessionMessageManager();

        /// <summary>
        /// Occurs when the specific <see cref="IProducer"/> is marked as up indicating the state of the SDK is synchronized
        /// with the state of the feed
        /// </summary>
        event EventHandler<ProducerStatusChangeEventArgs> ProducerUp;

        /// <summary>
        /// Occurs when the specific <see cref="IProducer"/> is marked as down indicating the state of the SDK is NOT synchronized
        /// with the state of the feed, or the associated producer is experiencing problems
        /// </summary>
        event EventHandler<ProducerStatusChangeEventArgs> ProducerDown;

        /// <summary>
        /// Occurs when the specific <see cref="IProducerRecoveryManager"/> is in status <see cref="ProducerRecoveryStatus.FatalError"/> indicating the recovery request could not be invoked
        /// </summary>
        event EventHandler<FeedCloseEventArgs> CloseFeed;

        /// <summary>
        /// Occurs when a requested event recovery completes
        /// </summary>
        event EventHandler<EventRecoveryCompletedEventArgs> EventRecoveryCompleted;

        /// <summary>
        /// Gets a value indicating whether the current instance is opened
        /// </summary>
        bool IsOpened { get; }

        /// <summary>
        /// Opens the current instance
        /// </summary>
        /// <param name="interests">The interests for which to open trackers</param>
        void Open(IEnumerable<MessageInterest> interests);

        /// <summary>
        /// Closes the current instance
        /// </summary>
        void Close();

        /// <summary>
        /// Executes the steps required when the connection to the message broker is shutdown.
        /// </summary>
        void ConnectionShutdown();
    }
}
