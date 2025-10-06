// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.Api.Managers
{
    /// <summary>
    /// Defines a type used to retrieve market probabilities used for cash out
    /// </summary>
    public interface ICashOutProbabilitiesProvider
    {
        /// <summary>
        /// Asynchronously gets the cash out probabilities for the specified sport event.
        /// </summary>
        /// <typeparam name="T">The type of the sport event</typeparam>
        /// <param name="eventId">The <see cref="Urn"/> uniquely identifying the sport event.</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned data, or a null reference to use default languages</param>
        /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation.</returns>
        Task<ICashOutProbabilities<T>> GetCashOutProbabilitiesAsync<T>(Urn eventId, CultureInfo culture = null) where T : ISportEvent;

        /// <summary>
        /// Asynchronously gets the cash out probabilities for the specified market on the specified sport event.
        /// </summary>
        /// <typeparam name="T">The type of the sport event</typeparam>
        /// <param name="eventId">The <see cref="Urn" /> uniquely identifying the sport event.</param>
        /// <param name="marketId">The id of the market for which to get the probabilities.</param>
        /// <param name="specifiers">A <see cref="IDictionary{String, String}"/> containing market specifiers or a null reference if market has no specifiers.</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned data, or a null reference to use default languages</param>
        /// <returns>A <see cref="Task{T}" /> representing the asynchronous operation.</returns>
        Task<ICashOutProbabilities<T>> GetCashOutProbabilitiesAsync<T>(Urn eventId, int marketId, IReadOnlyDictionary<string, string> specifiers, CultureInfo culture = null) where T : ISportEvent;
    }
}
