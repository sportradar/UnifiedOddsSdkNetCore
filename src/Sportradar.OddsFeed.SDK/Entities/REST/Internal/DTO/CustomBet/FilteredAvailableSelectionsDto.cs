/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet
{
    /// <summary>
    /// Defines a data-transfer-object for available selections for the event
    /// </summary>
    internal class FilteredAvailableSelectionsDto
    {
        /// <summary>
        /// Gets the <see cref="URN"/> of the event
        /// </summary>
        public URN Event { get; }

        /// <summary>
        /// Gets the list of markets for this event
        /// </summary>
        public IList<FilteredMarketDto> Markets { get; }

        /// <summary>
        /// Gets the <see cref="string"/> specifying when the associated message was generated (on the server side)
        /// </summary>
        public string GeneratedAt { get; }

        internal FilteredAvailableSelectionsDto(FilteredEventType eventType)
        {
            if (eventType == null)
            {
                throw new ArgumentNullException(nameof(eventType));
            }

            //TODO replacements should be removed once api bug fix
            Event = URN.Parse(eventType.id.Replace(":live", string.Empty).Replace(":prematch", string.Empty));
            Markets = eventType.markets != null
                          ? eventType.markets.Select(m => new FilteredMarketDto(m)).ToList().AsReadOnly()
                          : new ReadOnlyCollection<FilteredMarketDto>(new List<FilteredMarketDto>());
            GeneratedAt = string.Empty;
        }
    }
}
