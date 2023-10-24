/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{FixtureDto}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    internal class FixtureMapperFactory : ISingleTypeMapperFactory<fixturesEndpoint, FixtureDto>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="fixturesEndpoint"/> instances to <see cref="FixtureDto"/> instances
        /// </summary>
        /// <param name="data">A <see cref="fixturesEndpoint" /> instance containing fixture data</param>
        /// <returns>a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="fixturesEndpoint"/> instances to <see cref="FixtureDto"/> instances</returns>
        public ISingleTypeMapper<FixtureDto> CreateMapper(fixturesEndpoint data)
        {
            return new FixtureMapper(data);
        }
    }
}
