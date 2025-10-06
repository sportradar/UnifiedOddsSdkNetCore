// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// Class PeriodScoreDto
    /// </summary>
    internal class PeriodScoreDto
    {
        /// <summary>
        /// Gets the home score
        /// </summary>
        /// <value>The home score</value>
        public decimal HomeScore { get; }

        /// <summary>
        /// Gets the away score
        /// </summary>
        /// <value>The away score</value>
        public decimal AwayScore { get; }

        /// <summary>
        /// Gets the period number
        /// </summary>
        /// <value>The period number</value>
        public int? PeriodNumber { get; }

        /// <summary>
        /// Gets the match status code
        /// </summary>
        /// <value>The match status code</value>
        public int? MatchStatusCode { get; }

        /// <summary>
        /// Gets the type
        /// </summary>
        /// <value>The type</value>
        public PeriodType? Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodScoreDto"/> class
        /// </summary>
        /// <param name="periodScore">The period score</param>
        public PeriodScoreDto(periodScoreType periodScore)
        {
            HomeScore = periodScore.home_score;
            AwayScore = periodScore.away_score;
            PeriodNumber = periodScore.number;
            MatchStatusCode = periodScore.match_status_code;
            Type = GetPeriodType(null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodScoreDto"/> class
        /// </summary>
        /// <param name="periodScore">The period score</param>
        public PeriodScoreDto(periodScore periodScore)
        {
            if (decimal.TryParse(periodScore.home_score, out var homeScore))
            {
                HomeScore = homeScore;
            }
            else if (!string.IsNullOrEmpty(periodScore.home_score))
            {
                SdkInfo.ExecutionLog.LogWarning("PeriodScore - can not parse home score: {PeriodScoreHomeScore}", periodScore.home_score);
            }
            if (decimal.TryParse(periodScore.away_score, out var awayScore))
            {
                AwayScore = awayScore;
            }
            else if (!string.IsNullOrEmpty(periodScore.away_score))
            {
                SdkInfo.ExecutionLog.LogWarning("PeriodScore - can not parse away score: {PeriodScoreAwayScore}", periodScore.away_score);
            }
            PeriodNumber = periodScore.numberSpecified ? periodScore.number : (int?)null;
            MatchStatusCode = periodScore.match_status_code;
            Type = GetPeriodType(periodScore.type);
        }

        private PeriodType GetPeriodType(string periodType)
        {
            PeriodType? tempPeriodType = null;
            if (!string.IsNullOrEmpty(periodType))
            {
                if (periodType.Equals("overtime", StringComparison.InvariantCultureIgnoreCase))
                {
                    tempPeriodType = PeriodType.Overtime;
                }
                else if (periodType.Equals("penalties", StringComparison.InvariantCultureIgnoreCase))
                {
                    tempPeriodType = PeriodType.Penalties;
                }
                else if (periodType.Equals("regular_period", StringComparison.InvariantCultureIgnoreCase))
                {
                    tempPeriodType = PeriodType.RegularPeriod;
                }
            }

            if (MatchStatusCode != null && tempPeriodType == null)
            {
                if (MatchStatusCode == 40)
                {
                    // <match_status description="Overtime" id="40"/>
                    tempPeriodType = PeriodType.Overtime;
                }
                else if (MatchStatusCode == 50 || MatchStatusCode == 51 || MatchStatusCode == 52 || MatchStatusCode == 120)
                {
                    // <match_status description="Penalties" id="50"/>
                    // <match_status description="Penalties" id="51"/>
                    // <match_status description="Penalties" id="52"/>
                    tempPeriodType = PeriodType.Penalties;
                }
                else if (MatchStatusCode != 0)
                {
                    tempPeriodType = PeriodType.RegularPeriod;
                }
            }

            return tempPeriodType ?? PeriodType.Other;
        }
    }
}
