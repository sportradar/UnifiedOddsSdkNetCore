/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST.CustomBet
{
    /// <summary>
    /// Provides a probability calculation filter
    /// </summary>
    public interface ICalculationFilter
    {
        /// <summary>
        /// Gets the odds
        /// </summary>
        double Odds { get; }

        /// <summary>
        /// Gets the probability
        /// </summary>
        double Probability { get; }

        /// <summary>
        /// The available selections
        /// </summary>
        IEnumerable<IAvailableSelectionsFilter> AvailableSelections { get; }

        /// <summary>
        /// DateTime when API response was generated
        /// </summary>
        DateTime? GeneratedAt { get; }
    }
}
