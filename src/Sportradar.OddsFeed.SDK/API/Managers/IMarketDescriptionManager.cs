// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.Rest.Market;
using Sportradar.OddsFeed.SDK.Entities.Rest.MarketMapping;

namespace Sportradar.OddsFeed.SDK.Api.Managers
{
    /// <summary>
    /// Used to obtain information about available markets and get translations for markets and outcomes including outrights
    /// </summary>
    public interface IMarketDescriptionManager
    {
        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{IMarketDescription}"/> of all available static market descriptions
        /// </summary>
        /// <param name="culture">The language for which to get market descriptions</param>
        /// <returns>A <see cref="IEnumerable{IMarketDescription}"/> of available static market descriptions</returns>
        Task<IEnumerable<IMarketDescription>> GetMarketDescriptionsAsync(CultureInfo culture = null);

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{IMarketMappingData}"/> of available mappings for the provided marketId/producer combination
        /// </summary>
        /// <param name="marketId">The id of the market for which you need the mapping</param>
        /// <param name="producer">The <see cref="IProducer"/> for which you need the mapping</param>
        /// <returns>A <see cref="IEnumerable{IMarketMappingData}"/> of available mappings for the provided marketId/producer combination</returns>
        Task<IEnumerable<IMarketMappingData>> GetMarketMappingAsync(int marketId, IProducer producer);

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{IMarketMappingData}"/> of available mappings for the provided marketId/producer combination
        /// </summary>
        /// <param name="marketId">The id of the market for which you need the mapping</param>
        /// <param name="specifiers">The associated market specifiers</param>
        /// <param name="producer">The <see cref="IProducer"/> for which you need the mapping</param>
        /// <returns>A <see cref="IEnumerable{IMarketMappingData}"/> of available mappings for the provided marketId/producer combination</returns>
        Task<IEnumerable<IMarketMappingData>> GetMarketMappingAsync(int marketId, IReadOnlyDictionary<string, string> specifiers, IProducer producer);

        /// <summary>
        /// Asynchronously loads the invariant and variant list of market descriptions from the Sports API
        /// </summary>
        /// <remarks>To be used when manually changed market data via betradar control</remarks>
        /// <returns>Returns true if the action succeeded</returns>
        Task<bool> LoadMarketDescriptionsAsync();

        /// <summary>
        /// Deletes the variant market description from cache
        /// </summary>
        /// <param name="marketId">The market identifier</param>
        /// <param name="variantValue">The variant value</param>
        void DeleteVariantMarketDescriptionFromCache(int marketId, string variantValue);
    }
}
