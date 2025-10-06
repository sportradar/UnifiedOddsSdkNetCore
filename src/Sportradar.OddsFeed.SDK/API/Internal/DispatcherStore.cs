// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.Api.Internal
{
    /// <summary>
    /// A store used to manage <see cref="IEntityDispatcherInternal"/> instances
    /// </summary>
    /// <seealso cref="IDispatcherStore" />
    internal class DispatcherStore : IDispatcherStore
    {
        /// <summary>
        /// The <see cref="IEntityTypeMapper"/> used to determine the SDK type used to represent a specific sport entity
        /// </summary>
        private readonly IEntityTypeMapper _typeMapper;

        /// <summary>
        /// The <see cref="IDictionary{String,ISpecificEntityDispatcherInternal}"/> containing added dispatchers
        /// </summary>
        private readonly IDictionary<string, ISpecificEntityDispatcherInternal> _dispatchers = new Dictionary<string, ISpecificEntityDispatcherInternal>();

        /// <summary>
        /// A <see cref="object"/> used to enforce multi-thread safety
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// A <see cref="IDictionary{TKey, TValue}"/> containing already loaded type hierarchies
        /// </summary>
        private readonly IDictionary<string, List<Type>> _cachedHierarchies = new Dictionary<string, List<Type>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatcherStore"/> class.
        /// </summary>
        /// <param name="typeMapper">The <see cref="IEntityTypeMapper"/> used to determine the SDK type used to represent a specific sport entity.</param>
        public DispatcherStore(IEntityTypeMapper typeMapper)
        {
            Guard.Argument(typeMapper, nameof(typeMapper)).NotNull();

            _typeMapper = typeMapper;
        }

        /// <summary>
        /// Invoked when a dispatcher handled by the current store is closed
        /// </summary>
        /// <param name="sender">A <see cref="object"/> representation of the dispatcher being disposed.</param>
        /// <param name="e">The <see cref="EventArgs"/> providing further information about the event.</param>
        private void OnDispatcherClosed(object sender, EventArgs e)
        {
            var dispatcher = (ISpecificEntityDispatcherInternal)sender;
            dispatcher.OnClosed -= OnDispatcherClosed;
            var key = dispatcher.GetType().GetGenericArguments().First().Name;
            lock (_syncLock)
            {
                _dispatchers.Remove(key);
            }
        }

        /// <summary>
        /// Gets the hierarchy of the interfaces implemented by the type represented by the provided <see cref="Type"/> instance
        /// </summary>
        /// <param name="type">The <see cref="Type"/> representing the type for which to get the hierarchy.</param>
        /// <returns>A <see cref="List{Type}"/> representing the implemented interfaces</returns>
        private List<Type> GetHierarchy(Type type)
        {
            Guard.Argument(type, nameof(type)).NotNull();

            var key = type.Name;
            if (_cachedHierarchies.TryGetValue(key, out var hierarchies))
            {
                return hierarchies;
            }

            var baseTypes = type.GetInterfaces();
            var hierarchy = new List<Type>(baseTypes.Where(baseType => typeof(ICompetition).IsAssignableFrom(baseType)));
            _cachedHierarchies.Add(key, hierarchy);

            return hierarchy;
        }

        /// <summary>
        /// Adds the provided <see cref="ISpecificEntityDispatcherInternal"/> to the current store.
        /// </summary>
        /// <param name="dispatcher">The <see cref="ISpecificEntityDispatcherInternal"/> instance to be added.</param>
        public void Add(ISpecificEntityDispatcherInternal dispatcher)
        {
            Guard.Argument(dispatcher, nameof(dispatcher)).NotNull();

            var key = dispatcher.GetType().GetGenericArguments().First().Name;
            lock (_syncLock)
            {
                if (_dispatchers.ContainsKey(key))
                {
                    throw new InvalidOperationException($"Dispatcher for entities of type:{key} is already registered");
                }
                dispatcher.OnClosed += OnDispatcherClosed;
                _dispatchers.Add(key, dispatcher);
            }
        }

        /// <summary>
        /// Gets the <see cref="ISpecificEntityDispatcherInternal"/> instance associated with the sport entity associated with provided <c>id</c> and <c>sportId</c>
        /// </summary>
        /// <param name="id">The <see cref="Urn"/> representing the identifier of the sport entity for which to get the dispatcher</param>
        /// <param name="sportId">The <see cref="Urn"/> representing the id of the sport to which the sport entity belongs to.</param>
        /// <returns>IEntityDispatcherInternal used to dispatch the instance specified by <c>id</c> and <c>sportId</c>.</returns>
        public ISpecificEntityDispatcherInternal Get(Urn id, Urn sportId)
        {
            var entityType = _typeMapper.Map(id, (int)sportId.Id);
            var key = entityType.Name;

            lock (_syncLock)
            {
                if (_dispatchers.TryGetValue(key, out var dispatcher) && dispatcher.IsOpened)
                {
                    return dispatcher;
                }

                var entityHierarchy = GetHierarchy(entityType);
                foreach (var baseType in entityHierarchy)
                {
                    key = baseType.Name;
                    if (_dispatchers.TryGetValue(key, out dispatcher) && dispatcher.IsOpened)
                    {
                        return _dispatchers[key];
                    }
                }
            }
            return null;
        }
    }
}
