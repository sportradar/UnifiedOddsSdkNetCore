/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/


namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Used to obtain information about available markets and get translations for markets and outcomes including outrights
    /// </summary>
    public interface IMarketDescriptionManagerV1 : IMarketDescriptionManager
    {
        /// <summary>
        /// Deletes the variant market description from cache
        /// </summary>
        /// <param name="marketId">The market identifier</param>
        /// <param name="variantValue">The variant value</param>
        void DeleteVariantMarketDescriptionFromCache(int marketId, string variantValue);
    }
}