/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
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
            Guard.Argument(id).NotNull().NotEmpty();
            Guard.Argument(name).NotNull().NotEmpty();

            CountryCode = countryCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryDTO"/> class
        /// </summary>
        internal CategorySummaryDTO(category category)
            : base(category.id, category.name)
        {
            Guard.Argument(category).NotNull();

            CountryCode = category.country_code;
        }
    }
}
