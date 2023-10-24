/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Defines a contract for classes implementing additional market information
    /// </summary>
    public interface IMarketMetadata
    {
        /// <summary>
        /// Gets a epoch timestamp in UTC when to betstop the associated market. Typically used for outrights and typically is the start-time of the event the market refers to
        /// </summary>
        /// <value>The next betstop</value>
        long? NextBetstop { get; }

        /// <summary>
        /// Gets the start time of the event (as epoch timestamp)
        /// </summary>
        /// <value>The start time</value>
        long? StartTime { get; }

        /// <summary>
        /// Gets the end time of the event (as epoch timestamp)
        /// </summary>
        /// <value>The end time</value>
        long? EndTime { get; }

        /// <summary>
        /// Gets date/time when to betstop the associated market. Typically used for outrights and typically is the start-time of the event the market refers to
        /// </summary>
        /// <value>The next betstop</value>
        DateTime? NextBetstopDate { get; }

        /// <summary>
        /// Gets the start time of the event
        /// </summary>
        /// <value>The start time</value>
        DateTime? StartTimeDate { get; }

        /// <summary>
        /// Gets the end time of the event
        /// </summary>
        /// <value>The end time</value>
        DateTime? EndTimeDate { get; }

        /// <summary>
        /// Gets the Italian AAMS id for this outright
        /// </summary>
        /// <value>The Italian AAMS id for this outright</value>
        long? AamsId { get; }
    }
}
