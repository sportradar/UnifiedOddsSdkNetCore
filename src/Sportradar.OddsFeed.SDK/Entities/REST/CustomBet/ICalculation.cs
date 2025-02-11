// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet
{
    /// <summary>
    /// Provides a probability calculation
    /// </summary>
    public interface ICalculation
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
        IEnumerable<IAvailableSelections> AvailableSelections { get; }

        /// <summary>
        /// DateTime when API response was generated
        /// </summary>
        DateTime? GeneratedAt { get; }

        /// <summary>
        /// Get the value specifying if the calculation used harmonized method
        /// </summary>
        bool? Harmonization { get; }
    }
}
