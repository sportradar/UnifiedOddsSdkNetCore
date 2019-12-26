/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Maps <see cref="fixtureChangesEndpoint"/> instances to <see cref="IEnumerable{FixtureChangeDTO}" /> instance
    /// </summary>
    internal class FixtureChangesMapper : ISingleTypeMapper<IEnumerable<FixtureChangeDTO>>
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
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.fixture_change == null)
                throw  new ArgumentNullException(nameof(data.fixture_change));

            _data = data;
        }

        /// <summary>
        /// Maps it's data to <see cref="IEnumerable{FixtureChangeDTO}"/> instance
        /// </summary>
        /// <returns>Constructed <see cref="IEnumerable{FixtureChangeDTO}"/> instance</returns>
        public IEnumerable<FixtureChangeDTO> Map()
        {
            return _data.fixture_change.Select(f => new FixtureChangeDTO(f, _data.generated_atSpecified ? _data.generated_at : (DateTime?) null)).ToList();
        }
    }
}
