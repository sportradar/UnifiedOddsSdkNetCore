// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl.CustomBet
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
