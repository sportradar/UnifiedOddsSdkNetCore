/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

// ReSharper disable InconsistentNaming
namespace Sportradar.OddsFeed.SDK.Entities.REST.Enums
{
    /// <summary>
    /// Specifies the void factor of the associated outcome. The value indicates
    /// the percentage of the stake that should be voided (returned to the punter).
    /// </summary>
    public enum VoidFactor
    {
        /// <summary>
        /// Indicates that the entire bet(stake) should be settled according to the result of the outcome
        /// </summary>
        Zero,

        /// <summary>
        /// Indicates that the half of bet(stake) should be settled according to the result of the outcome
        /// and the other have should be voided (returned to the punter)
        /// </summary>
        Half,

        /// <summary>
        /// Indicates that the entire bet(stake) should be voided (returned to the punter)
        /// </summary>
        One
    }
}
