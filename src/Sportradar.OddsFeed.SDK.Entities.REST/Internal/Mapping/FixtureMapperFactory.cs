/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{FixtureDTO}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    public class FixtureMapperFactory : ISingleTypeMapperFactory<fixturesEndpoint, FixtureDTO>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="fixturesEndpoint"/> instances to <see cref="FixtureDTO"/> instances
        /// </summary>
        /// <param name="data">A <see cref="fixturesEndpoint" /> instance containing fixture data</param>
        /// <returns>a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="fixturesEndpoint"/> instances to <see cref="FixtureDTO"/> instances</returns>
        public ISingleTypeMapper<FixtureDTO> CreateMapper(fixturesEndpoint data)
        {
            return new FixtureMapper(data);
        }
    }
}
