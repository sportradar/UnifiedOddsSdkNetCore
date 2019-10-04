/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST.MarketMapping
{
    /// <summary>
    /// Defines a contract implemented by classes representing mapping ids of markets and outcomes
    /// </summary>
    public interface IMarketMapping
    {
        /// <summary>
        /// Gets the type identifier
        /// </summary>
        int TypeId { get; }

        /// <summary>
        /// Gets the special odds value
        /// </summary>
        string Sov { get; }
    }
}
