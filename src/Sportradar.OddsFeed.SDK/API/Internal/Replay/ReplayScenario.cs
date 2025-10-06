// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Api.Replay;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Replay
{
    /// <summary>
    /// A basic implementation of the <see cref="IReplayScenario"/>
    /// </summary>
    internal class ReplayScenario : IReplayScenario
    {
        /// <summary>
        /// Gets the id of the scenario
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the description of the scenario
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets an indication if the scenario can be run in parallel
        /// </summary>
        public bool RunParallel { get; }

        /// <summary>
        /// Gets the associated event identifiers
        /// </summary>
        public IEnumerable<Urn> AssociatedEventIds { get; }

        public ReplayScenario(int id, string description, bool runParallel, IEnumerable<Urn> associatedEventIds)
        {
            Id = id;
            Description = description;
            RunParallel = runParallel;
            AssociatedEventIds = associatedEventIds;
        }
    }
}
