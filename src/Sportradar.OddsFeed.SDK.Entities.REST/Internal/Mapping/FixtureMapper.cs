/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}"/> implementation used to map <see cref="fixturesEndpoint"/> instances to <see cref="FixtureDTO"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{FixtureDTO}" />
    internal class FixtureMapper : ISingleTypeMapper<FixtureDTO>
    {
        /// <summary>
        /// A <see cref="fixturesEndpoint"/> instance containing fixture data
        /// </summary>
        private readonly fixturesEndpoint _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixtureMapper"/> class.
        /// </summary>
        /// <param name="data">A <see cref="fixturesEndpoint"/> instance containing fixture data</param>
        internal FixtureMapper(fixturesEndpoint data)
        {
            Guard.Argument(data).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to <see cref="FixtureDTO"/> instance
        /// </summary>
        /// <returns>The created <see cref="FixtureDTO"/> instance </returns>
        public FixtureDTO Map()
        {
            return new FixtureDTO(_data.fixture, _data.generated_atSpecified ? _data.generated_at : (DateTime?) null);
        }
    }
}