/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Defines a contract implemented by messages which indicate a fixture was changed
    /// </summary>
    /// <typeparam name="T">A <see cref="ICompetition"/> derived type used to describe the sport event associated with the fixture change</typeparam>
    public interface IFixtureChange<out T> : IEventMessage<T> where T : ISportEvent
    {
        /// <summary>
        /// Gets a <see cref="FixtureChangeType"/> indicating how the fixture was changed (added, re-scheduled, ...)
        /// </summary>
        /// <remarks>If not specified in message, returns <see cref="FixtureChangeType.NA"/></remarks>
        FixtureChangeType ChangeType { get; }

        /// <summary>
        /// Gets a value specifying the start time of the fixture in milliseconds since EPOCH UTC after the fixture was re-scheduled
        /// </summary>
        long? NextLiveTime { get; }

        /// <summary>
        /// Gets a value specifying the start time of the fixture in milliseconds since EPOCH UTC
        /// </summary>
        long StartTime { get; }
    }
}
