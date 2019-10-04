/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet
{
    /// <summary>
    /// Defines a data-transfer-object for probability calculations
    /// </summary>
    public class CalculationDTO
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

        internal CalculationDTO(CalculationResponseType calculation)
        {
            if (calculation == null)
                throw new ArgumentNullException(nameof(calculation));

            Odds = calculation.calculation.odds;
            Probability = calculation.calculation.probability;
            GeneratedAt = calculation.generated_at;
        }
    }
}
