/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Class EventResult
    /// </summary>
    /// <seealso cref="IEventResult" />
    internal class EventResult : IEventResult
    {
        private readonly ILocalizedNamedValueCache _matchStatusesCache;
        private readonly int _matchStatusCode;

        /// <summary>
        /// Gets the id of the event result
        /// </summary>
        /// <value>The id of the event result</value>
        public string Id { get; }

        /// <summary>
        /// Gets the position of the result
        /// </summary>
        /// <value>The position of the result</value>
        public int? Position { get; }

        /// <summary>
        /// Gets the points
        /// </summary>
        /// <value>The points</value>
        public decimal? PointsDecimal { get; }

        /// <summary>
        /// Gets the WC points
        /// </summary>
        /// <value>The WC points</value>
        public decimal? WcPoints { get; }

        /// <summary>
        /// Gets the time of the result
        /// </summary>
        /// <value>The time of the result</value>
        public string Time { get; }

        /// <summary>
        /// Gets the time ranking
        /// </summary>
        /// <value>The time ranking</value>
        public int? TimeRanking { get; }

        /// <summary>
        /// Gets the status of the result
        /// </summary>
        /// <value>The status of the result</value>
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
        /// Gets the score of the home competitor competing on the associated sport event
        /// </summary>
        /// <value>The home score.</value>
        public decimal? HomeScore { get; }

        /// <summary>
        /// Gets the score of the away competitor competing on the associated sport event
        /// </summary>
        /// <value>The away score.</value>
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
        /// Gets the results.
        /// </summary>
        /// <value>The results.</value>
        public IEnumerable<ICompetitorResult> CompetitorResults { get; }

        /// <summary>
        /// Asynchronously gets the match status
        /// </summary>
        /// <param name="culture">The culture used to get match status id and description</param>
        /// <returns>Returns the match status id and description in selected culture</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ILocalizedNamedValue> GetMatchStatusAsync(CultureInfo culture)
        {
            return _matchStatusesCache == null
                ? null
                : await _matchStatusesCache.GetAsync(_matchStatusCode, new List<CultureInfo> { culture }).ConfigureAwait(false);
        }

        internal EventResult(EventResultDTO dto, ILocalizedNamedValueCache matchStatusesCache)
        {
            _matchStatusesCache = matchStatusesCache;

            Id = dto.Id;
            Position = dto.Position;
            PointsDecimal = dto.PointsDecimal;
            WcPoints = dto.WcPoints;
            Time = dto.Time;
            TimeRanking = dto.TimeRanking;
            Status = dto.Status;
            StatusComment = dto.StatusComment;
            SprintDecimal = dto.SprintDecimal;
            SprintRanking = dto.SprintRanking;
            ClimberDecimal = dto.ClimberDecimal;
            ClimberRanking = dto.ClimberRanking;
            _matchStatusCode = dto.MatchStatus;
            HomeScore = dto.HomeScore;
            AwayScore = dto.AwayScore;
            Grid = dto.Grid;
            Distance = dto.Distance;
            CompetitorResults = dto.CompetitorResults?.Select(s => new CompetitorResult(s)).ToList() ?? new List<CompetitorResult>();
        }
    }
}
