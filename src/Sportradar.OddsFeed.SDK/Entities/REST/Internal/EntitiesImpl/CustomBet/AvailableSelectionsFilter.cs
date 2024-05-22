// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl.CustomBet
{
    /// <summary>
    /// Implements methods used to access available selections for the event
    /// </summary>
    internal class AvailableSelectionsFilter : IAvailableSelectionsFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AvailableSelections"/> class
        /// </summary>
        /// <param name="availableSelections">a <see cref="AvailableSelectionsDto"/> representing the available selections</param>
        internal AvailableSelectionsFilter(FilteredAvailableSelectionsDto availableSelections)
        {
            if (availableSelections == null)
            {
                throw new ArgumentNullException(nameof(availableSelections));
            }

            Event = availableSelections.Event;
            Markets = availableSelections.Markets.Select(m => new MarketFilter(m));
        }

        public Urn Event { get; }

        public IEnumerable<IMarketFilter> Markets { get; }
    }
}
