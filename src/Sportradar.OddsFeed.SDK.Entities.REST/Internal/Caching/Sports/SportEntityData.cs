/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Sports
{
    /// <summary>
    /// Contains sport related entity (sport, category, tournament) data
    /// </summary>
    public abstract class SportEntityData
    {
        /// <summary>
        /// Gets <see cref="URN"/> specifying the id of the associated entity.
        /// </summary>
        public URN Id { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing translated entity name
        /// </summary>
        public IReadOnlyDictionary<CultureInfo, string> Names { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEntityData"/> class.
        /// </summary>
        /// <param name="id">a <see cref="URN"/> specifying the id of the associated entity</param>
        /// <param name="names">a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing translated entity name</param>
        protected SportEntityData(URN id, IReadOnlyDictionary<CultureInfo, string> names)
        {
            Contract.Requires(id != null);
            Contract.Requires(names != null);
            Contract.Requires(names.Any());

            Id = id;
            Names = names;
        }

        /// <summary>
        /// Defined field invariants needed by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Id != null);
            Contract.Invariant(Names != null);
            Contract.Invariant(Names.Any());
        }
    }
}