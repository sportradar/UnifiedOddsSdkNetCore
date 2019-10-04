/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// Represents a list or collection of sport related entities
    /// </summary>
    /// <typeparam name="T">The type of the entities held by the instance</typeparam>
    public class EntityList<T> where T : class
    {
        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> containing sport related instances associated with the current instance
        /// </summary>
        internal IEnumerable<T> Items { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityList{T}"/> class.
        /// </summary>
        /// <param name="items">a <see cref="IEnumerable{T}"/> containing sport related instances associated with the current instance.</param>
        public EntityList(IEnumerable<T> items)
        {
            Items = items;
        }
    }
}
