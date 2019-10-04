/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Creates mapper for fixture changes
    /// </summary>
    public class FixtureChangesMapperFactory : ISingleTypeMapperFactory<fixtureChangesEndpoint, IEnumerable<FixtureChangeDTO>>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance for fixture changes
        /// </summary>
        /// <param name="data">A <see cref="fixtureChangesEndpoint" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<IEnumerable<FixtureChangeDTO>> CreateMapper(fixtureChangesEndpoint data)
        {
            return new FixtureChangesMapper(data);
        }
    }
}
