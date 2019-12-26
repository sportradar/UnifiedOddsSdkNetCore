/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST.MarketMapping
{
    /// <summary>
    /// Implementation of the <see cref="IMarketMapping"/> for the LiveCycleOfOdds feed
    /// </summary>
    /// <seealso cref="IMarketMapping" />
    public class LcooMarketMapping : IMarketMapping
    {
        /// <summary>
        /// Gets the TypeId of the Lcoo market
        /// </summary>
        public int TypeId { get; }

        /// <summary>
        /// Gets the special odds value for the Lcoo market
        /// </summary>
        public string Sov { get; }

        internal LcooMarketMapping(int typeId, string sov)
        {
            TypeId = typeId;
            Sov = sov;
        }
    }
}
