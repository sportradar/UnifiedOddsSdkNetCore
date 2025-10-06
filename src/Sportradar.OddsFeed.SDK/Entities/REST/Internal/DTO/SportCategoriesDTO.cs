// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object for sport categories
    /// </summary>
    internal class SportCategoriesDto
    {
        /// <summary>
        /// Gets the <see cref="SportEntityDto"/> specifying the parent sport
        /// </summary>
        public SportEntityDto Sport { get; }

        /// <summary>
        /// Gets the <see cref="CategoryDto"/> specifying associated categories
        /// </summary>
        public IEnumerable<CategoryDto> Categories { get; }

        internal SportCategoriesDto(sportCategoriesEndpoint categoriesEndpoint)
        {
            Guard.Argument(categoriesEndpoint, nameof(categoriesEndpoint)).NotNull();
            Guard.Argument(categoriesEndpoint.sport, nameof(categoriesEndpoint.sport)).NotNull();

            Sport = new SportEntityDto(categoriesEndpoint.sport.id, categoriesEndpoint.sport.name);
            Categories = categoriesEndpoint.categories?.Select(c => new CategoryDto(c.id, c.name, c.country_code, new List<tournamentExtended>())).ToList();
        }
    }
}
