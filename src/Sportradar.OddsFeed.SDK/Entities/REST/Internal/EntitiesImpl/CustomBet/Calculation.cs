/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl.CustomBet
{
    /// <summary>
    /// Implements methods used to provides a probability calculation
    /// </summary>
    public class Calculation : ICalculation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Calculation"/> class
        /// </summary>
        /// <param name="calculation">a <see cref="CalculationDTO"/> representing the calculation</param>
        internal Calculation(CalculationDTO calculation)
        {
            if (calculation == null)
                throw new ArgumentNullException(nameof(calculation));

            Odds = calculation.Odds;
            Probability = calculation.Probability;
        }

        public double Odds { get; }
        public double Probability { get; }
    }
}
