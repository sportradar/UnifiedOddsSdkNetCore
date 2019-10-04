/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing the target(tournament, match, race) of feed messages
    /// </summary>
    public interface ISportEvent
    {
        /// <summary>
        /// Gets a <see cref="URN"/> uniquely identifying the sport event associated with the current instance
        /// </summary>
        URN Id { get; }

        /// <summary>
        /// Asynchronously gets the name of the sport event
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned name</param>
        /// <returns>Return a name of the race, or match</returns>
        Task<string> GetNameAsync(CultureInfo culture);

        /// <summary>
        /// Asynchronously gets a <see cref="URN"/> uniquely identifying the sport associated with the current instance
        /// </summary>
        /// <returns>Returns a <see cref="URN"/> uniquely identifying the sport associated with the current instance</returns>
        Task<URN> GetSportIdAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="DateTime" /> instance specifying for when the sport event associated with the current instance is
        /// scheduled or a null reference if the value is not known
        /// </summary>
        /// <returns>A <see cref="Task{DateTime}"/> representing the retrieval operation</returns>
        Task<DateTime?> GetScheduledTimeAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="DateTime" /> instance specifying for when the sport event associated with the current instance is
        /// scheduled to end or a null reference if the value is not known
        /// </summary>
        /// <returns>A <see cref="Task{DateTime}"/> representing the retrieval operation</returns>
        Task<DateTime?> GetScheduledEndTimeAsync();
    }
}
