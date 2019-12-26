/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes representing soccer sport events
    /// </summary>
    public interface ISoccerEvent : IMatch
    {
        /// <summary>
        /// Asynchronously get the status of the soccer match
        /// </summary>
        /// <returns>The status of the soccer match</returns>
        new Task<ISoccerStatus> GetStatusAsync();
    }
}
