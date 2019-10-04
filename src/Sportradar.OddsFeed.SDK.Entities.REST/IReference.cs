/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// A contract for classes implementing array a+of references
    /// </summary>
    public interface IReference : IEntityPrinter
    {
        /// <summary>
        /// Gets the Betradar id for this instance if provided amount reference ids
        /// </summary>
        /// <returns>If exists among reference ids, result is greater then 0</returns>
        int BetradarId { get; }

        /// <summary>
        /// Gets the Betfair id for this instance if provided amount reference ids
        /// </summary>
        /// <returns>If exists among reference ids, result is greater then 0</returns>
        int BetfairId { get; }

        /// <summary>
        /// Gets the rotation number if provided among reference ids
        /// </summary>
        /// <value>If exists among reference ids, result is greater then 0</value>
        /// <remarks>This id only exists for US leagues</remarks>
        int RotationNumber { get; }

        /// <summary>
        /// Gets all the reference ids
        /// </summary>
        IReadOnlyDictionary<string, string> References { get; }
    }
}