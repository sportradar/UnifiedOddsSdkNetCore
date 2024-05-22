// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Exportable;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Caching
{
    /// <summary>
    /// Defines a contract implemented by caches used to store information about player and competitor profiles
    /// </summary>
    internal interface IProfileCache : ISdkCache, IHealthStatusProvider, IDisposable, IExportableSdkCache
    {
        /// <summary>
        /// Asynchronously gets a <see cref="PlayerProfileCacheItem"/> representing the profile for the specified player
        /// </summary>
        /// <param name="playerId">A <see cref="Urn"/> specifying the id of the player for which to get the profile</param>
        /// <param name="wantedLanguages">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying languages in which the information should be available</param>
        /// <param name="fetchIfMissing">Indicated if player profile should be fetched when <see cref="PlayerProfileCacheItem"/> is missing name for specified cultures</param>
        /// <returns>A <see cref="Task{PlayerProfileCacheItem}"/> representing the asynchronous operation</returns>
        /// <exception cref="CacheItemNotFoundException">The requested item was not found in cache and could not be obtained from the API</exception>
        Task<PlayerProfileCacheItem> GetPlayerProfileAsync(Urn playerId, IReadOnlyCollection<CultureInfo> wantedLanguages, bool fetchIfMissing);

        /// <summary>
        /// Asynchronously gets <see cref="CompetitorCacheItem"/> representing the profile for the specified competitor
        /// </summary>
        /// <param name="competitorId">A <see cref="Urn"/> specifying the id of the competitor for which to get the profile</param>
        /// <param name="wantedLanguages">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying languages in which the information should be available</param>
        /// <param name="fetchIfMissing">Indicated if competitor profile should be fetched when <see cref="CompetitorCacheItem"/> is missing name for specified cultures</param>
        /// <returns>A <see cref="Task{CompetitorCacheItem}"/> representing the asynchronous operation</returns>
        /// <exception cref="CacheItemNotFoundException">The requested item was not found in cache and could not be obtained from the API</exception>
        Task<CompetitorCacheItem> GetCompetitorProfileAsync(Urn competitorId, IReadOnlyCollection<CultureInfo> wantedLanguages, bool fetchIfMissing);

        /// <summary>
        /// Asynchronously gets a list of player name values
        /// </summary>
        /// <param name="playerId">A <see cref="Urn"/> specifying the id of the player for which to get the profile</param>
        /// <param name="wantedLanguages">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying languages in which the information should be available</param>
        /// <param name="fetchIfMissing">Indicated if player profile should be fetched when <see cref="PlayerProfileCacheItem"/> is missing name for specified cultures</param>
        /// <returns>A dictionary of name values for all cultures (if missing returns empty string)</returns>
        /// <exception cref="CacheItemNotFoundException">The requested item was not found in cache and could not be obtained from the API</exception>
        Task<IReadOnlyDictionary<CultureInfo, string>> GetPlayerNamesAsync(Urn playerId, IReadOnlyCollection<CultureInfo> wantedLanguages, bool fetchIfMissing);

        /// <summary>
        /// Asynchronously gets a list of competitor name values
        /// </summary>
        /// <param name="competitorId">A <see cref="Urn"/> specifying the id of the competitor for which to get the profile</param>
        /// <param name="wantedLanguages">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying languages in which the information should be available</param>
        /// <param name="fetchIfMissing">Indicated if competitor profile should be fetched when <see cref="CompetitorCacheItem"/> is missing name for specified cultures</param>
        /// <returns>A dictionary of name values for all cultures (if missing returns empty string)</returns>
        /// <exception cref="CacheItemNotFoundException">The requested item was not found in cache and could not be obtained from the API</exception>
        Task<IReadOnlyDictionary<CultureInfo, string>> GetCompetitorNamesAsync(Urn competitorId, IReadOnlyCollection<CultureInfo> wantedLanguages, bool fetchIfMissing);

        /// <summary>
        /// Asynchronously gets a player name in specified culture
        /// </summary>
        /// <param name="playerId">A <see cref="Urn"/> specifying the id of the player for which to get the profile</param>
        /// <param name="wantedLanguage">A specifying languages in which the information should be available</param>
        /// <param name="fetchIfMissing">Indicated if player profile should be fetched when <see cref="PlayerProfileCacheItem"/> is missing name for specified cultures</param>
        /// <returns>A player name in specified culture</returns>
        /// <exception cref="CacheItemNotFoundException">The requested item was not found in cache and could not be obtained from the API</exception>
        Task<string> GetPlayerNameAsync(Urn playerId, CultureInfo wantedLanguage, bool fetchIfMissing);

        /// <summary>
        /// Asynchronously gets a competitor name in specified culture
        /// </summary>
        /// <param name="competitorId">A <see cref="Urn"/> specifying the id of the competitor for which to get the profile</param>
        /// <param name="wantedLanguage">A specifying language in which the information should be available</param>
        /// <param name="fetchIfMissing">Indicated if competitor profile should be fetched when <see cref="CompetitorCacheItem"/> is missing name for specified cultures</param>
        /// <returns>A competitor name in specified culture</returns>
        /// <exception cref="CacheItemNotFoundException">The requested item was not found in cache and could not be obtained from the API</exception>
        Task<string> GetCompetitorNameAsync(Urn competitorId, CultureInfo wantedLanguage, bool fetchIfMissing);
    }
}
