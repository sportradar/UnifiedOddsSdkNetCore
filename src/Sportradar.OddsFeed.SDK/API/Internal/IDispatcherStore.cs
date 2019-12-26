/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes used to hold user registered dispatchers
    /// </summary>
    internal interface IDispatcherStore
    {
        /// <summary>
        /// Adds the provided <see cref="ISpecificEntityDispatcherInternal"/> to the current store.
        /// </summary>
        /// <param name="dispatcher">The <see cref="ISpecificEntityDispatcherInternal"/> instance to be added.</param>
        void Add(ISpecificEntityDispatcherInternal dispatcher);

        /// <summary>
        /// Gets the <see cref="ISpecificEntityDispatcherInternal"/> instance associated with the sport entity associated with provided <code>id</code> and <code>sportId</code>
        /// </summary>
        /// <param name="id">The <see cref="URN"/> representing the identifier of the sport entity for which to get the dispatcher</param>
        /// <param name="sportId">The <see cref="URN"/> representing the id of the sport to which the sport entity belongs to.</param>
        /// <returns>IEntityDispatcherInternal used to dispatch the instance specified by <code>id</code> and <code>sportId</code>.</returns>
        ISpecificEntityDispatcherInternal Get(URN id, URN sportId);
    }
}
