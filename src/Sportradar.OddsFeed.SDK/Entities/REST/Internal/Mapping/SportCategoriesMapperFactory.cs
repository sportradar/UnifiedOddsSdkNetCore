/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Creates mapper for sport categories
    /// </summary>
    public class SportCategoriesMapperFactory : ISingleTypeMapperFactory<sportCategoriesEndpoint, SportCategoriesDTO>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance for sport categories
        /// </summary>
        /// <param name="data">A <see cref="sportCategoriesEndpoint" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<SportCategoriesDTO> CreateMapper(sportCategoriesEndpoint data)
        {
            return new SportCategoriesMapper(data);
        }
    }
}
