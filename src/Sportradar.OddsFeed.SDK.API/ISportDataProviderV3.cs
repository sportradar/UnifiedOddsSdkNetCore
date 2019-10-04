/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines a contract implemented by classes used to provide sport related data (sports, tournaments, sport events, ...)
    /// </summary>
    public interface ISportDataProviderV3 : ISportDataProviderV2
    {
        /// <summary>
        /// Asynchronously gets a list of active <see cref="IEnumerable{ISportEvent}"/>
        /// </summary>
        /// <remarks>Lists all <see cref="ISportEvent"/> that are cached (once schedule is loaded)</remarks>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        Task<IEnumerable<ISportEvent>> GetActiveTournamentsAsync(CultureInfo culture = null);

        /// <summary>
        /// Asynchronously gets a list of available <see cref="IEnumerable{ISportEvent}"/> for a specific sport
        /// </summary>
        /// <remarks>Lists all available tournaments for a sport event we provide coverage for</remarks>
        /// <param name="sportId">A <see cref="URN"/> specifying the sport to retrieve</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        Task<IEnumerable<ISportEvent>> GetAvailableTournamentsAsync(URN sportId, CultureInfo culture = null);
    }
}
