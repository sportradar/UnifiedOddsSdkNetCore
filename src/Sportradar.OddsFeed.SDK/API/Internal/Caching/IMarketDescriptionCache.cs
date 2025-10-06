// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Market;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Caching
{
    /// <summary>
    /// Defines a contract implemented by classes used to cache market descriptions, for single variant market description)
    /// </summary>
    internal interface IMarketDescriptionCache : ISdkCache, IHealthStatusProvider, IDisposable
    {
        /// <summary>
        /// Asynchronously gets a <see cref="IMarketDescription" /> instance for the market specified by <c>id</c> and <c>specifiers</c>
        /// </summary>
        /// <param name="marketId">The market identifier</param>
        /// <param name="variant">A <see cref="string" /> specifying market variant or a null reference if market is invariant</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}" /> specifying required translations</param>
        /// <returns>A <see cref="Task{T}" /> representing the async retrieval operation</returns>
        /// <exception cref="CacheItemNotFoundException">The requested key was not found in the cache and could not be loaded</exception>
        Task<IMarketDescription> GetMarketDescriptionAsync(int marketId, string variant, IReadOnlyCollection<CultureInfo> cultures);

        /// <summary>
        /// Updates cache item fetch time
        /// </summary>
        void UpdateCacheItem(int marketId, string variantValue);
    }
}
