/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.MarketMapping;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Represents a betting market
    /// </summary>
    public interface IMarket
    {
        /// <summary>
        /// Gets a <see cref="int"/> value specifying the market type
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{String, String}"/> containing market specifiers
        /// </summary>
        /// <remarks>Note that the <see cref="Id"/> and <see cref="Specifiers"/> combined uniquely identify the market within the event</remarks>
        IReadOnlyDictionary<string, string> Specifiers { get; }

        /// <summary>
        /// Gets the <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing additional market information
        /// </summary>
        IReadOnlyDictionary<string, string> AdditionalInfo { get; }

        /// <summary>
        /// Gets the associated market definition instance
        /// </summary>
        IMarketDefinition MarketDefinition { get; }

        /// <summary>
        /// Asynchronously gets the name of the market in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language in which to get the name</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the async operation</returns>
        Task<string> GetNameAsync(CultureInfo culture);

        /// <summary>
        /// Asynchronously gets the mapping Id of the specified market
        /// </summary>
        /// <returns>Returns the mapping Id of the specified market</returns>
        [Obsolete("Return only the first mapping. Use GetMappedMarketIdsAsync for all possible.")]
        Task<IMarketMapping> GetMappedMarketIdAsync();

        /// <summary>
        /// Asynchronously gets the mapping Ids of the specified market
        /// </summary>
        /// <returns>Returns the mapping Ids of the specified market</returns>
        Task<IEnumerable<IMarketMapping>> GetMappedMarketIdsAsync();
    }
}
