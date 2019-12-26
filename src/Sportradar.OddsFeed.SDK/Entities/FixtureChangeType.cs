/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

// ReSharper disable InconsistentNaming
namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Enumerates reasons why a <see cref="IFixtureChange{T}"/> message was received
    /// </summary>
    public enum FixtureChangeType
    {
        /// <summary>
        /// A new sport event has been added(typically used for events that are created and will start in the near-term)
        /// </summary>
        NEW = 1,

        /// <summary>
        /// The start time of the sport event has changed
        /// </summary>
        START_TIME = 2,

        /// <summary>
        /// The sport event has been canceled
        /// </summary>
        CANCELLED = 3,

        /// <summary>
        ///  Other various changes to the fixture
        /// </summary>
        OTHER = 4,

        /// <summary>
        /// Coverage of the sport event has been changed
        /// </summary>
        COVERAGE = 5
    }
}