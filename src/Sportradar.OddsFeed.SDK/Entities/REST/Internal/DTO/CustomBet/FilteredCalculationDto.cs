// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet
{
    /// <summary>
    /// Defines a data-transfer-object for probability calculations
    /// </summary>
    internal class FilteredCalculationDto
    {
        /// <summary>
        /// Gets the odds
        /// </summary>
        public double Odds { get; }

        /// <summary>
        /// Gets the probability
        /// </summary>
        public double Probability { get; }

        /// <summary>
        /// Gets the <see cref="string"/> specifying when the associated message was generated (on the server side)
        /// </summary>
        public string GeneratedAt { get; }

        /// <summary>
        /// Get the value specifying if the calculation used harmonized method
        /// </summary>
        public bool? Harmonization { get; }

        /// <summary>
        /// Gets the available selections
        /// </summary>
        public IList<FilteredAvailableSelectionsDto> AvailableSelections { get; }

        internal FilteredCalculationDto(FilteredCalculationResponseType calculation)
        {
            if (calculation == null)
            {
                throw new ArgumentNullException(nameof(calculation));
            }

            Odds = calculation.calculation.odds;
            Probability = calculation.calculation.probability;
            GeneratedAt = calculation.generated_at;
            AvailableSelections = calculation.available_selections.IsNullOrEmpty()
                                      ? new List<FilteredAvailableSelectionsDto>()
                                      : calculation.available_selections.Select(s => new FilteredAvailableSelectionsDto(s)).ToList();
            Harmonization = !calculation.calculation.harmonizationSpecified ? (bool?)null : calculation.calculation.harmonization;
        }
    }
}
