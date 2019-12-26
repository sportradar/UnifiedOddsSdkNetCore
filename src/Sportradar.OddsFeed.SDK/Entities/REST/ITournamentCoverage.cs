/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing tournament coverage information
    /// </summary>
    public interface ITournamentCoverage
    {
        /// <summary>
        /// Gets a value indicating whether live coverage is available
        /// </summary>
        /// <value><c>true</c> if [live coverage]; otherwise, <c>false</c>.</value>
        bool LiveCoverage { get; }
    }
}
