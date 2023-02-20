/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet
{
    /// <summary>
    /// Defines a data-transfer-object for available selections for the filtered outcome
    /// </summary>
    internal class FilteredOutcomeDto
    {
        /// <summary>
        /// Gets the id of the outcome
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Value indicating if this outcome is in conflict
        /// </summary>
        public bool? IsConflict { get; }

        internal FilteredOutcomeDto(FilteredOutcomeType outcomeType)
        {
            if (outcomeType == null)
            {
                throw new ArgumentNullException(nameof(outcomeType));
            }

            Id = outcomeType.id;
            IsConflict = outcomeType.conflictSpecified ? outcomeType.conflict : (bool?)null;
        }
    }
}
