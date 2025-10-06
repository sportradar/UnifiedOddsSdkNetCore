// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// Maps <see cref="fixtureChangesEndpoint"/> instances to <see cref="EntityList{T}" /> instance
    /// </summary>
    internal class FixtureChangesMapper : ISingleTypeMapper<EntityList<FixtureChangeDto>>
    {
        /// <summary>
        /// A <see cref="fixtureChangesEndpoint"/> instance containing fixture changes
        /// </summary>
        private readonly fixtureChangesEndpoint _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixtureChangesMapper"/> class
        /// </summary>
        /// <param name="data">>A <see cref="fixtureChangesEndpoint"/> instance containing fixture changes</param>
        internal FixtureChangesMapper(fixtureChangesEndpoint data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <summary>
        /// Maps it's data to <see cref="EntityList{FixtureChangeDto}"/> instance
        /// </summary>
        /// <returns>Constructed <see cref="EntityList{FixtureChangeDto}"/> instance</returns>
        public EntityList<FixtureChangeDto> Map()
        {
            var fixtureChanges = new List<FixtureChangeDto>();
            if (_data.fixture_change != null)
            {
                fixtureChanges = _data.fixture_change.Select(f => new FixtureChangeDto(f, _data.generated_atSpecified ? _data.generated_at : (DateTime?)null)).ToList();
            }
            return new EntityList<FixtureChangeDto>(fixtureChanges);
        }
    }
}
