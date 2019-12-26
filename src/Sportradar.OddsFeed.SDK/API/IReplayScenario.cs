/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines a contract for replay scenario instances
    /// </summary>
    public interface IReplayScenario
    {
        /// <summary>
        /// Gets the id of the scenario
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets the description of the scenario
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets an indication if the scenario can be run in parallel
        /// </summary>
        bool RunParallel { get; }

        /// <summary>
        /// Gets the associated event identifiers
        /// </summary>
        IEnumerable<URN> AssociatedEventIds { get; }
    }
}
