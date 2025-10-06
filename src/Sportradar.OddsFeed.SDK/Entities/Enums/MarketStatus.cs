// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Entities.Enums
{
    /// <summary>
    /// Enumerates market statuses
    /// </summary>
    public enum MarketStatus
    {
        /// <summary>
        /// Indicating that the market should be displayed and bets on it should be accepted
        /// </summary>
        Active = 1,

        /// <summary>
        /// Indicates the market should NOT be displayed and bets on it should NOT be accepted
        /// </summary>
        Inactive = 0,

        /// <summary>
        /// Indicates that the market should be displayed but the bets on it should NOT be accepted
        /// </summary>
        Suspended = -1,

        /// <summary>
        /// Indicates the market was handed over from one odds producer to another.
        /// If the odds for this market were already received from another producer this update should be ignored,
        /// otherwise associated market should be suspended until such odds are received.
        /// </summary>
        HandedOver = -2,

        /// <summary>
        /// Indicates the markets has already been settled by a previous message
        /// </summary>
        Settled = -3,

        /// <summary>
        /// Indicates the markets has already been cancelled by a previous message
        /// </summary>
        Cancelled = -4
    }
}
