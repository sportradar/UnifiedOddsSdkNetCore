/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Implementation of <see cref="IMarketMetadata"/>
    /// </summary>
    /// <seealso cref="IMarketMetadata" />
    public class MarketMetadata : IMarketMetadata
    {
        /// <summary>
        /// Gets a timestamp in UTC when to betstop the associated market. Typically used for outrights and typically is the start-time of the event the market refers to
        /// </summary>
        /// <value>The next betstop.</value>
        public long? NextBetstop { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketMetadata"/> class.
        /// </summary>
        /// <param name="nextBetstop">The next betstop.</param>
        public MarketMetadata(long? nextBetstop)
        {
            NextBetstop = nextBetstop;
        }
    }
}
