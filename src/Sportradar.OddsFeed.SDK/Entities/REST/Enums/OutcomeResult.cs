// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Enums
{
    /// <summary>
    /// An indication of the outcome result
    /// </summary>
    public enum OutcomeResult
    {
        /// <summary>
        /// Lost
        /// </summary>
        Lost,

        /// <summary>
        /// Won
        /// </summary>
        Won,

        /// <summary>
        /// Undecided yet
        /// </summary>
        UndecidedYet,

        /// <summary>
        /// Outcome result is not supported by the SDK
        /// </summary>
        UnsupportedBySdk
    }
}
