// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet
{
    /// <summary>
    /// Provides a probability calculation filter
    /// </summary>
    public interface ICalculationFilterV1 : ICalculationFilter
    {
        /// <summary>
        /// Get the value specifying if the calculation used harmonized method
        /// </summary>
        bool? Harmonization { get; }
    }
}
