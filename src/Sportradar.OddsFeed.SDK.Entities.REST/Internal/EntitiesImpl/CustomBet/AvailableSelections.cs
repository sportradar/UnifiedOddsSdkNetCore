/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl.CustomBet
{
    /// <summary>
    /// Implements methods used to access available selections for the event
    /// </summary>
    public class AvailableSelections : IAvailableSelections
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AvailableSelections"/> class
        /// </summary>
        /// <param name="availableSelections">a <see cref="AvailableSelectionsDTO"/> representing the available selections</param>
        internal AvailableSelections(AvailableSelectionsDTO availableSelections)
        {
            if (availableSelections == null)
                throw new ArgumentNullException(nameof(availableSelections));

            Event = availableSelections.Event;
            Markets = availableSelections.Markets.Select(m => new Market(m)).ToList().AsReadOnly();
        }

        public URN Event { get; }
        public IEnumerable<IMarket> Markets { get; }
    }
}
