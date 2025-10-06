// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// Creates mapper for sport categories
    /// </summary>
    internal class SportCategoriesMapperFactory : ISingleTypeMapperFactory<sportCategoriesEndpoint, SportCategoriesDto>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance for sport categories
        /// </summary>
        /// <param name="data">A <see cref="sportCategoriesEndpoint" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<SportCategoriesDto> CreateMapper(sportCategoriesEndpoint data)
        {
            return new SportCategoriesMapper(data);
        }
    }
}
