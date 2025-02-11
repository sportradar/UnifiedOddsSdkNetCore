﻿// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet
{
    /// <summary>
    /// Defines a data-transfer-object for available selections for the event
    /// </summary>
    internal class AvailableSelectionsDto
    {
        /// <summary>
        /// Gets the <see cref="Urn"/> of the event
        /// </summary>
        public Urn EventId { get; }

        /// <summary>
        /// Gets the list of markets for this event
        /// </summary>
        public IList<MarketDto> Markets { get; }

        /// <summary>
        /// Gets the <see cref="string"/> specifying when the associated message was generated (on the server side)
        /// </summary>
        public string GeneratedAt { get; }

        internal AvailableSelectionsDto(AvailableSelectionsType availableSelections)
        {
            if (availableSelections == null)
            {
                throw new ArgumentNullException(nameof(availableSelections));
            }

            EventId = Urn.Parse(availableSelections.@event.id);
            var markets = availableSelections.@event.markets;
            Markets = markets != null
                ? markets.Select(m => new MarketDto(m)).ToList()
                : new List<MarketDto>();
            GeneratedAt = availableSelections.generated_at;
        }

        internal AvailableSelectionsDto(EventType eventType, string generatedAt)
        {
            if (eventType == null)
            {
                throw new ArgumentNullException(nameof(eventType));
            }

            EventId = Urn.Parse(eventType.id);
            Markets = eventType.markets != null
                          ? eventType.markets.Select(m => new MarketDto(m)).ToList()
                          : new List<MarketDto>();
            GeneratedAt = generatedAt;
        }
    }
}
