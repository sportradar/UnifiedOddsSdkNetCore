/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
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
            Guard.Argument(data).NotNull();

            _data = data;
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
