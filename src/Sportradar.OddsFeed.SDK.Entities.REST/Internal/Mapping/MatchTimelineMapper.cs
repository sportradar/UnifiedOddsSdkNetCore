/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Maps <see cref="matchTimelineEndpoint"/> instances to <see cref="MatchTimelineDTO" /> instance
    /// </summary>
    internal class MatchTimelineMapper : ISingleTypeMapper<MatchTimelineDTO>
    {
        /// <summary>
        /// A <see cref="matchTimelineEndpoint"/> instance containing match timeline info
        /// </summary>
        private readonly matchTimelineEndpoint _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchTimelineMapper"/> class
        /// </summary>
        /// <param name="data">>A <see cref="matchTimelineEndpoint"/> instance containing match timeline info</param>
        internal MatchTimelineMapper(matchTimelineEndpoint data)
        {
            Contract.Requires(data != null);

            _data = data;
        }

        /// <summary>
        /// Defines object invariants used by the code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_data != null);
        }

        /// <summary>
        /// Maps it's data to <see cref="MatchTimelineDTO"/> instance
        /// </summary>
        /// <returns>Constructed <see cref="MatchTimelineDTO"/> instance</returns>
        public MatchTimelineDTO Map()
        {
            return new MatchTimelineDTO(_data);
        }
    }
}
