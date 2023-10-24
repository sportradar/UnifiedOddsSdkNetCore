/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.Enums
{
    /// <summary>
    /// Enumerates reasons why a <see cref="IFixtureChange{T}"/> message was received
    /// </summary>
    public enum FixtureChangeType
    {
        /// <summary>
        /// A new sport event has been added(typically used for events that are created and will start in the near-term)
        /// </summary>
        New = 1,

        /// <summary>
        /// The start time of the sport event has changed
        /// </summary>
        /// <remarks>On message this value is represented with DATETIME value - keeping same name to avoid breaking change</remarks>
        StartTime = 2,

        /// <summary>
        /// The sport event has been canceled
        /// </summary>
        Cancelled = 3,

        /// <summary>
        ///  Format changes to the fixture
        /// </summary>
        Format = 4,

        /// <summary>
        /// Coverage of the sport event has been changed
        /// </summary>
        Coverage = 5,

        /// <summary>
        /// Pitcher has been changed
        /// </summary>
        Pitcher = 6,

        /// <summary>
        ///  Other various changes to the fixture
        /// </summary>
        Other = 101,

        /// <summary>
        /// Type not specified (not available - equal null)
        /// </summary>
        NotAvailable = 102
    }
}
