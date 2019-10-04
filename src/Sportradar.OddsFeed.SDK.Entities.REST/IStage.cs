/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines methods implemented by classes representing sport events of stage type
    /// </summary>
    public interface IStage : ICompetition
    {
        /// <summary>
        /// Asynchronously get the <see cref="ISportSummary"/> instance associated with the current <see cref="IStage"/> instance
        /// </summary>
        /// <returns>The <see cref="ISportSummary"/> instance associated with the current <see cref="IStage"/> instance</returns>
        Task<ISportSummary> GetSportAsync();

        /// <summary>
        /// Asynchronously get the <see cref="ICategorySummary"/> instance associated with the current <see cref="IStage"/> instance
        /// </summary>
        /// <returns>The <see cref="ICategorySummary"/> instance associated with the current <see cref="IStage"/> instance</returns>
        Task<ICategorySummary> GetCategoryAsync();

        /// <summary>
        /// Asynchronously get the parent stage
        /// </summary>
        /// <returns>The parent stage</returns>
        Task<IStage> GetParentStageAsync();

        /// <summary>
        /// Asynchronously get the list of stages representing stages of the multi-stage stage
        /// </summary>
        /// <returns>The list of stages representing stages of the multi-stage stage</returns>
        Task<IEnumerable<IStage>> GetStagesAsync();

        /// <summary>
        /// Asynchronously get the type of the stage
        /// </summary>
        /// <returns>The type of the stage</returns>
        Task<StageType> GetStageTypeAsync();
    }
}
