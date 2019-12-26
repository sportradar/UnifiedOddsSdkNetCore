/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing a fixture
    /// </summary>
    /// <remarks>
    /// A Fixture is a sport event that has been arranged for a particular time and place
    /// </remarks>
    public interface IFixture : IEntityPrinter
    {
        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying when the fixture associated with the current <see cref="IFixture"/>
        /// is scheduled to start
        /// </summary>
        DateTime? StartTime { get; }

        /// <summary>
        /// Gets a value indicating whether the start time of the fixture represented by current
        /// <see cref="IFixture"/> instance has been confirmed
        /// </summary>
        bool? StartTimeConfirmed { get; }

        /// <summary>
        /// Gets a value indicating whether the start time is yet to be determent
        /// </summary>
        bool? StartTimeTBD { get; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying the live time in case the fixture represented by current
        /// <see cref="IFixture"/> instance was re-schedule, or a null reference if the fixture was not re-scheduled
        /// </summary>
        DateTime? NextLiveTime { get; }

        /// <summary>
        /// When sport event is postponed this field indicates with which event it is replaced
        /// </summary>
        /// <value>The <see cref="URN"/> this event is replaced by</value>
        URN ReplacedBy { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{String, String}"/> containing additional information about the
        /// fixture represented by current <see cref="IFixture"/> instance
        /// </summary>
        IReadOnlyDictionary<string, string> ExtraInfo { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{ITvChannel}"/> representing TV channels covering the sport event
        /// represented by the current <see cref="IFixture"/> instance
        /// </summary>
        IEnumerable<ITvChannel> TvChannels { get; }

        /// <summary>
        /// Gets a <see cref="ICoverageInfo"/> instance specifying what coverage is available for the sport event
        /// associated with current instance
        /// </summary>
        ICoverageInfo CoverageInfo { get; }

        /// <summary>
        /// Gets a <see cref="IProductInfo"/> instance providing Sportradar related information about the sport event associated
        /// with the current instance.
        /// </summary>
        IProductInfo ProductInfo { get; }

        /// <summary>
        /// Gets the reference ids
        /// </summary>
        IReference References { get; }

        /// <summary>
        /// Gets the scheduled start time changes
        /// </summary>
        /// <value>The scheduled start time changes</value>
        IEnumerable<IScheduledStartTimeChange> ScheduledStartTimeChanges { get; }
    }
}
