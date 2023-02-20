/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl.CustomBet
{
    internal class OutcomeFilter : IOutcomeFilter
    {
        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        public bool? IsConflict { get; }

        internal OutcomeFilter(FilteredOutcomeDto outcomeDto)
        {
            if (outcomeDto == null)
            {
                throw new ArgumentNullException(nameof(outcomeDto));
            }

            Id = outcomeDto.Id;
            IsConflict = outcomeDto.IsConflict;
        }
    }
}
