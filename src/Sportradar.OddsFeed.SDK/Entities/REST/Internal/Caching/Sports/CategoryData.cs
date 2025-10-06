// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Sports
{
    /// <summary>
    /// Contains basic information about a sport tournament
    /// </summary>
    /// <seealso cref="SportEntityData" />
    internal class CategoryData : SportEntityData
    {
        /// <summary>
        /// Gets the <see cref="IEnumerable{Urn}"/> representing the ids of tournaments, which belong to category represented by the current instance
        /// </summary>
        public readonly IEnumerable<Urn> Tournaments;

        /// <summary>
        /// Gets the country code
        /// </summary>
        /// <value>The country code</value>
        public string CountryCode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryData"/> class.
        /// </summary>
        /// <param name="id">a <see cref="Urn"/> specifying the id of the associated category</param>
        /// <param name="names">a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing translated entity name</param>
        /// <param name="countryCode">the country code</param>
        /// <param name="tournaments">the <see cref="IEnumerable{Urn}"/> representing the tournaments, which belong to the category</param>
        public CategoryData(Urn id, IReadOnlyDictionary<CultureInfo, string> names, string countryCode, IEnumerable<Urn> tournaments)
            : base(id, names)
        {
            //Guard.Argument(tournaments, nameof()).NotNull(); // on WNSs there is no tournament
            //Guard.Argument(Contract.Exists(tournaments, t => true));

            CountryCode = countryCode;

            Tournaments = tournaments as IReadOnlyCollection<Urn>;
        }
    }
}
