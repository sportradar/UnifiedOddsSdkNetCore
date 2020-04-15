/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing tournament information
    /// </summary>
    /// <seealso cref="ILongTermEvent" />
    public interface IBasicTournament : ILongTermEvent
    {
        /// <summary>
        /// Asynchronously gets the category
        /// </summary>
        /// <returns>The category</returns>
        Task<ICategorySummary> GetCategoryAsync();

        /// <summary>
        /// Asynchronously gets the competitors
        /// </summary>
        /// <returns>The competitors</returns>
        Task<IEnumerable<ICompetitor>> GetCompetitorsAsync();

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
