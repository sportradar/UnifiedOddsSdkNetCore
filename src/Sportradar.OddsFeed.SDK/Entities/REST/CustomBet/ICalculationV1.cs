// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet
{
    /// <summary>
    /// Provides a probability calculation
    /// </summary>
    public interface ICalculationV1 : ICalculation
    {
        /// <summary>
        /// Get the value specifying if the calculation used harmonized method
        /// </summary>
        bool? Harmonization { get; }
    }
}
