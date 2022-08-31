/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl.CustomBet
{
    /// <summary>
    /// Implements methods used to access available selections for the market
    /// </summary>
    internal class MarketFilter : IMarketFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IMarketFilter"/> class
        /// </summary>
        /// <param name="market">a <see cref="FilteredMarketDto"/> representing the market</param>
        internal MarketFilter(FilteredMarketDto market)
        {
            if (market == null)
            {
                throw new ArgumentNullException(nameof(market));
            }

            Id = market.Id;
            Specifiers = market.Specifiers;
            Outcomes = market.Outcomes.Select(s => new OutcomeFilter(s));
            IsConflict = market.IsConflict;
        }

        public int Id { get; }

        public string Specifiers { get; }

        public IEnumerable<IOutcomeFilter> Outcomes { get; }

        public bool? IsConflict { get; }
    }
}
