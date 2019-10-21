/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object for sport categories
    /// </summary>
    public class SportCategoriesDTO
    {
        /// <summary>
        /// Gets the <see cref="SportEntityDTO"/> specifying the the parent sport
        /// </summary>
        public SportEntityDTO Sport { get; }

        /// <summary>
        /// Gets the <see cref="CategoryDTO"/> specifying associated categories
        /// </summary>
        public IEnumerable<CategoryDTO> Categories { get; }

        internal SportCategoriesDTO(sportCategoriesEndpoint categoriesEndpoint)
        {
            Guard.Argument(categoriesEndpoint).NotNull();
            Guard.Argument(categoriesEndpoint.sport).NotNull();

            Sport = new SportEntityDTO(categoriesEndpoint.sport.id, categoriesEndpoint.sport.name);
            Categories = categoriesEndpoint.categories?.Select(c => new CategoryDTO(c.id, c.name, c.country_code, new List<tournamentExtended>())).ToList();
        }
    }
}
