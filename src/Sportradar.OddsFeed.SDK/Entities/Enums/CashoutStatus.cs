// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Entities.Enums
{
    /// <summary>
    /// Enumerates market cashout availability
    /// </summary>
    public enum CashoutStatus
    {
        /// <summary>
        /// Indicates cashout for associated market is available
        /// </summary>
        Available = 1,

        /// <summary>
        /// Indicates cashout for associated market is unavailable
        /// </summary>
        Unavailable = -1,

        /// <summary>
        /// Indicates cashout for associated market is no longer available - is closed
        /// </summary>
        Closed = -2
    }
}
