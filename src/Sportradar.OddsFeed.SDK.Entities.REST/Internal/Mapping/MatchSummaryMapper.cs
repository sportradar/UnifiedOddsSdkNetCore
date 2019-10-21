/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> used to map <see cref="matchSummaryEndpoint" /> instances to <see cref="MatchDTO" /> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{MatchDTO}" />
    internal class MatchSummaryMapper : ISingleTypeMapper<MatchDTO>
    {
        /// <summary>
        /// A <see cref="matchSummaryEndpoint"/> containing sport event data
        /// </summary>
        private readonly matchSummaryEndpoint _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchSummaryMapper"/> class.
        /// </summary>
        /// <param name="data">A <see cref="matchSummaryEndpoint"/> containing sport event data</param>
        internal MatchSummaryMapper(matchSummaryEndpoint data)
        {
            Guard.Argument(data).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to instance of <see cref="SportEventSummaryDTO" />
        /// </summary>
        /// <returns>The created <see cref="SportEventSummaryDTO" /> instance</returns>
        public MatchDTO Map()
        {
            return new MatchDTO(_data);
        }
    }
}
