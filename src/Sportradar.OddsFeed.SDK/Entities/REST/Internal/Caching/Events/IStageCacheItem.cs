/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events
{
    /// <summary>
    /// Defines a contract implemented by classes, which represent cached information about a stage
    /// </summary>
    internal interface IStageCacheItem : ICompetitionCacheItem
    {
        /// <summary>
        /// Asynchronously gets <see cref="Urn"/> specifying the id of the category to which the sport event belongs to
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{Urn}"/> representing the asynchronous operation</returns>
        Task<Urn> GetCategoryIdAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets an id of the parent stage of the current instance or a null reference if the represented stage does not have the parent stage
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{StageCacheItem}"/> representing the asynchronous operation</returns>
        Task<Urn> GetParentStageAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a list of additional ids of the parent stages of the current instance or a null reference if the represented stage does not have the parent stages
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{StageCacheItem}"/> representing the asynchronous operation</returns>
        Task<IEnumerable<Urn>> GetAdditionalParentStagesAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{Urn}"/> representing child stages ids of the current instance or a null reference if the represented stage does not have children
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation</returns>
        /// <returns></returns>
        Task<IEnumerable<Urn>> GetStagesAsync(IEnumerable<CultureInfo> cultures);
    }
}
