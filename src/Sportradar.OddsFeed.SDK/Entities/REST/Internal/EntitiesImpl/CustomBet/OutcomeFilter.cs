using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet;
using System;

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
