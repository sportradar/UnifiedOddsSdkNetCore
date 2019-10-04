/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Sports
{
    /// <summary>
    /// Contains basic information about a sport tournament
    /// </summary>
    /// <seealso cref="SportEntityData" />
    public class CategoryData : SportEntityData
    {
        /// <summary>
        /// Gets the <see cref="IEnumerable{URN}"/> representing the ids of tournaments, which belong to category represented by the current instance
        /// </summary>
        public readonly IEnumerable<URN> Tournaments;

        /// <summary>
        /// Gets the country code
        /// </summary>
        /// <value>The country code</value>
        public string CountryCode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryData"/> class.
        /// </summary>
        /// <param name="id">a <see cref="URN"/> specifying the id of the associated category</param>
        /// <param name="names">a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing translated entity name</param>
        /// <param name="countryCode">the country code</param>
        /// <param name="tournaments">the <see cref="IEnumerable{URN}"/> representing the tournaments, which belong to the category</param>
        public CategoryData(URN id, IReadOnlyDictionary<CultureInfo, string> names, string countryCode, IEnumerable<URN> tournaments)
            :base(id, names)
        {
            //Contract.Requires(tournaments != null); // on WNSs there is no tournament
            //Contract.Requires(Contract.Exists(tournaments, t => true));

            CountryCode = countryCode;

            Tournaments = tournaments as IReadOnlyCollection<URN>;
        }
    }
}