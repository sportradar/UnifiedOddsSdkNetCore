/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Diagnostics.CodeAnalysis;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Enumerates market cashout availability
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum CashoutStatus
    {
        /// <summary>
        /// Indicates cashout for associated market is available
        /// </summary>
        AVAILABLE = 1,

        /// <summary>
        /// Indicates cashout for associated market is unavailable
        /// </summary>
        UNAVAILABLE = -1,

        /// <summary>
        /// Indicates cashout for associated market is no longer available - is closed
        /// </summary>
        CLOSED = -2
    }
}
