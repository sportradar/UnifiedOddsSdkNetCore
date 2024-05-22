// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}"/> implementation used to map <see cref="fixturesEndpoint"/> instances to <see cref="FixtureDto"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{FixtureDto}" />
    internal class FixtureMapper : ISingleTypeMapper<FixtureDto>
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
            Guard.Argument(data, nameof(data)).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to <see cref="FixtureDto"/> instance
        /// </summary>
        /// <returns>The created <see cref="FixtureDto"/> instance </returns>
        public FixtureDto Map()
        {
            return new FixtureDto(_data.fixture, _data.generated_atSpecified ? _data.generated_at : (DateTime?)null);
        }
    }
}
