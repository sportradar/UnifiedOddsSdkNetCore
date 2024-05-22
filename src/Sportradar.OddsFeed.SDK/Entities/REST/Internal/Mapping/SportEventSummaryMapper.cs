// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> used to map <see cref="matchSummaryEndpoint" /> instances to <see cref="MatchDto" /> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{MatchDto}" />
    internal class SportEventSummaryMapper : ISingleTypeMapper<SportEventSummaryDto>
    {
        /// <summary>
        /// A <see cref="matchSummaryEndpoint"/> containing match data
        /// </summary>
        private readonly matchSummaryEndpoint _matchSummaryData;

        /// <summary>
        /// A <see cref="stageSummaryEndpoint"/> containing stage data
        /// </summary>
        private readonly stageSummaryEndpoint _stageSummaryData;

        /// <summary>
        /// A <see cref="tournamentInfoEndpoint"/> containing tournament data
        /// </summary>
        private readonly tournamentInfoEndpoint _tournamentInfoData;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventSummaryMapper"/> class.
        /// </summary>
        /// <param name="matchSummaryData">A <see cref="matchSummaryEndpoint"/> containing match data</param>
        internal SportEventSummaryMapper(matchSummaryEndpoint matchSummaryData)
        {
            Guard.Argument(matchSummaryData, nameof(matchSummaryData)).NotNull();

            _matchSummaryData = matchSummaryData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventSummaryMapper"/> class.
        /// </summary>
        /// <param name="stageSummaryData">A <see cref="stageSummaryEndpoint"/> containing stage data</param>
        internal SportEventSummaryMapper(stageSummaryEndpoint stageSummaryData)
        {
            Guard.Argument(stageSummaryData, nameof(stageSummaryData)).NotNull();

            _stageSummaryData = stageSummaryData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventSummaryMapper"/> class.
        /// </summary>
        /// <param name="tournamentInfoData">A <see cref="tournamentInfoEndpoint"/> containing tournament data</param>
        internal SportEventSummaryMapper(tournamentInfoEndpoint tournamentInfoData)
        {
            Guard.Argument(tournamentInfoData, nameof(tournamentInfoData)).NotNull();

            _tournamentInfoData = tournamentInfoData;
        }

        public SportEventSummaryDto Map()
        {
            if (_matchSummaryData != null)
            {
                return new MatchDto(_matchSummaryData);
            }
            if (_stageSummaryData != null)
            {
                return new StageDto(_stageSummaryData);
            }
            return new TournamentInfoDto(_tournamentInfoData);
        }
    }
}
