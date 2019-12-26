/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Enumerates market cash-out availability
    /// </summary>
    public enum CashoutStatus
    {
        /// <summary>
        /// Indicates cashout for associated market is available
        /// </summary>
        AVAILABLE = 1,

        /// <summary>
        /// Indicates cashout for associated market is un-available
        /// </summary>
        UNAVAILABLE = -1,

        /// <summary>
        /// Indicates cashout for associated market is no longer available - is closed
        /// </summary>
        CLOSED = -2
    }
}
