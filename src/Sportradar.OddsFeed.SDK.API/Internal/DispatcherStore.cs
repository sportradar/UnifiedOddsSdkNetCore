/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// A store used to manage <see cref="IEntityDispatcherInternal"/> instances
    /// </summary>
    /// <seealso cref="Sportradar.OddsFeed.SDK.API.Internal.IDispatcherStore" />
    internal class DispatcherStore : IDispatcherStore
    {
        /// <summary>
        /// The <see cref="IEntityTypeMapper"/> used to determine the SDK type used to represent a specific sport entity
        /// </summary>
        private readonly IEntityTypeMapper _typeMapper;

        /// <summary>
        /// The <see cref="IDictionary{String, ISpecificEntityDispatcherInternal}"/> containing added dispatchers
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
            Contract.Requires(typeMapper != null);

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
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<List<Type>>() != null);

            var key = type.Name;
            List<Type> hierarchies;
            if (_cachedHierarchies.TryGetValue(key, out hierarchies))
            {
                return hierarchies;
            }

            var baseTypes = type.GetInterfaces();
            var hierarchy = new List<Type>(baseTypes.Where(baseType => (typeof(ICompetition)).IsAssignableFrom(baseType)));
            _cachedHierarchies.Add(key, hierarchy);

            return hierarchy;
        }

        /// <summary>
        /// Defined field invariants needed by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_typeMapper != null);
            Contract.Invariant(_dispatchers != null);
            Contract.Invariant(_syncLock != null);
            Contract.Invariant(_cachedHierarchies != null);
        }

        /// <summary>
        /// Adds the provided <see cref="ISpecificEntityDispatcherInternal"/> to the current store.
        /// </summary>
        /// <param name="dispatcher">The <see cref="ISpecificEntityDispatcherInternal"/> instance to be added.</param>
        public void Add(ISpecificEntityDispatcherInternal dispatcher)
        {
            var key = dispatcher.GetType().GetGenericArguments().First().Name;
            lock (_syncLock)
            {
                if (_dispatchers.ContainsKey(key))
                {
                    throw new InvalidOperationException($"Dispatcher for entities of type:{0} is already registered");
                }
                dispatcher.OnClosed += OnDispatcherClosed;
                _dispatchers.Add(key, dispatcher);
            }
        }


        /// <summary>
        /// Gets the <see cref="ISpecificEntityDispatcherInternal"/> instance associated with the sport entity associated with provided <code>id</code> and <code>sportId</code>
        /// </summary>
        /// <param name="id">The <see cref="URN"/> representing the identifier of the sport entity for which to get the dispatcher</param>
        /// <param name="sportId">The <see cref="URN"/> representing the id of the sport to which the sport entity belongs to.</param>
        /// <returns>IEntityDispatcherInternal used to dispatch the instance specified by <code>id</code> and <code>sportId</code>.</returns>
        public ISpecificEntityDispatcherInternal Get(URN id, URN sportId)
        {
            var entityType = _typeMapper.Map(id, (int)sportId.Id);
            var key = entityType.Name;

            lock (_syncLock)
            {
                ISpecificEntityDispatcherInternal dispatcher;
                if (_dispatchers.TryGetValue(key, out dispatcher) && dispatcher.IsOpened)
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