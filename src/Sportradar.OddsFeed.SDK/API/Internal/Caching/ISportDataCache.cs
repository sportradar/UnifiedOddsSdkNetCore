/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Sports;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Caching
{
    /// <summary>
    /// Defines a contract implemented by classes used to cache Sport data
    /// </summary>
    internal interface ISportDataCache : ISdkCache, IHealthStatusProvider, IDisposable, IExportableSdkCache
    {
        /// <summary>
        /// Asynchronously gets a <see cref="IReadOnlyCollection{SportData}"/> representing sport hierarchies for all sports supported by the feed.
        /// </summary>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying the languages in which the data is returned</param>
        /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation</returns>
        Task<IEnumerable<SportData>> GetSportsAsync(IReadOnlyCollection<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a <see cref="SportData"/> instance representing the sport hierarchy for the specified sport
        /// </summary>
        /// <param name="id">The <see cref="Urn"/> specifying the id of the sport</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying the languages in which the data is returned</param>
        /// <returns>A <see cref="Task{SportData}"/> representing the asynchronous operation</returns>
        Task<SportData> GetSportAsync(Urn id, IReadOnlyCollection<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a <see cref="CategoryData"/> instance
        /// </summary>
        /// <param name="id">The <see cref="Urn"/> specifying the id of the category</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying the languages in which the data is returned</param>
        /// <returns>A <see cref="Task{CategoryData}"/> representing the asynchronous operation</returns>
        Task<CategoryData> GetCategoryAsync(Urn id, IReadOnlyCollection<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a <see cref="SportData"/> representing sport associated with the tournament specified by it's id. Note that the hierarchy will only contain the
        /// specified tournament and it's parent category not all categories / tournaments in the hierarchy
        /// </summary>
        /// <param name="tournamentId">A <see cref="Urn"/> specifying the id of the tournament</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying the languages in which the data is returned</param>
        /// <returns>A <see cref="Task{SportData}"/> representing the asynchronous operation</returns>
        Task<SportData> GetSportForTournamentAsync(Urn tournamentId, IReadOnlyCollection<CultureInfo> cultures);

        /// <summary>
        /// Loads all tournaments for all sports asynchronous.
        /// </summary>
        /// <returns>Task.</returns>
        Task LoadAllTournamentsForAllSportsAsync();
    }
}
