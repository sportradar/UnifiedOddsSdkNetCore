/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles
{
    /// <summary>
    /// Defines a contract implemented by caches used to store information about player and competitor profiles
    /// </summary>
    internal interface IProfileCache : ISdkCache
    {
        /// <summary>
        /// Asynchronously gets a <see cref="PlayerProfileCI"/> representing the profile for the specified player
        /// </summary>
        /// <param name="playerId">A <see cref="URN"/> specifying the id of the player for which to get the profile</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages in which the information should be available</param>
        /// <returns>A <see cref="Task{PlayerProfileCI}"/> representing the asynchronous operation</returns>
        /// <exception cref="CacheItemNotFoundException">The requested item was not found in cache and could not be obtained from the API</exception>
        Task<PlayerProfileCI> GetPlayerProfileAsync(URN playerId, IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets <see cref="CompetitorCI"/> representing the profile for the specified competitor
        /// </summary>
        /// <param name="competitorId">A <see cref="URN"/> specifying the id of the competitor for which to get the profile</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages in which the information should be available</param>
        /// <returns>A <see cref="Task{PlayerProfileCI}"/> representing the asynchronous operation</returns>
        /// <exception cref="CacheItemNotFoundException">The requested item was not found in cache and could not be obtained from the API</exception>
        Task<CompetitorCI> GetCompetitorProfileAsync(URN competitorId, IEnumerable<CultureInfo> cultures);
    }
}
