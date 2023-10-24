/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Sports
{
    /// <summary>
    /// Contains sport related entity (sport, category, tournament) data
    /// </summary>
    internal abstract class SportEntityData
    {
        /// <summary>
        /// Gets <see cref="Urn"/> specifying the id of the associated entity.
        /// </summary>
        public Urn Id { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing translated entity name
        /// </summary>
        public IReadOnlyDictionary<CultureInfo, string> Names { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEntityData"/> class.
        /// </summary>
        /// <param name="id">a <see cref="Urn"/> specifying the id of the associated entity</param>
        /// <param name="names">a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing translated entity name</param>
        protected SportEntityData(Urn id, IReadOnlyDictionary<CultureInfo, string> names)
        {
            Guard.Argument(id, nameof(id)).NotNull();
            Guard.Argument(names, nameof(names)).NotNull().NotEmpty();

            Id = id;
            Names = names;
        }
    }
}
