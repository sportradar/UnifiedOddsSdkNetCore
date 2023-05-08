/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal.Metrics;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles
{
    /// <summary>
    /// Defines a contract implemented by caches used to store information about player and competitor profiles
    /// </summary>
    internal interface IProfileCache : ISdkCache, IHealthStatusProvider, IDisposable, IExportableSdkCache
    {
        /// <summary>
        /// Asynchronously gets a <see cref="PlayerProfileCI"/> representing the profile for the specified player
        /// </summary>
        /// <param name="playerId">A <see cref="URN"/> specifying the id of the player for which to get the profile</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying languages in which the information should be available</param>
        /// <param name="fetchIfMissing">Indicated if player profile should be fetched when <see cref="PlayerProfileCI"/> is missing name for specified cultures</param>
        /// <returns>A <see cref="Task{PlayerProfileCI}"/> representing the asynchronous operation</returns>
        /// <exception cref="CacheItemNotFoundException">The requested item was not found in cache and could not be obtained from the API</exception>
        Task<PlayerProfileCI> GetPlayerProfileAsync(URN playerId, IReadOnlyCollection<CultureInfo> cultures, bool fetchIfMissing);

        /// <summary>
        /// Asynchronously gets <see cref="CompetitorCI"/> representing the profile for the specified competitor
        /// </summary>
        /// <param name="competitorId">A <see cref="URN"/> specifying the id of the competitor for which to get the profile</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying languages in which the information should be available</param>
        /// <param name="fetchIfMissing">Indicated if competitor profile should be fetched when <see cref="CompetitorCI"/> is missing name for specified cultures</param>
        /// <returns>A <see cref="Task{CompetitorCI}"/> representing the asynchronous operation</returns>
        /// <exception cref="CacheItemNotFoundException">The requested item was not found in cache and could not be obtained from the API</exception>
        Task<CompetitorCI> GetCompetitorProfileAsync(URN competitorId, IReadOnlyCollection<CultureInfo> cultures, bool fetchIfMissing);

        /// <summary>
        /// Asynchronously gets a list of player name values
        /// </summary>
        /// <param name="playerId">A <see cref="URN"/> specifying the id of the player for which to get the profile</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying languages in which the information should be available</param>
        /// <param name="fetchIfMissing">Indicated if player profile should be fetched when <see cref="PlayerProfileCI"/> is missing name for specified cultures</param>
        /// <returns>A dictionary of name values for all cultures (if missing returns empty string)</returns>
        /// <exception cref="CacheItemNotFoundException">The requested item was not found in cache and could not be obtained from the API</exception>
        Task<IReadOnlyDictionary<CultureInfo, string>> GetPlayerNamesAsync(URN playerId, IReadOnlyCollection<CultureInfo> cultures, bool fetchIfMissing);

        /// <summary>
        /// Asynchronously gets a list of competitor name values
        /// </summary>
        /// <param name="competitorId">A <see cref="URN"/> specifying the id of the competitor for which to get the profile</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying languages in which the information should be available</param>
        /// <param name="fetchIfMissing">Indicated if competitor profile should be fetched when <see cref="CompetitorCI"/> is missing name for specified cultures</param>
        /// <returns>A dictionary of name values for all cultures (if missing returns empty string)</returns>
        /// <exception cref="CacheItemNotFoundException">The requested item was not found in cache and could not be obtained from the API</exception>
        Task<IReadOnlyDictionary<CultureInfo, string>> GetCompetitorNamesAsync(URN competitorId, IReadOnlyCollection<CultureInfo> cultures, bool fetchIfMissing);

        /// <summary>
        /// Asynchronously gets a player name in specified culture
        /// </summary>
        /// <param name="playerId">A <see cref="URN"/> specifying the id of the player for which to get the profile</param>
        /// <param name="culture">A specifying languages in which the information should be available</param>
        /// <param name="fetchIfMissing">Indicated if player profile should be fetched when <see cref="PlayerProfileCI"/> is missing name for specified cultures</param>
        /// <returns>A player name in specified culture</returns>
        /// <exception cref="CacheItemNotFoundException">The requested item was not found in cache and could not be obtained from the API</exception>
        Task<string> GetPlayerNameAsync(URN playerId, CultureInfo culture, bool fetchIfMissing);

        /// <summary>
        /// Asynchronously gets a competitor name in specified culture
        /// </summary>
        /// <param name="competitorId">A <see cref="URN"/> specifying the id of the competitor for which to get the profile</param>
        /// <param name="culture">A specifying language in which the information should be available</param>
        /// <param name="fetchIfMissing">Indicated if competitor profile should be fetched when <see cref="CompetitorCI"/> is missing name for specified cultures</param>
        /// <returns>A competitor name in specified culture</returns>
        /// <exception cref="CacheItemNotFoundException">The requested item was not found in cache and could not be obtained from the API</exception>
        Task<string> GetCompetitorNameAsync(URN competitorId, CultureInfo culture, bool fetchIfMissing);
    }
}
