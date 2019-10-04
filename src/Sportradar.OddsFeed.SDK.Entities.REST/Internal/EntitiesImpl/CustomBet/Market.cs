/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl.CustomBet
{
    /// <summary>
    /// Implements methods used to access available selections for the market
    /// </summary>
    internal class Market : IMarket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Market"/> class
        /// </summary>
        /// <param name="market">a <see cref="MarketDTO"/> representing the market</param>
        internal Market(MarketDTO market)
        {
            if (market == null)
                throw new ArgumentNullException(nameof(market));

            Id = market.Id;
            Specifiers = market.Specifiers;
            Outcomes = market.Outcomes.ToList().AsReadOnly();
        }

        public int Id { get; }

        public string Specifiers { get; }

        public IEnumerable<string> Outcomes { get; }
    }
}
