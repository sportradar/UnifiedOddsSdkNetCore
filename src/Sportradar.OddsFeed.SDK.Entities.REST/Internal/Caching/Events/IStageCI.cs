/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    /// Defines a contract implemented by classes, which represent cached information about a stage
    /// </summary>
    public interface IStageCI : ICompetitionCI
    {
        /// <summary>
        /// Asynchronously gets <see cref="URN"/> specifying the id of the category to which the sport event belongs to
        /// </summary>
        /// <returns>A <see cref="Task{URN}"/> representing the asynchronous operation</returns>
        Task<URN> GetCategoryIdAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="StageCI"/> representing the parent stage of the current instance or a null reference if the represented stage does not have the parent stage
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{StageCI}"/> representing the asynchronous operation</returns>
        Task<StageCI> GetParentStageAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{StageCI}"/> representing child stages of the current instance or a null reference if the represented stage does not have childs
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation</returns>
        /// <returns></returns>
        Task<IEnumerable<StageCI>> GetStagesAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets <see cref="StageType"/> specifying the type of the stage
        /// </summary>
        /// <returns>A <see cref="Task{URN}"/> representing the asynchronous operation</returns>
        Task<StageType> GetTypeAsync();
    }
}