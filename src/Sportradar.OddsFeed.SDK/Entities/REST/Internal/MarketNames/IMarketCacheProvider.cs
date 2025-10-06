// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Market;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames
{
    /// <summary>
    /// Defines a contract implemented by classes used to market cache provider
    /// </summary>
    internal interface IMarketCacheProvider
    {
        /// <summary>
        /// Asynchronously gets a <see cref="IMarketDescription" /> instance for the market specified by <c>id</c> and <c>specifiers</c>
        /// </summary>
        /// <param name="marketId">The market identifier</param>
        /// <param name="specifiers">Dictionary of the specifiers</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}" /> specifying required translations</param>
        /// <param name="fetchVariantDescriptions">Should variant should be fetched</param>
        /// <returns>A <see cref="Task{T}" /> representing the async retrieval operation</returns>
        /// <exception cref="CacheItemNotFoundException">The requested key was not found in the cache and could not be loaded</exception>
        Task<IMarketDescription> GetMarketDescriptionAsync(int marketId,
                                                           IReadOnlyDictionary<string, string> specifiers,
                                                           IReadOnlyCollection<CultureInfo> cultures,
                                                           bool fetchVariantDescriptions);

        /// <summary>
        /// Reload data for market descriptions
        /// </summary>
        /// <param name="marketId">The market identifier</param>
        /// <param name="specifiers">A dictionary specifying market specifiers or a null reference if market is invariant</param>
        /// <returns>True if succeeded, false otherwise</returns>
        Task<bool> ReloadMarketDescriptionAsync(int marketId, IReadOnlyDictionary<string, string> specifiers);
    }
}
