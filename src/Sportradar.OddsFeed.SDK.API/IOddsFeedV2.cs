/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using Sportradar.OddsFeed.SDK.API.EventArguments;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Represent a root object of the unified odds feed
    /// </summary>
    public interface IOddsFeedV2 : IOddsFeedV1
    {
        /// <summary>
        /// Gets a <see cref="IMarketDescriptionManager"/> instance used to get info about available markets, and to get translations for markets and outcomes including outrights
        /// </summary>
        IMarketDescriptionManager MarketDescriptionManager { get; }

        /// <summary>
        /// Gets a <see cref="ICustomBetManager"/> instance used to perform various custom bet operations
        /// </summary>
        ICustomBetManager CustomBetManager { get; }

        /// <summary>
        /// Occurs when a requested event recovery completes
        /// </summary>
        event EventHandler<EventRecoveryCompletedEventArgs> EventRecoveryCompleted;
    }
}
