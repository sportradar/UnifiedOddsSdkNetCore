/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST.MarketMapping
{
    /// <summary>
    /// Implementation of the <see cref="IMarketMapping"/> for the LiveOdds feed
    /// </summary>
    /// <seealso cref="IMarketMapping" />
    public class LoMarketMapping : IMarketMapping
    {
        /// <summary>
        /// Gets the TypeId for the LO market
        /// </summary>
        public int TypeId { get; }

        /// <summary>
        /// Gets the sub type identifier for the LO market
        /// </summary>
        public int SubTypeId { get; }

        /// <summary>
        /// Gets the special odds value for the LO market
        /// </summary>
        public string Sov { get; }

        internal LoMarketMapping(int typeId, int subTypeId, string sov)
        {
            TypeId = typeId;
            SubTypeId = subTypeId;
            Sov = sov;
        }
    }
}
