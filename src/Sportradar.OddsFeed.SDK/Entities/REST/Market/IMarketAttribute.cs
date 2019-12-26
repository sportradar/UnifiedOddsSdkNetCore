/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST.Market
{
    /// <summary>
    /// Represents market attribute used to provide additional information about a market
    /// </summary>
    public interface IMarketAttribute
    {
        /// <summary>
        /// Gets the attribute name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the attribute description
        /// </summary>
        string Description { get; }
    }
}