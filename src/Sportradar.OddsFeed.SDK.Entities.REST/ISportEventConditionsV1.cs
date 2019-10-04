/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing sport event conditions
    /// </summary>
    public interface ISportEventConditionsV1 : ISportEventConditions
    {
        /// <summary>
        /// Gets the pitchers
        /// </summary>
        IEnumerable<IPitcher> Pitchers { get; }
    }
}
