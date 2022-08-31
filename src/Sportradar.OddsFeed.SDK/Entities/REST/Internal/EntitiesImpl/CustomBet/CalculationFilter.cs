/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl.CustomBet
{
    /// <summary>
    /// Implements methods used to provides a probability calculation
    /// </summary>
    internal class CalculationFilter : ICalculationFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ICalculationFilter"/> class
        /// </summary>
        /// <param name="calculation">a <see cref="FilteredCalculationDto"/> representing the calculation</param>
        internal CalculationFilter(FilteredCalculationDto calculation)
        {
            if (calculation == null)
            {
                throw new ArgumentNullException(nameof(calculation));
            }

            Odds = calculation.Odds;
            Probability = calculation.Probability;
            GeneratedAt = SdkInfo.ParseDate(calculation.GeneratedAt);
            AvailableSelections = calculation.AvailableSelections.Select(s => new AvailableSelectionsFilter(s));
        }

        public double Odds { get; }

        public double Probability { get; }

        public DateTime? GeneratedAt { get; }

        public IEnumerable<IAvailableSelectionsFilter> AvailableSelections { get; }
    }
}
