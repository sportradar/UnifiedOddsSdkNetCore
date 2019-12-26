/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Maps <see cref="sportCategoriesEndpoint"/> instances to <see cref="SportCategoriesDTO" /> instance
    /// </summary>
    internal class SportCategoriesMapper : ISingleTypeMapper<SportCategoriesDTO>
    {
        /// <summary>
        /// A <see cref="sportCategoriesEndpoint"/> instance containing sport categories
        /// </summary>
        private readonly sportCategoriesEndpoint _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportCategoriesMapper"/> class
        /// </summary>
        /// <param name="data">>A <see cref="sportCategoriesEndpoint"/> instance containing sport categories</param>
        internal SportCategoriesMapper(sportCategoriesEndpoint data)
        {
            Guard.Argument(data, nameof(data)).NotNull();
            Guard.Argument(data.sport, nameof(data.sport)).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to <see cref="SportCategoriesDTO"/> instance
        /// </summary>
        /// <returns>Constructed <see cref="SportCategoriesDTO"/> instance</returns>
        public SportCategoriesDTO Map()
        {
            return new SportCategoriesDTO(_data);
        }
    }
}
