// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.InternalEntities;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Caching
{
    /// <summary>
    /// Defines a contract implemented by classes used to cache variant descriptions
    /// </summary>
    internal interface IVariantDescriptionsCache : ISdkCache, IHealthStatusProvider, IDisposable
    {
        /// <summary>
        /// Asynchronously gets a <see cref="IVariantDescription" /> instance for the variant market specified by <c>id</c>
        /// </summary>
        /// <param name="variantId">The variant identifier</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}" /> specifying required translations</param>
        /// <returns>A <see cref="Task{T}" /> representing the async retrieval operation</returns>
        /// <exception cref="CacheItemNotFoundException">The requested key was not found in the cache and could not be loaded</exception>
        Task<IVariantDescription> GetVariantDescriptorAsync(string variantId, IReadOnlyCollection<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously loads the variant list of market descriptions from the Sports API
        /// </summary>
        /// <returns>Returns true if the action succeeded</returns>
        Task<bool> LoadMarketDescriptionsAsync();
    }
}
