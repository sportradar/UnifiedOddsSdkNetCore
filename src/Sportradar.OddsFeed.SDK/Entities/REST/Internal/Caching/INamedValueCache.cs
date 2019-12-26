/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching
{
    /// <summary>
    /// Defines a contract implemented by classes used to cache <see cref="INamedValue"/> instances
    /// </summary>
    internal interface INamedValueCache
    {
        /// <summary>
        /// Gets the <see cref="INamedValue"/> specified by it's id
        /// </summary>
        /// <param name="id">The id of the <see cref="INamedValue"/> to retrieve</param>
        /// <returns>The specified <see cref="INamedValue"/></returns>
        INamedValue GetNamedValue(int id);

        /// <summary>
        /// Determines whether specified id is present int the cache
        /// </summary>
        /// <param name="id">The id to be tested</param>
        /// <returns>True if the value is defined in the cache; False otherwise</returns>
        bool IsValueDefined(int id);
    }
}