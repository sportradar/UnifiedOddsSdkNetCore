/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes representing soccer status
    /// </summary>
    /// <seealso cref="IMatchStatus" />
    public interface ISoccerStatus : IMatchStatus
    {
        /// <summary>
        /// Gets the soccer match statistics
        /// </summary>
        /// <value>The soccer match statistics</value>
        ISoccerStatistics Statistics { get; }
    }
}
