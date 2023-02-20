/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet
{
    /// <summary>
    /// Defines a data-transfer-object for available selections for the market
    /// </summary>
    internal class FilteredMarketDto
    {
        /// <summary>
        /// Gets the id of the market
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the specifiers for this market
        /// </summary>
        public string Specifiers { get; }

        /// <summary>
        /// Gets the outcomes for this market
        /// </summary>
        public IEnumerable<FilteredOutcomeDto> Outcomes { get; }

        /// <summary>
        /// Value indicating if this market is in conflict
        /// </summary>
        public bool? IsConflict { get; }

        internal FilteredMarketDto(FilteredMarketType market)
        {
            if (market == null)
            {
                throw new ArgumentNullException(nameof(market));
            }

            Id = market.id;
            Specifiers = market.specifiers;
            IsConflict = market.conflictSpecified ? market.conflict : (bool?)null;
            Outcomes = market.outcome.IsNullOrEmpty()
                           ? new List<FilteredOutcomeDto>()
                           : market.outcome.Select(o => new FilteredOutcomeDto(o)).ToList();
        }
    }
}
