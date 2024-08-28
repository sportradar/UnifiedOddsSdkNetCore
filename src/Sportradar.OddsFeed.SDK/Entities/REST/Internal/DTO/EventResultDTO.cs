// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// Class EventResultDto
    /// </summary>
    internal class EventResultDto
    {
        /// <summary>
        /// Gets the identifier
        /// </summary>
        /// <value>The identifier</value>
        public string Id { get; }

        /// <summary>
        /// Gets the position
        /// </summary>
        /// <value>The position</value>
        public int? Position { get; }

        /// <summary>
        /// Gets the points
        /// </summary>
        /// <value>The points</value>
        public decimal? PointsDecimal { get; }

        /// <summary>
        /// Gets the wc? points
        /// </summary>
        /// <value>The wc? points</value>
        public decimal? WcPoints { get; }

        /// <summary>
        /// Gets the time
        /// </summary>
        /// <value>The time</value>
        public string Time { get; }

        /// <summary>
        /// Gets the time ranking
        /// </summary>
        /// <value>The time ranking</value>
        public int? TimeRanking { get; }

        /// <summary>
        /// Gets the status
        /// </summary>
        /// <value>The status</value>
        public string Status { get; }

        /// <summary>
        /// Gets the status comment
        /// </summary>
        /// <value>The status comment</value>
        public string StatusComment { get; }

        /// <summary>
        /// Gets the sprint
        /// </summary>
        /// <value>The sprint</value>
        public decimal? SprintDecimal { get; }

        /// <summary>
        /// Gets the sprint ranking
        /// </summary>
        /// <value>The sprint ranking</value>
        public int? SprintRanking { get; }

        /// <summary>
        /// Gets the climber
        /// </summary>
        /// <value>The climber</value>
        public decimal? ClimberDecimal { get; }

        /// <summary>
        /// Gets the climber ranking
        /// </summary>
        /// <value>The climber ranking</value>
        public int? ClimberRanking { get; }

        /// <summary>
        /// Gets the match status
        /// </summary>
        /// <value>The match status</value>
        public int MatchStatus { get; }

        /// <summary>
        /// Gets the home score
        /// </summary>
        /// <value>The home score</value>
        public decimal? HomeScore { get; }

        /// <summary>
        /// Gets the away score
        /// </summary>
        /// <value>The away score</value>
        public decimal? AwayScore { get; }

        /// <summary>
        /// Gets the grid
        /// </summary>
        /// <value>The grid</value>
        public int? Grid { get; }

        /// <summary>
        /// Gets the distance
        /// </summary>
        /// <value>The distance</value>
        public double? Distance { get; }

        /// <summary>
        /// Gets the competitor results
        /// </summary>
        /// <value>The results</value>
        /// <remarks>Sportradar</remarks>
        public IEnumerable<CompetitorResultDto> CompetitorResults { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventResultDto"/> class
        /// </summary>
        /// <param name="result">The result</param>
        public EventResultDto(resultType result)
        {
            HomeScore = result.home_score;
            AwayScore = result.away_score;
            MatchStatus = result.match_status_code;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventResultDto"/> class
        /// </summary>
        /// <param name="result">The result</param>
        public EventResultDto(resultScore result)
        {
            if (decimal.TryParse(result.home_score, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out var homeScore))
            {
                HomeScore = homeScore;
            }
            else if (!string.IsNullOrEmpty(result.home_score))
            {
                SdkInfo.ExecutionLog.LogWarning("EventResult - can not parse home score: {ResultHomeScore}", result.home_score);
            }
            if (decimal.TryParse(result.away_score, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out var awayScore))
            {
                AwayScore = awayScore;
            }
            else if (!string.IsNullOrEmpty(result.away_score))
            {
                SdkInfo.ExecutionLog.LogWarning("EventResult - can not parse away score: {ResultAwayScore}", result.away_score);
            }
            MatchStatus = result.match_status_code;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventResultDto"/> class
        /// </summary>
        /// <param name="stageResultCompetitor">The stage result competitor</param>
        public EventResultDto(stageResultCompetitor stageResultCompetitor)
        {
            Guard.Argument(stageResultCompetitor, nameof(stageResultCompetitor)).NotNull();

            Id = stageResultCompetitor.id;
            Position = stageResultCompetitor.positionSpecified ? stageResultCompetitor.position : (int?)null;
            PointsDecimal = stageResultCompetitor.pointsSpecified ? (decimal)stageResultCompetitor.points : (decimal?)null;
            WcPoints = stageResultCompetitor.wc_pointsSpecified ? (decimal?)stageResultCompetitor.wc_points : null;
            Time = stageResultCompetitor.time;
            TimeRanking = stageResultCompetitor.time_rankingSpecified ? stageResultCompetitor.time_ranking : (int?)null;
            Status = stageResultCompetitor.status;
            StatusComment = stageResultCompetitor.status_comment;
            SprintDecimal = stageResultCompetitor.sprintSpecified ? (decimal)stageResultCompetitor.sprint : (decimal?)null;
            SprintRanking = stageResultCompetitor.sprint_rankingSpecified ? stageResultCompetitor.sprint_ranking : (int?)null;
            ClimberDecimal = stageResultCompetitor.climberSpecified ? (decimal)stageResultCompetitor.climber : (decimal?)null;
            ClimberRanking = stageResultCompetitor.climber_rankingSpecified ? stageResultCompetitor.climber_ranking : (int?)null;
            Grid = stageResultCompetitor.gridSpecified ? stageResultCompetitor.grid : (int?)null;
            Distance = stageResultCompetitor.distanceSpecified ? stageResultCompetitor.distance : (double?)null;
            if (!stageResultCompetitor.result.IsNullOrEmpty())
            {
                CompetitorResults = stageResultCompetitor.result.Select(s => new CompetitorResultDto(s));
            }
        }
    }
}
