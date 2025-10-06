// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// Creates mapper for result changes
    /// </summary>
    internal class ResultChangesMapperFactory : ISingleTypeMapperFactory<resultChangesEndpoint, EntityList<ResultChangeDto>>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance for result changes
        /// </summary>
        /// <param name="data">A <see cref="resultChangesEndpoint" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<EntityList<ResultChangeDto>> CreateMapper(resultChangesEndpoint data)
        {
            return new ResultChangesMapper(data);
        }
    }
}
