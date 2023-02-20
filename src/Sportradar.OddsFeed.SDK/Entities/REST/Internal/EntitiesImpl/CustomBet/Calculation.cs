/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl.CustomBet
{
    /// <summary>
    /// Implements methods used to provides a probability calculation
    /// </summary>
    internal class Calculation : ICalculation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Calculation"/> class
        /// </summary>
        /// <param name="calculation">a <see cref="CalculationDto"/> representing the calculation</param>
        internal Calculation(CalculationDto calculation)
        {
            if (calculation == null)
            {
                throw new ArgumentNullException(nameof(calculation));
            }

            Odds = calculation.Odds;
            Probability = calculation.Probability;
            GeneratedAt = SdkInfo.ParseDate(calculation.GeneratedAt);
            AvailableSelections = calculation.AvailableSelections.Select(s => new AvailableSelections(s));
        }

        public double Odds { get; }

        public double Probability { get; }

        public DateTime? GeneratedAt { get; }

        public IEnumerable<IAvailableSelections> AvailableSelections { get; }
    }
}
