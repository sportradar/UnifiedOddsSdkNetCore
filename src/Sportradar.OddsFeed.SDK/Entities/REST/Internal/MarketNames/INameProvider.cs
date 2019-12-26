/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Globalization;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// Defines a contract implemented by classes used to provide names of markets and outcomes
    /// </summary>
    public interface INameProvider
    {
        /// <summary>
        /// Asynchronously gets the name of the specified market in the requested language
        /// </summary>
        /// <param name="culture">The language of the returned name</param>
        /// <returns>A <see cref="Task{String}"/> representing the asynchronous operation</returns>
        Task<string> GetMarketNameAsync(CultureInfo culture);

        /// <summary>
        /// Asynchronously gets the name of the specified outcome in the requested language
        /// </summary>
        /// <param name="outcomeId">The outcome identifier.</param>
        /// <param name="culture">The language of the returned name</param>
        /// <returns>A <see cref="Task{String}"/> representing the asynchronous operation</returns>
        Task<string> GetOutcomeNameAsync(string outcomeId, CultureInfo culture);
    }
}