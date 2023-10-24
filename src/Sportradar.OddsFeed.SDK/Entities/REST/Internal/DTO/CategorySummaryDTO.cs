/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object representing a sport category
    /// </summary>
    internal class CategorySummaryDto : SportEntityDto
    {
        /// <summary>
        /// The country code
        /// </summary>
        public readonly string CountryCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryDto"/> class
        /// </summary>
        /// <param name="id">The id of the category</param>
        /// <param name="name">The name of the category</param>
        /// <param name="countryCode">A country code</param>
        internal CategorySummaryDto(string id, string name, string countryCode)
            : base(id, name)
        {
            Guard.Argument(id, nameof(id)).NotNull().NotEmpty();
            Guard.Argument(name, nameof(name)).NotNull().NotEmpty();

            CountryCode = countryCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryDto"/> class
        /// </summary>
        internal CategorySummaryDto(category category)
            : base(category.id, category.name)
        {
            Guard.Argument(category, nameof(category)).NotNull();

            CountryCode = category.country_code;
        }
    }
}
