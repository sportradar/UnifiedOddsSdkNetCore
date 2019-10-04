/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.Messages
{
    /// <summary>
    /// Defines a contract for producer which use the feed to dispatch messages
    /// </summary>
    public interface IProducer
    {
        /// <summary>
        /// Gets the id of the producer
        /// </summary>
        /// <value>The id</value>
        int Id { get; }

        /// <summary>
        /// Gets the name of the producer
        /// </summary>
        /// <value>The name</value>
        string Name { get; }

        /// <summary>
        /// Gets the description of the producer
        /// </summary>
        /// <value>The description</value>
        string Description { get; }

        /// <summary>
        /// Gets a value indicating whether the producer is available on feed
        /// </summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c></value>
        bool IsAvailable { get; }

        /// <summary>
        /// Gets a value indicating whether the producer is disabled
        /// </summary>
        /// <value><c>true</c> if this instance is disabled; otherwise, <c>false</c></value>
        bool IsDisabled { get; }

        /// <summary>
        /// Gets a value indicating whether the producer is marked as down
        /// </summary>
        /// <value><c>true</c> if this instance is down; otherwise, <c>false</c></value>
        bool IsProducerDown { get; }

        /// <summary>
        /// Gets the time of last alive message received from feed
        /// </summary>
        /// <value>The time of last alive message</value>
        [Obsolete]
        DateTime TimeOfLastAlive { get; }

        /// <summary>
        /// Gets the last timestamp before disconnect for this producer
        /// </summary>
        /// <value>The last timestamp before disconnect</value>
        DateTime LastTimestampBeforeDisconnect { get; }

        /// <summary>
        /// Gets the maximum recovery time
        /// </summary>
        /// <value>The maximum recovery time</value>
        int MaxRecoveryTime { get; }

        /// <summary>
        /// Gets the maximum inactivity seconds
        /// </summary>
        /// <value>The maximum inactivity seconds</value>
        int MaxInactivitySeconds { get; }
    }
}