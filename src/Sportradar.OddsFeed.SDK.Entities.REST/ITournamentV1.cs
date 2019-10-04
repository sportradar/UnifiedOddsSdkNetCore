/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes providing information about tournament schedule
    /// </summary>
    public interface ITournamentV1 : ITournament
    {
        /// <summary>
        /// Asynchronously gets a <see cref="bool"/> specifying if the tournament is exhibition game
        /// </summary>
        /// <returns>A <see cref="bool"/> specifying if the tournament is exhibition game</returns>
        Task<bool?> GetExhibitionGamesAsync();
    }
}
