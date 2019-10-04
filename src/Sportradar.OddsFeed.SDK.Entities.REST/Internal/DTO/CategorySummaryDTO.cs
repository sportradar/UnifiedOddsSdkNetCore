/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representing a sport category
    /// </summary>
    public class CategorySummaryDTO : SportEntityDTO
    {
        /// <summary>
        /// The country code
        /// </summary>
        public readonly string CountryCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryDTO"/> class
        /// </summary>
        /// <param name="id">The id of the category</param>
        /// <param name="name">The name of the category</param>
        /// <param name="countryCode">A country code</param>
        internal CategorySummaryDTO(string id, string name, string countryCode)
            :base(id, name)
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
            Contract.Requires(!string.IsNullOrEmpty(name));

            CountryCode = countryCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryDTO"/> class
        /// </summary>
        internal CategorySummaryDTO(category category)
            : base(category.id, category.name)
        {
            Contract.Requires(category != null);

            CountryCode = category.country_code;
        }
    }
}
