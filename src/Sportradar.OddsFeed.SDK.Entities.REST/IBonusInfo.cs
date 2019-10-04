/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing the bonus info
    /// </summary>
    public interface IBonusInfo
    {
        /// <summary>
        /// Gets the bonus balls info
        /// </summary>
        /// <value>The bonus balls info or null if not known</value>
        int? BonusBalls { get; }

        /// <summary>
        /// Gets the type of the bonus drum
        /// </summary>
        /// <value>The type of the bonus drum or null if not known</value>
        BonusDrumType? BonusDrumType { get; }

        /// <summary>
        /// Gets the bonus range
        /// </summary>
        /// <value>The bonus range</value>
        string BonusRange { get; }
    }
}
