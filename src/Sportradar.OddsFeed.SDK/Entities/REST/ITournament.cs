﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes providing information about tournament schedule
    /// </summary>
    public interface ITournament : ILongTermEvent
    {
        /// <summary>
        /// Asynchronously gets a <see cref="ICategorySummary"/> representing the category to which the tournament belongs to
        /// </summary>
        /// <returns>A <see cref="Task{ISportCategory}"/> representing the asynchronous operation</returns>
        Task<ICategorySummary> GetCategoryAsync();

        /// <summary>
        /// Asynchronously gets <see cref="ICurrentSeasonInfo"/> instance containing detailed information about the current season of the tournament
        /// </summary>
        /// <returns>A <see cref="Task{ICurrentSeasonInfo}"/> representing the asynchronous operation</returns>
        Task<ICurrentSeasonInfo> GetCurrentSeasonAsync();

        /// <summary>
        /// Asynchronously gets a list of <see cref="ISeason"/> associated with this tournament
        /// </summary>
        /// <returns>A list of <see cref="ISeason"/> associated with this tournament</returns>
        Task<IEnumerable<ISeason>> GetSeasonsAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="bool"/> specifying if the tournament is exhibition game
        /// </summary>
        /// <returns>A <see cref="bool"/> specifying if the tournament is exhibition game</returns>
        Task<bool?> GetExhibitionGamesAsync();

        /// <summary>
        /// Gets the list of all <see cref="ICompetition"/> that belongs to the basic tournament schedule
        /// </summary>
        /// <returns>The list of all <see cref="ICompetition"/> that belongs to the basic tournament schedule</returns>
        Task<IEnumerable<ISportEvent>> GetScheduleAsync() => Task.FromResult<IEnumerable<ISportEvent>>(null);
    }
}
