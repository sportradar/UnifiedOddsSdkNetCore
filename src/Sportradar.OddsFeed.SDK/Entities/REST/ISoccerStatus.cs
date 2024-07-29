// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;

namespace Sportradar.OddsFeed.SDK.Entities.Rest
{
    /// <summary>
    /// Defines a contract for classes representing soccer status
    /// </summary>
    /// <seealso cref="IMatchStatus" />
    [Obsolete("IMatchStatusV1 is recommended instead of ISoccerStatus")]
    public interface ISoccerStatus : IMatchStatus
    {
        /// <summary>
        /// Gets the soccer match statistics
        /// </summary>
        /// <value>The soccer match statistics</value>
        ISoccerStatistics Statistics { get; }
    }
}
