/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet
{
    /// <summary>
    /// Defines a data-transfer-object for probability calculations
    /// </summary>
    internal class CalculationDto
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
        /// Gets the available selections
        /// </summary>
        public IList<AvailableSelectionsDto> AvailableSelections { get; }

        internal CalculationDto(CalculationResponseType calculation)
        {
            if (calculation == null)
            {
                throw new ArgumentNullException(nameof(calculation));
            }

            AvailableSelections = new List<AvailableSelectionsDto>();
            Odds = calculation.calculation.odds;
            Probability = calculation.calculation.probability;
            GeneratedAt = calculation.generated_at;

            if (calculation.available_selections != null && calculation.available_selections.Any())
            {
                AvailableSelections = calculation.available_selections.Select(s => new AvailableSelectionsDto(s, calculation.generated_at)).ToList();
            }
        }
    }
}
