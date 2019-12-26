/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing the event clock
    /// </summary>
    public interface IEventClock : IEntityPrinter
    {
        /// <summary>
        /// Gets the event time of the sport event associated with the current <see cref="IEventClock"/> instance
        /// </summary>
        string EventTime { get; }

        /// <summary>
        /// Gets the representation of the time the event associated with the current
        /// <see cref="IEventClock"/> has been stopped
        /// </summary>
        string StoppageTime { get; }

        /// <summary>
        /// Gets the announced stoppage time
        /// </summary>
        string StoppageTimeAnnounced { get; }

        /// <summary>
        /// Gets the remaining date
        /// </summary>
        /// <value>The remaining date</value>
        string RemainingDate { get; }

        /// <summary>
        /// Gets the remaining time in period
        /// </summary>
        /// <value>The remaining time in period</value>
        string RemainingTimeInPeriod { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IEventClock"/> is stopped
        /// </summary>
        /// <value><c>true</c> if stopped; otherwise, <c>false</c></value>
        bool? Stopped { get; }
    }
}
