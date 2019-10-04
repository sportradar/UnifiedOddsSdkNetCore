/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Represents information for a market with void reason
    /// </summary>
    public interface IMarketCancel : IMarket
    {
        /// <summary>
        /// Gets a <see cref="INamedValue"/> specifying the void reason, or a null reference if no void reason is specified
        /// </summary>
        INamedValue VoidReason { get; }
    }
}