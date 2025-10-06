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
    internal class AvailableSelections : IAvailableSelections
    {
        public Urn EventId { get; }

        public IEnumerable<Rest.CustomBet.IMarket> Markets { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AvailableSelections"/> class
        /// </summary>
        /// <param name="availableSelections">a <see cref="AvailableSelectionsDto"/> representing the available selections</param>
        internal AvailableSelections(AvailableSelectionsDto availableSelections)
        {
            if (availableSelections == null)
            {
                throw new ArgumentNullException(nameof(availableSelections));
            }

            EventId = availableSelections.EventId;
            Markets = availableSelections.Markets.Select(m => new Market(m));
        }
    }
}
