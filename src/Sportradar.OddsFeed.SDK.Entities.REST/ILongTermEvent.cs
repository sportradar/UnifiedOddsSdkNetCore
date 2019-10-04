/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing long term sport event
    /// </summary>
    public interface ILongTermEvent : ISportEvent
    {
        /// <summary>
        /// Asynchronously get the <see cref="ISportSummary"/> instance representing the sport associated with the current instance
        /// </summary>
        /// <returns>The <see cref="ISportSummary"/> instance representing the sport associated with the current instance</returns>
        Task<ISportSummary> GetSportAsync();

        /// <summary>
        /// Asynchronously get the <see cref="ITournamentCoverage"/> instance representing the tournament coverage associated with the current instance
        /// </summary>
        /// <returns>The <see cref="ITournamentCoverage"/> instance representing the tournament coverage associated with the current instance</returns>
        Task<ITournamentCoverage> GetTournamentCoverage();
    }
}
