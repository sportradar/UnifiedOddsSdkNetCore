// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// Creates mapper for fixture changes
    /// </summary>
    internal class FixtureChangesMapperFactory : ISingleTypeMapperFactory<fixtureChangesEndpoint, EntityList<FixtureChangeDto>>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance for fixture changes
        /// </summary>
        /// <param name="data">A <see cref="fixtureChangesEndpoint" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<EntityList<FixtureChangeDto>> CreateMapper(fixtureChangesEndpoint data)
        {
            return new FixtureChangesMapper(data);
        }
    }
}
