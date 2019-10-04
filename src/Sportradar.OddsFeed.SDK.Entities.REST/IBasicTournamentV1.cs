/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing tournament information
    /// </summary>
    /// <seealso cref="ILongTermEvent" />
    public interface IBasicTournamentV1 : IBasicTournament
    {
        /// <summary>
        /// Asynchronously gets a <see cref="bool"/> specifying if the tournament is exhibition game
        /// </summary>
        /// <returns>A <see cref="bool"/> specifying if the tournament is exhibition game</returns>
        Task<bool?> GetExhibitionGamesAsync();
    }
}
