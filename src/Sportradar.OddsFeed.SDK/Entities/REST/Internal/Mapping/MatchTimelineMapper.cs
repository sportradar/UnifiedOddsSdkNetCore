// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// Maps <see cref="matchTimelineEndpoint"/> instances to <see cref="MatchTimelineDto" /> instance
    /// </summary>
    internal class MatchTimelineMapper : ISingleTypeMapper<MatchTimelineDto>
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
            Guard.Argument(data, nameof(data)).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to <see cref="MatchTimelineDto"/> instance
        /// </summary>
        /// <returns>Constructed <see cref="MatchTimelineDto"/> instance</returns>
        public MatchTimelineDto Map()
        {
            return new MatchTimelineDto(_data);
        }
    }
}
