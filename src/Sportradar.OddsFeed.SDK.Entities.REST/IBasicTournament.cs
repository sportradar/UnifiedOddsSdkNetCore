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
    }
}
