/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet
{
    /// <summary>
    /// Defines a data-transfer-object for available selections for the event
    /// </summary>
    public class AvailableSelectionsDTO
    {
        /// <summary>
        /// Gets the <see cref="URN"/> of the event
        /// </summary>
        public URN Event { get; }

        /// <summary>
        /// Gets the list of markets for this event
        /// </summary>
        public IEnumerable<MarketDTO> Markets { get; }

        /// <summary>
        /// Gets the <see cref="string"/> specifying when the associated message was generated (on the server side)
        /// </summary>
        public string GeneratedAt { get; }

        internal AvailableSelectionsDTO(AvailableSelectionsType availableSelections)
        {
            if (availableSelections == null)
                throw new ArgumentNullException(nameof(availableSelections));

            Event = URN.Parse(availableSelections.@event.id);
            var markets = availableSelections.@event.markets;
            Markets = markets != null
                ? markets.Select(m => new MarketDTO(m)).ToList().AsReadOnly()
                : new ReadOnlyCollection<MarketDTO>(new List<MarketDTO>());
            GeneratedAt = availableSelections.generated_at;
        }
    }
}
