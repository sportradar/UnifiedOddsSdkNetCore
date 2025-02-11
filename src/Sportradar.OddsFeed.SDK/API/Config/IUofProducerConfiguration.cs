// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Api.Config
{
    /// <summary>
    /// Defines a contract implemented by classes representing api connection configuration / settings
    /// </summary>
    public interface IUofProducerConfiguration
    {
        /// <summary>
        /// Gets the maximum allowed timeout, between consecutive AMQP messages associated with the same producer.
        /// If this value is exceeded, the producer is considered to be down (seconds)
        /// </summary>
        TimeSpan InactivitySeconds { get; }

        /// <summary>
        /// Gets the maximum allowed timeout, between consecutive AMQP messages associated for the prematch producer.
        /// If this value is exceeded, the producer is considered to be down (seconds)
        /// </summary>
        TimeSpan InactivitySecondsPrematch { get; }

        /// <summary>
        /// Gets the maximum recovery time (seconds)
        /// </summary>
        /// <value>The maximum recovery time</value>
        TimeSpan MaxRecoveryTime { get; }

        /// <summary>
        /// Gets the minimal interval between recovery requests initiated by alive messages (seconds)
        /// </summary>
        TimeSpan MinIntervalBetweenRecoveryRequests { get; }

        /// <summary>
        /// Gets the comma delimited list of ids of disabled producers (default: none)
        /// </summary>
        /// <remarks></remarks>
        /// <value>The list of ids of disabled producers</value>
        List<int> DisabledProducers { get; }

        /// <summary>
        /// The collection of available producers (for provided access token)
        /// </summary>
        IReadOnlyCollection<IProducer> Producers { get; }
    }
}
