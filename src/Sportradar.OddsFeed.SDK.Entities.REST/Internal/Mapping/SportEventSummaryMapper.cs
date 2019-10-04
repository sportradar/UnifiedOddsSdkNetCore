/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> used to map <see cref="matchSummaryEndpoint" /> instances to <see cref="MatchDTO" /> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{MatchDTO}" />
    internal class SportEventSummaryMapper : ISingleTypeMapper<SportEventSummaryDTO>
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
            Contract.Requires(matchSummaryData != null);

            _matchSummaryData = matchSummaryData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventSummaryMapper"/> class.
        /// </summary>
        /// <param name="stageSummaryData">A <see cref="stageSummaryEndpoint"/> containing stage data</param>
        internal SportEventSummaryMapper(stageSummaryEndpoint stageSummaryData)
        {
            Contract.Requires(stageSummaryData != null);

            _stageSummaryData = stageSummaryData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventSummaryMapper"/> class.
        /// </summary>
        /// <param name="tournamentInfoData">A <see cref="tournamentInfoEndpoint"/> containing tournament data</param>
        internal SportEventSummaryMapper(tournamentInfoEndpoint tournamentInfoData)
        {
            Contract.Requires(tournamentInfoData != null);

            _tournamentInfoData = tournamentInfoData;
        }

        public SportEventSummaryDTO Map()
        {
            if (_matchSummaryData != null)
            {
                return new MatchDTO(_matchSummaryData);
            }
            if (_stageSummaryData != null)
            {
                return new StageDTO(_stageSummaryData);
            }
            return new TournamentInfoDTO(_tournamentInfoData);
        }
    }
}
