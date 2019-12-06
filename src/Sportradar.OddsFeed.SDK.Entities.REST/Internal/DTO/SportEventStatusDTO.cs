/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Messages.REST;

// ReSharper disable InconsistentNaming

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representation for sport event status (primarily received via feed message)
    /// </summary>
    public class SportEventStatusDTO
    {
        private const string THROW_PROPERTY = "Throw";
        private const string TRY_PROPERTY = "Try";
        private const string AWAY_BATTER_PROPERTY = "AwayBatter";
        private const string AWAY_DISMISSALS_PROPERTY = "AwayDismissals";
        private const string AWAY_GAME_SCORE_PROPERTY = "AwayGameScore";
        private const string AWAY_LEG_SCORE_PROPERTY = "AwayLegScore";
        private const string AWAY_PENALTY_RUNS_PROPERTY = "AwayPenaltyRuns";
        private const string AWAY_REMAINING_BOWLS_PROPERTY = "AwayRemainingBowls";
        private const string AWAY_SUSPEND_PROPERTY = "AwaySuspend";
        private const string HOME_BATTER_PROPERTY = "HomeBatter";
        private const string HOME_DISMISSALS_PROPERTY = "HomeDismissals";
        private const string HOME_GAME_SCORE_PROPERTY = "HomeGameScore";
        private const string HOME_LEG_SCORE_PROPERTY = "HomeLegScore";
        private const string HOME_PENALTY_RUNS_PROPERTY = "HomePenaltyRuns";
        private const string HOME_REMAINING_BOWLS_PROPERTY = "HomeRemainingBowls";
        private const string HOME_SUSPEND_PROPERTY = "HomeSuspend";
        private const string BALLS_PROPERTY = "Balls";
        private const string BASES_PROPERTY = "Bases";
        private const string MATCH_STATUS = "MatchStatus";

        /// <summary>
        /// Gets a <see cref="EventStatus"/> describing the high-level status of the associated sport event
        /// </summary>
        public EventStatus Status { get; }

        /// <summary>
        /// Gets a value indicating whether a data journalist is present on the associated sport event, or a
        /// null reference if the information is not available
        /// </summary>
        public int? IsReported { get; }

        /// <summary>
        /// Gets the score of the home competitor competing on the associated sport event
        /// </summary>
        public decimal? HomeScore { get; }

        /// <summary>
        /// Gets the score of the away competitor competing on the associated sport event
        /// </summary>
        public decimal? AwayScore { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing additional event status values
        /// </summary>
        /// <value>a <see cref="IReadOnlyDictionary{String, Object}"/> containing additional event status values</value>
        public IReadOnlyDictionary<string, object> Properties { get; }

        /// <summary>
        /// Gets the match status for specific locale
        /// </summary>
        public int MatchStatusId { get; }

        /// <summary>
        /// Gets the winner identifier
        /// </summary>
        /// <value>The winner identifier</value>
        public URN WinnerId { get; }

        /// <summary>
        /// Gets the reporting status
        /// </summary>
        /// <value>The reporting status</value>
        public ReportingStatus ReportingStatus { get; }

        /// <summary>
        /// Gets the period scores
        /// </summary>
        public IEnumerable<PeriodScoreDTO> PeriodScores { get; }

        /// <summary>
        /// Gets the event clock
        /// </summary>
        /// <value>The event clock</value>
        public EventClockDTO EventClock { get; }

        /// <summary>
        /// Gets the event results
        /// </summary>
        /// <value>The event results</value>
        public IEnumerable<EventResultDTO> EventResults { get; }

        /// <summary>
        /// Gets the sport event statistics
        /// </summary>
        /// <value>The sport event statistics</value>
        public SportEventStatisticsDTO SportEventStatistics { get; internal set; }

        /// <summary>
        /// Gets the indicator for competitors if there are home or away
        /// </summary>
        /// <value>The indicator for competitors if there are home or away</value>
        public IDictionary<HomeAway, URN> _homeAwayCompetitors { get; }

        /// <summary>
        /// Gets the penalty score of the home competitor competing on the associated sport event (for Ice Hockey)
        /// </summary>
        public int? HomePenaltyScore { get; }

        /// <summary>
        /// Gets the penalty score of the away competitor competing on the associated sport event (for Ice Hockey)
        /// </summary>
        public int? AwayPenaltyScore { get; }

        /// <summary>
        /// Gets the indicator wither the event is decided by fed
        /// </summary>
        public bool? DecidedByFed { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventStatusDTO"/> class
        /// </summary>
        /// <param name="record">A <see cref="restSportEventStatus" /> instance containing status data about the associated sport event</param>
        /// <param name="homeAwayCompetitors">The list of competitors with the indicator if it is a home or away team</param>
        public SportEventStatusDTO(sportEventStatus record, IDictionary<HomeAway, URN> homeAwayCompetitors)
        {
            Guard.Argument(record).NotNull();

            _homeAwayCompetitors = homeAwayCompetitors;

            var tempProperties = new Dictionary<string, object>();

            ApplyPropertyValue(record.throwSpecified, THROW_PROPERTY, record.@throw, tempProperties);
            ApplyPropertyValue(record.trySpecified, TRY_PROPERTY, record.@try, tempProperties);
            ApplyPropertyValue(record.away_batterSpecified, AWAY_BATTER_PROPERTY, record.away_batter, tempProperties);
            ApplyPropertyValue(record.away_dismissalsSpecified, AWAY_DISMISSALS_PROPERTY, record.away_dismissals, tempProperties);
            ApplyPropertyValue(record.away_gamescoreSpecified, AWAY_GAME_SCORE_PROPERTY, record.away_gamescore, tempProperties);
            ApplyPropertyValue(record.away_legscoreSpecified, AWAY_LEG_SCORE_PROPERTY, record.away_legscore, tempProperties);
            ApplyPropertyValue(record.away_penalty_runsSpecified, AWAY_PENALTY_RUNS_PROPERTY, record.away_penalty_runs, tempProperties);
            ApplyPropertyValue(record.away_remaining_bowlsSpecified, AWAY_REMAINING_BOWLS_PROPERTY, record.away_remaining_bowls, tempProperties);
            ApplyPropertyValue(record.away_suspendSpecified, AWAY_SUSPEND_PROPERTY, record.away_suspend, tempProperties);
            ApplyPropertyValue(record.away_scoreSpecified, "AwayScore", record.away_score, tempProperties); // BELOW
            ApplyPropertyValue(record.ballsSpecified, BALLS_PROPERTY, record.balls, tempProperties);
            ApplyPropertyValue(!string.IsNullOrEmpty(record.bases), BASES_PROPERTY, record.bases, tempProperties);
            ApplyPropertyValue(record.current_ct_teamSpecified, "CurrentCtTeam", record.current_ct_team, tempProperties);
            ApplyPropertyValue(record.current_endSpecified, "CurrentEnd", record.current_end, tempProperties);
            ApplyPropertyValue(record.current_serverSpecified, "CurrentServer", record.current_server, tempProperties);
            ApplyPropertyValue(record.deliverySpecified, "Delivery", record.delivery, tempProperties);
            ApplyPropertyValue(record.expedite_modeSpecified, "ExpediteMode", record.expedite_mode, tempProperties);
            ApplyPropertyValue(record.home_batterSpecified, HOME_BATTER_PROPERTY, record.home_batter, tempProperties);
            ApplyPropertyValue(record.home_dismissalsSpecified, HOME_DISMISSALS_PROPERTY, record.home_dismissals, tempProperties);
            ApplyPropertyValue(record.home_gamescoreSpecified, HOME_GAME_SCORE_PROPERTY, record.home_gamescore, tempProperties);
            ApplyPropertyValue(record.home_legscoreSpecified, HOME_LEG_SCORE_PROPERTY, record.home_legscore, tempProperties);
            ApplyPropertyValue(record.home_penalty_runsSpecified, HOME_PENALTY_RUNS_PROPERTY, record.home_penalty_runs, tempProperties);
            ApplyPropertyValue(record.home_remaining_bowlsSpecified, HOME_REMAINING_BOWLS_PROPERTY, record.home_remaining_bowls, tempProperties);
            ApplyPropertyValue(record.home_suspendSpecified, HOME_SUSPEND_PROPERTY, record.home_suspend, tempProperties);
            ApplyPropertyValue(record.home_scoreSpecified, "HomeScore", record.home_score, tempProperties); // BELOW
            ApplyPropertyValue(record.inningsSpecified, "Innings", record.innings, tempProperties);
            ApplyPropertyValue(true, MATCH_STATUS, record.match_status, tempProperties); //BELOW
            ApplyPropertyValue(record.outsSpecified, "Outs", record.outs, tempProperties);
            ApplyPropertyValue(record.overSpecified, "Over", record.over, tempProperties);
            ApplyPropertyValue(record.positionSpecified, "Position", record.position, tempProperties);
            ApplyPropertyValue(record.possessionSpecified, "Possession", record.possession, tempProperties);
            ApplyPropertyValue(record.remaining_redsSpecified, "RemainingReds", record.remaining_reds, tempProperties);
            ApplyPropertyValue(record.reportingSpecified, "Reporting", record.reporting, tempProperties); // BELOW
            ApplyPropertyValue(true, "Status", record.status, tempProperties);    //BELOW
            ApplyPropertyValue(record.strikesSpecified, "Strikes", record.strikes, tempProperties);
            ApplyPropertyValue(record.tiebreakSpecified, "Tiebreak", record.tiebreak, tempProperties);
            ApplyPropertyValue(record.visitSpecified, "Visit", record.visit, tempProperties);
            ApplyPropertyValue(record.yardsSpecified, "Yards", record.yards, tempProperties);
            ApplyPropertyValue(record.home_penalty_scoreSpecified, "home_penalty_score", record.home_penalty_score, tempProperties);
            ApplyPropertyValue(record.away_penalty_scoreSpecified, "away_penalty_score", record.away_penalty_score, tempProperties);

            if (record.period_scores != null && record.period_scores.Any())
            {
                var periodScores = new List<PeriodScoreDTO>();
                foreach (var periodScore in record.period_scores)
                {
                    periodScores.Add(new PeriodScoreDTO(periodScore));
                    ApplyPropertyValue(true, $"PeriodScore{periodScore.number}_Number", periodScore.number, tempProperties);
                    ApplyPropertyValue(true, $"PeriodScore{periodScore.number}_HomeScore", periodScore.home_score, tempProperties);
                    ApplyPropertyValue(true, $"PeriodScore{periodScore.number}_AwayScore", periodScore.away_score, tempProperties);
                    ApplyPropertyValue(true, $"PeriodScore{periodScore.number}_MatchStatusCode", periodScore.match_status_code, tempProperties);
                }
                PeriodScores = periodScores;
            }

            if (record.clock != null)
            {
                EventClock = new EventClockDTO(record.clock.match_time,
                                               record.clock.stoppage_time,
                                               record.clock.stoppage_time_announced,
                                               record.clock.remaining_time,
                                               record.clock.remaining_time_in_period,
                                               record.clock.stoppedSpecified,
                                               record.clock.stopped);
                ApplyPropertyValue(true, "Clock_MatchTime", record.clock.match_time, tempProperties);
                ApplyPropertyValue(true, "Clock_RemainingTime", record.clock.remaining_time, tempProperties);
                ApplyPropertyValue(true, "Clock_RemainingTimeInPeriod", record.clock.remaining_time_in_period, tempProperties);
                ApplyPropertyValue(true, "Clock_StoppageTime", record.clock.stoppage_time, tempProperties);
                ApplyPropertyValue(true, "Clock_StoppageTimeAnnounced", record.clock.stoppage_time_announced, tempProperties);
                ApplyPropertyValue(record.clock.stoppedSpecified, "Clock_Stopped", record.clock.stopped, tempProperties);
            }

            if (record.results != null && record.results.Any())
            {
                var i = 0;
                foreach (var resultType in record.results)
                {
                    i++;
                    ApplyPropertyValue(true, $"Result{i}_HomeScore", resultType.home_score, tempProperties);
                    ApplyPropertyValue(true, $"Result{i}_AwayScore", resultType.away_score, tempProperties);
                    ApplyPropertyValue(true, $"Result{i}_MatchStatusCode", resultType.match_status_code, tempProperties);
                }
            }

            Properties = new ReadOnlyDictionary<string, object>(tempProperties);

            Status = MessageMapperHelper.GetEnumValue(record.status, EventStatus.Unknown);

            IsReported = record.reportingSpecified
                ? (int?) record.reporting
                : null;

            HomeScore = record.home_scoreSpecified
                ? (decimal?) record.home_score
                : null;

            AwayScore = record.away_scoreSpecified
                ? (decimal?) record.away_score
                : null;

            MatchStatusId = record.match_status;

            WinnerId = null;

            if (record.reportingSpecified)
            {
                ReportingStatus = MessageMapperHelper.GetEnumValue(record.reporting, ReportingStatus.Unknown);
            }

            if (record.results != null)
            {
                var eventResults = new List<EventResultDTO>();
                foreach (var result in record.results)
                {
                    eventResults.Add(new EventResultDTO(result));
                }
                EventResults = eventResults;
            }

            if (record.statistics != null)
            {
                SportEventStatistics = new SportEventStatisticsDTO(record.statistics);
            }

            if (record.home_penalty_scoreSpecified)
            {
                HomePenaltyScore = record.home_penalty_score;
            }
            if (record.away_penalty_scoreSpecified)
            {
                AwayPenaltyScore = record.away_penalty_score;
            }

            // load home and away penalty score from the penalty period score
            if (HomePenaltyScore == null && AwayPenaltyScore == null && PeriodScores != null && PeriodScores.Any())
            {
                try
                {
                    foreach (var periodScoreDTO in PeriodScores)
                    {
                        if (periodScoreDTO.Type.HasValue && periodScoreDTO.Type.Value == PeriodType.Penalties)
                        {
                            HomePenaltyScore = (int) periodScoreDTO.HomeScore;
                            AwayPenaltyScore = (int) periodScoreDTO.AwayScore;
                        }
                    }
                }
                catch (Exception)
                {
                    //ignored
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventStatusDTO"/> class.
        /// </summary>
        /// <param name="record">A <see cref="restSportEventStatus" /> instance containing status data about the associated sport event</param>
        /// <param name="statistics"></param>
        /// <param name="homeAwayCompetitors"></param>
        public SportEventStatusDTO(restSportEventStatus record, matchStatistics statistics, IDictionary<HomeAway, URN> homeAwayCompetitors)
        {
            Guard.Argument(record).NotNull();

            _homeAwayCompetitors = homeAwayCompetitors;

            var tempProperties = new Dictionary<string, object>();

            if (record.clock != null)
            {
                var i = 0;
                foreach (var clock in record.clock)
                {
                    i++;
                    ApplyPropertyValue(true, $"Clock{i}_MatchTime", clock.match_time, tempProperties);
                    ApplyPropertyValue(true, $"Clock{i}_StoppageTime", clock.stoppage_time, tempProperties);
                    ApplyPropertyValue(true, $"Clock{i}_StoppageTimeAnnounced", clock.stoppage_time_announced, tempProperties);

                }
            }
            ApplyPropertyValue(record.periodSpecified, "Period", record.period, tempProperties);
            if (record.period_scores != null && record.period_scores.Any())
            {
                var periodScores = new List<PeriodScoreDTO>();
                foreach (var periodScore in record.period_scores)
                {
                    periodScores.Add(new PeriodScoreDTO(periodScore));
                    if (periodScore.numberSpecified)
                    {
                        ApplyPropertyValue(periodScore.numberSpecified, $"PeriodScore{periodScore.number}_Number", periodScore.number, tempProperties);
                    }
                    ApplyPropertyValue(true, $"PeriodScore{periodScore.number}_HomeScore", periodScore.home_score, tempProperties);
                    ApplyPropertyValue(true, $"PeriodScore{periodScore.number}_AwayScore", periodScore.away_score, tempProperties);
                    ApplyPropertyValue(true, $"PeriodScore{periodScore.number}_Type", periodScore.type, tempProperties);
                }
                PeriodScores = periodScores;
            }
            ApplyPropertyValue(true, "WinnerId", record.winner_id, tempProperties);
            ApplyPropertyValue(true, "WinningReason", record.winning_reason, tempProperties);
            ApplyPropertyValue(record.aggregate_away_scoreSpecified, "AggregateAwayScore", record.aggregate_away_score, tempProperties);
            ApplyPropertyValue(record.aggregate_home_scoreSpecified, "AggregateHomeScore", record.aggregate_home_score, tempProperties);
            ApplyPropertyValue(true, "AggregateWinnerId", record.aggregate_winner_id, tempProperties);
            ApplyPropertyValue(record.away_scoreSpecified, "AwayScore", record.away_score, tempProperties); // BELOW
            ApplyPropertyValue(record.home_scoreSpecified, "HomeScore", record.home_score, tempProperties); // BELOW

            Properties = new ReadOnlyDictionary<string, object>(tempProperties);

            int statusId;
            Status = record.status_codeSpecified
                         ? MessageMapperHelper.GetEnumValue(record.status_code, EventStatus.Unknown)
                         : int.TryParse(record.status, out statusId)
                             ? MessageMapperHelper.GetEnumValue(statusId, EventStatus.Unknown)
                             : MessageMapperHelper.GetEnumValue(record.status, EventStatus.Unknown);
            ApplyPropertyValue(true, "Status", (int) Status, tempProperties);    //BELOW

            //IsReported = record.reportingSpecified
            //    ? (int?)record.reporting
            //    : null;

            HomeScore = record.home_scoreSpecified
                ? (decimal?)record.home_score
                : null;

            AwayScore = record.away_scoreSpecified
                ? (decimal?)record.away_score
                : null;

            var matchStatusId = -1;
            if (record.match_status_codeSpecified)
            {
                matchStatusId = record.match_status_code;
                ApplyPropertyValue(true, MATCH_STATUS, matchStatusId, tempProperties);
            }
            if (matchStatusId < 0 && !string.IsNullOrEmpty(record.match_status))
            {
                //TODO: status here is received as "2nd_set", not even like descriptions on API (there are no "_" between words)
                if (int.TryParse(record.match_status, out matchStatusId))
                {
                    ApplyPropertyValue(true, MATCH_STATUS, matchStatusId, tempProperties);
                }
                else if (record.match_status.Equals("not_started", StringComparison.InvariantCultureIgnoreCase))
                {
                    matchStatusId = 0;
                    ApplyPropertyValue(true, MATCH_STATUS, matchStatusId, tempProperties);
                }
                else
                {
                    //ignored
                }
            }
            MatchStatusId = matchStatusId;

            if (!string.IsNullOrEmpty(record.winner_id))
            {
                WinnerId = URN.Parse(record.winner_id);
            }

            ReportingStatus = ReportingStatus.Unknown;

            EventClock = null;

            EventResults = null;

            if (statistics != null)
            {
                SportEventStatistics = new SportEventStatisticsDTO(statistics, _homeAwayCompetitors);
            }

            DecidedByFed = record.decided_by_fedSpecified ? record.decided_by_fed : (bool?) null;

            // load home and away penalty score from the penalty period score
            if (HomePenaltyScore == null && AwayPenaltyScore == null && PeriodScores != null && PeriodScores.Any())
            {
                try
                {
                    foreach (var periodScoreDTO in PeriodScores)
                    {
                        if (periodScoreDTO.Type.HasValue && periodScoreDTO.Type.Value == PeriodType.Penalties)
                        {
                            HomePenaltyScore = (int) periodScoreDTO.HomeScore;
                            AwayPenaltyScore = (int) periodScoreDTO.AwayScore;
                        }
                    }
                }
                catch (Exception)
                {
                    //ignored
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventStatusDTO"/> class.
        /// </summary>
        /// <param name="record">A <see cref="restSportEventStatus" /> instance containing status data about the associated sport event</param>
        public SportEventStatusDTO(stageSportEventStatus record)
        {
            Guard.Argument(record).NotNull();

            var tempProperties = new Dictionary<string, object>(0);

            if (record.results != null && record.results.Any())
            {
                var i = 0;
                foreach (var resultType in record.results)
                {
                    i++;
                    ApplyPropertyValue(true, $"Result{i}_Climber", resultType.climber, tempProperties);
                    ApplyPropertyValue(true, $"Result{i}_ClimberRanking", resultType.climber_ranking, tempProperties);
                    ApplyPropertyValue(true, $"Result{i}_Id", resultType.id, tempProperties);
                    ApplyPropertyValue(true, $"Result{i}_Points", resultType.points, tempProperties);
                    ApplyPropertyValue(true, $"Result{i}_Sprint", resultType.sprint, tempProperties);
                    ApplyPropertyValue(true, $"Result{i}_SprintRanking", resultType.sprint_ranking, tempProperties);
                    ApplyPropertyValue(true, $"Result{i}_Status", resultType.status, tempProperties);
                    ApplyPropertyValue(true, $"Result{i}_StatusComment", resultType.status_comment, tempProperties);
                    ApplyPropertyValue(true, $"Result{i}_Time", resultType.time, tempProperties);
                    ApplyPropertyValue(true, $"Result{i}_TimeRanking", resultType.time_ranking, tempProperties);
                }
            }
            ApplyPropertyValue(true, "WinnerId", record.winner_id, tempProperties);

            Properties = new ReadOnlyDictionary<string, object>(tempProperties);

            Status = MessageMapperHelper.GetEnumValue(record.status, EventStatus.Unknown);
            ApplyPropertyValue(true, "Status", (int) Status, tempProperties);

            MatchStatusId = -1;

            WinnerId = !string.IsNullOrEmpty(record.winner_id) ? URN.Parse(record.winner_id) : null;

            ReportingStatus = ReportingStatus.Unknown;

            PeriodScores = null;

            EventClock = null;

            var eventResults = new List<EventResultDTO>();
            if (record.results != null && record.results.Any())
            {
                foreach (var stageResultCompetitor in record.results)
                {
                    eventResults.Add(new EventResultDTO(stageResultCompetitor));
                }

                EventResults = eventResults;
            }

            SportEventStatistics = null;
        }

        /// <summary>
        /// Gets the value of the property specified by it's name
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>A <see cref="object"/> representation of the value of the specified property, or a null reference
        /// if the value of the specified property was not specified</returns>
        public object GetPropertyValue(string propertyName)
        {
            return Properties[propertyName];
        }

        /// <summary>
        /// Adds the provided <code>name</code> and <code>value</code> to the provided <see cref="IDictionary{String, Object}"/>
        /// if so specified by the <code>specified</code>"/> field
        /// </summary>
        /// <param name="specified">Specifies whether the provided name and value should be added to the dictionary</param>
        /// <param name="name">The name of the property</param>
        /// <param name="value">The value of the property</param>
        /// <param name="target">A <see cref="IDictionary{String, Object}"/>to which the name / value pair will be added</param>
        private static void ApplyPropertyValue(bool specified, string name, object value, IDictionary<string, object> target)
        {
            if (target == null)
            {
                target = new Dictionary<string, object>();
            }

            if (specified)
            {
                target[name] = value;
            }
            else
            {
                //it this ok?
                target.Remove(name);
            }
        }

        //TODO: what to use for final score: aggregate or normal score; should we remove empty values? to reflect last status received
    }
}
