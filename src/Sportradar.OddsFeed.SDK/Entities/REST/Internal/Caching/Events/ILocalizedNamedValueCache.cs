/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    /// Defines a contract implemented by classes used to cache <see cref="ILocalizedNamedValue"/> instances
    /// </summary>
    public interface ILocalizedNamedValueCache
    {
        /// <summary>
        /// Gets the match status translations asynchronously for specified cultures
        /// </summary>
        /// <param name="id">The id of the <see cref="ILocalizedNamedValue"/> to retrieve</param>
        /// <param name="cultures">The cultures to be used to retrieve descriptions</param>
        /// <returns>A <see cref="Task{ILocalizedNamedValue}"/> representing the async operation</returns>
        Task<ILocalizedNamedValue> GetAsync(int id, IEnumerable<CultureInfo> cultures = null);

        /// <summary>
        /// Determines whether specified id is present int the cache
        /// </summary>
        /// <param name="id">The id to be tested.</param>
        /// <returns>True if the value is defined in the cache; False otherwise.</returns>
        bool IsValueDefined(int id);
    }
}