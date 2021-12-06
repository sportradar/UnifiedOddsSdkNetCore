/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
    internal class SportEventStatusDTO
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
        /// Gets the period of ladder.
        /// </summary>
        /// <value>The period of ladder</value>
        public int? PeriodOfLadder { get; }

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
        /// <param name="ses">A <see cref="restSportEventStatus" /> instance containing status data about the associated sport event</param>
        /// <param name="homeAwayCompetitors">The list of competitors with the indicator if it is a home or away team</param>
        public SportEventStatusDTO(sportEventStatus ses, IDictionary<HomeAway, URN> homeAwayCompetitors)
        {
            Guard.Argument(ses, nameof(ses)).NotNull();

            _homeAwayCompetitors = homeAwayCompetitors;

            var tempProperties = new Dictionary<string, object>();

            ApplyPropertyValue(ses.throwSpecified, THROW_PROPERTY, ses.@throw, tempProperties);
            ApplyPropertyValue(ses.trySpecified, TRY_PROPERTY, ses.@try, tempProperties);
            ApplyPropertyValue(ses.away_batterSpecified, AWAY_BATTER_PROPERTY, ses.away_batter, tempProperties);
            ApplyPropertyValue(ses.away_dismissalsSpecified, AWAY_DISMISSALS_PROPERTY, ses.away_dismissals, tempProperties);
            ApplyPropertyValue(ses.away_gamescoreSpecified, AWAY_GAME_SCORE_PROPERTY, ses.away_gamescore, tempProperties);
            ApplyPropertyValue(ses.away_legscoreSpecified, AWAY_LEG_SCORE_PROPERTY, ses.away_legscore, tempProperties);
            ApplyPropertyValue(ses.away_penalty_runsSpecified, AWAY_PENALTY_RUNS_PROPERTY, ses.away_penalty_runs, tempProperties);
            ApplyPropertyValue(ses.away_remaining_bowlsSpecified, AWAY_REMAINING_BOWLS_PROPERTY, ses.away_remaining_bowls, tempProperties);
            ApplyPropertyValue(ses.away_suspendSpecified, AWAY_SUSPEND_PROPERTY, ses.away_suspend, tempProperties);
            ApplyPropertyValue(ses.away_scoreSpecified, "AwayScore", ses.away_score, tempProperties); // BELOW
            ApplyPropertyValue(ses.ballsSpecified, BALLS_PROPERTY, ses.balls, tempProperties);
            ApplyPropertyValue(!string.IsNullOrEmpty(ses.bases), BASES_PROPERTY, ses.bases, tempProperties);
            ApplyPropertyValue(ses.current_ct_teamSpecified, "CurrentCtTeam", ses.current_ct_team, tempProperties);
            ApplyPropertyValue(ses.current_endSpecified, "CurrentEnd", ses.current_end, tempProperties);
            ApplyPropertyValue(ses.current_serverSpecified, "CurrentServer", ses.current_server, tempProperties);
            ApplyPropertyValue(ses.deliverySpecified, "Delivery", ses.delivery, tempProperties);
            ApplyPropertyValue(ses.expedite_modeSpecified, "ExpediteMode", ses.expedite_mode, tempProperties);
            ApplyPropertyValue(ses.home_batterSpecified, HOME_BATTER_PROPERTY, ses.home_batter, tempProperties);
            ApplyPropertyValue(ses.home_dismissalsSpecified, HOME_DISMISSALS_PROPERTY, ses.home_dismissals, tempProperties);
            ApplyPropertyValue(ses.home_gamescoreSpecified, HOME_GAME_SCORE_PROPERTY, ses.home_gamescore, tempProperties);
            ApplyPropertyValue(ses.home_legscoreSpecified, HOME_LEG_SCORE_PROPERTY, ses.home_legscore, tempProperties);
            ApplyPropertyValue(ses.home_penalty_runsSpecified, HOME_PENALTY_RUNS_PROPERTY, ses.home_penalty_runs, tempProperties);
            ApplyPropertyValue(ses.home_remaining_bowlsSpecified, HOME_REMAINING_BOWLS_PROPERTY, ses.home_remaining_bowls, tempProperties);
            ApplyPropertyValue(ses.home_suspendSpecified, HOME_SUSPEND_PROPERTY, ses.home_suspend, tempProperties);
            ApplyPropertyValue(ses.home_scoreSpecified, "HomeScore", ses.home_score, tempProperties); // BELOW
            ApplyPropertyValue(ses.inningsSpecified, "Innings", ses.innings, tempProperties);
            ApplyPropertyValue(true, MATCH_STATUS, ses.match_status, tempProperties); //BELOW
            ApplyPropertyValue(ses.outsSpecified, "Outs", ses.outs, tempProperties);
            ApplyPropertyValue(ses.overSpecified, "Over", ses.over, tempProperties);
            ApplyPropertyValue(ses.positionSpecified, "Position", ses.position, tempProperties);
            ApplyPropertyValue(ses.possessionSpecified, "Possession", ses.possession, tempProperties);
            ApplyPropertyValue(ses.remaining_redsSpecified, "RemainingReds", ses.remaining_reds, tempProperties);
            ApplyPropertyValue(ses.reportingSpecified, "Reporting", ses.reporting, tempProperties); // BELOW
            ApplyPropertyValue(true, "Status", ses.status, tempProperties);    //BELOW
            ApplyPropertyValue(ses.strikesSpecified, "Strikes", ses.strikes, tempProperties);
            ApplyPropertyValue(ses.tiebreakSpecified, "Tiebreak", ses.tiebreak, tempProperties);
            ApplyPropertyValue(ses.visitSpecified, "Visit", ses.visit, tempProperties);
            ApplyPropertyValue(ses.yardsSpecified, "Yards", ses.yards, tempProperties);
            ApplyPropertyValue(ses.home_penalty_scoreSpecified, "home_penalty_score", ses.home_penalty_score, tempProperties);
            ApplyPropertyValue(ses.away_penalty_scoreSpecified, "away_penalty_score", ses.away_penalty_score, tempProperties);
            ApplyPropertyValue(ses.period_of_leaderSpecified, "period_of_leader", ses.period_of_leader, tempProperties);
            ApplyPropertyValue(!string.IsNullOrEmpty(ses.pitcher), "pitcher", ses.pitcher, tempProperties);
            ApplyPropertyValue(!string.IsNullOrEmpty(ses.batter), "batter", ses.batter, tempProperties);
            ApplyPropertyValue(ses.pitch_countSpecified, "pitch_count", ses.pitch_count, tempProperties);
            ApplyPropertyValue(ses.pitches_seenSpecified, "pitches_seen", ses.pitches_seen, tempProperties);
            ApplyPropertyValue(ses.total_hitsSpecified, "total_hits", ses.total_hits, tempProperties);
            ApplyPropertyValue(ses.total_pitchesSpecified, "total_pitches", ses.total_pitches, tempProperties);
            ApplyPropertyValue(ses.home_drive_countSpecified, "home_drive_count", ses.home_drive_count, tempProperties);
            ApplyPropertyValue(ses.home_play_countSpecified, "home_play_count", ses.home_play_count, tempProperties);
            ApplyPropertyValue(ses.away_drive_countSpecified, "away_drive_count", ses.away_drive_count, tempProperties);
            ApplyPropertyValue(ses.away_play_countSpecified, "away_play_count", ses.away_play_count, tempProperties);

            if (ses.period_scores != null && ses.period_scores.Any())
            {
                var periodScores = new List<PeriodScoreDTO>();
                foreach (var periodScore in ses.period_scores)
                {
                    periodScores.Add(new PeriodScoreDTO(periodScore));
                    ApplyPropertyValue(true, $"PeriodScore{periodScore.number}_Number", periodScore.number, tempProperties);
                    ApplyPropertyValue(true, $"PeriodScore{periodScore.number}_HomeScore", periodScore.home_score, tempProperties);
                    ApplyPropertyValue(true, $"PeriodScore{periodScore.number}_AwayScore", periodScore.away_score, tempProperties);
                    ApplyPropertyValue(true, $"PeriodScore{periodScore.number}_MatchStatusCode", periodScore.match_status_code, tempProperties);
                }
                PeriodScores = periodScores;
            }

            if (ses.clock != null)
            {
                EventClock = new EventClockDTO(ses.clock.match_time,
                                               ses.clock.stoppage_time,
                                               ses.clock.stoppage_time_announced,
                                               ses.clock.remaining_time,
                                               ses.clock.remaining_time_in_period,
                                               ses.clock.stoppedSpecified,
                                               ses.clock.stopped);
                ApplyPropertyValue(true, "Clock_MatchTime", ses.clock.match_time, tempProperties);
                ApplyPropertyValue(true, "Clock_RemainingTime", ses.clock.remaining_time, tempProperties);
                ApplyPropertyValue(true, "Clock_RemainingTimeInPeriod", ses.clock.remaining_time_in_period, tempProperties);
                ApplyPropertyValue(true, "Clock_StoppageTime", ses.clock.stoppage_time, tempProperties);
                ApplyPropertyValue(true, "Clock_StoppageTimeAnnounced", ses.clock.stoppage_time_announced, tempProperties);
                ApplyPropertyValue(ses.clock.stoppedSpecified, "Clock_Stopped", ses.clock.stopped, tempProperties);
            }

            if (ses.results != null && ses.results.Any())
            {
                var i = 0;
                foreach (var resultType in ses.results)
                {
                    i++;
                    ApplyPropertyValue(true, $"Result{i}_HomeScore", resultType.home_score, tempProperties);
                    ApplyPropertyValue(true, $"Result{i}_AwayScore", resultType.away_score, tempProperties);
                    ApplyPropertyValue(true, $"Result{i}_MatchStatusCode", resultType.match_status_code, tempProperties);
                }
            }

            Properties = new ReadOnlyDictionary<string, object>(tempProperties);

            Status = MessageMapperHelper.GetEnumValue(ses.status, EventStatus.Unknown);

            IsReported = ses.reportingSpecified
                ? (int?) ses.reporting
                : null;

            HomeScore = ses.home_scoreSpecified
                ? (decimal?) ses.home_score
                : null;

            AwayScore = ses.away_scoreSpecified
                ? (decimal?) ses.away_score
                : null;

            MatchStatusId = ses.match_status;

            WinnerId = null;

            PeriodOfLadder = ses.period_of_leaderSpecified ? ses.period_of_leader : (int?)null;

            if (ses.reportingSpecified)
            {
                ReportingStatus = MessageMapperHelper.GetEnumValue(ses.reporting, ReportingStatus.Unknown);
            }

            if (ses.results != null)
            {
                var eventResults = new List<EventResultDTO>();
                foreach (var result in ses.results)
                {
                    eventResults.Add(new EventResultDTO(result));
                }
                EventResults = eventResults;
            }

            if (ses.statistics != null)
            {
                SportEventStatistics = new SportEventStatisticsDTO(ses.statistics);
            }

            if (ses.home_penalty_scoreSpecified)
            {
                HomePenaltyScore = ses.home_penalty_score;
            }
            if (ses.away_penalty_scoreSpecified)
            {
                AwayPenaltyScore = ses.away_penalty_score;
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
        /// <param name="restSES">A <see cref="restSportEventStatus" /> instance containing status data about the associated sport event</param>
        /// <param name="statistics"></param>
        /// <param name="homeAwayCompetitors"></param>
        public SportEventStatusDTO(restSportEventStatus restSES, matchStatistics statistics, IDictionary<HomeAway, URN> homeAwayCompetitors)
        {
            Guard.Argument(restSES, nameof(restSES)).NotNull();

            _homeAwayCompetitors = homeAwayCompetitors;

            var tempProperties = new Dictionary<string, object>();

            if (restSES.clock != null)
            {
                var i = 0;
                foreach (var clock in restSES.clock)
                {
                    i++;
                    ApplyPropertyValue(true, $"Clock{i}_MatchTime", clock.match_time, tempProperties);
                    ApplyPropertyValue(true, $"Clock{i}_StoppageTime", clock.stoppage_time, tempProperties);
                    ApplyPropertyValue(true, $"Clock{i}_StoppageTimeAnnounced", clock.stoppage_time_announced, tempProperties);
                }
            }
            ApplyPropertyValue(restSES.periodSpecified, "Period", restSES.period, tempProperties);
            if (restSES.period_scores != null && restSES.period_scores.Any())
            {
                var periodScores = new List<PeriodScoreDTO>();
                foreach (var periodScore in restSES.period_scores)
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
            ApplyPropertyValue(!string.IsNullOrEmpty(restSES.winner_id), "WinnerId", restSES.winner_id, tempProperties);
            ApplyPropertyValue(!string.IsNullOrEmpty(restSES.winning_reason), "WinningReason", restSES.winning_reason, tempProperties);
            ApplyPropertyValue(!string.IsNullOrEmpty(restSES.aggregate_home_score), "AggregateHomeScore", restSES.aggregate_home_score, tempProperties);
            ApplyPropertyValue(!string.IsNullOrEmpty(restSES.aggregate_away_score), "AggregateAwayScore", restSES.aggregate_away_score, tempProperties);
            ApplyPropertyValue(!string.IsNullOrEmpty(restSES.aggregate_winner_id), "AggregateWinnerId", restSES.aggregate_winner_id, tempProperties);
            ApplyPropertyValue(!string.IsNullOrEmpty(restSES.home_score), "HomeScore", restSES.home_score, tempProperties); // BELOW
            ApplyPropertyValue(!string.IsNullOrEmpty(restSES.away_score), "AwayScore", restSES.away_score, tempProperties); // BELOW

            Properties = new ReadOnlyDictionary<string, object>(tempProperties);

            int statusId;
            Status = restSES.status_codeSpecified
                         ? MessageMapperHelper.GetEnumValue(restSES.status_code, EventStatus.Unknown)
                         : int.TryParse(restSES.status, out statusId)
                             ? MessageMapperHelper.GetEnumValue(statusId, EventStatus.Unknown)
                             : MessageMapperHelper.GetEnumValue(restSES.status, EventStatus.Unknown);
            ApplyPropertyValue(true, "Status", (int) Status, tempProperties);    //BELOW

            HomeScore = decimal.TryParse(restSES.home_score, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out var homeScore)
                            ? (decimal?)homeScore
                            : null;

            AwayScore = decimal.TryParse(restSES.away_score, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out var awayScore)
                            ? (decimal?)awayScore
                            : null;

            var matchStatusId = -1;
            if (restSES.match_status_codeSpecified)
            {
                matchStatusId = restSES.match_status_code;
                ApplyPropertyValue(true, MATCH_STATUS, matchStatusId, tempProperties);
            }
            if (matchStatusId < 0 && !string.IsNullOrEmpty(restSES.match_status))
            {
                //TODO: status here is received as "2nd_set", not even like descriptions on API (there are no "_" between words)
                if (int.TryParse(restSES.match_status, out matchStatusId))
                {
                    ApplyPropertyValue(true, MATCH_STATUS, matchStatusId, tempProperties);
                }
                else if (restSES.match_status.Equals("not_started", StringComparison.InvariantCultureIgnoreCase))
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

            if (!string.IsNullOrEmpty(restSES.winner_id))
            {
                WinnerId = URN.Parse(restSES.winner_id);
            }

            PeriodOfLadder = null;

            ReportingStatus = ReportingStatus.Unknown;

            EventClock = null;

            EventResults = null;
            if (restSES.results != null)
            {
                var eventResults = new List<EventResultDTO>();
                foreach (var result in restSES.results)
                {
                    eventResults.Add(new EventResultDTO(result));
                }
                EventResults = eventResults;
            }

            if (statistics != null)
            {
                SportEventStatistics = new SportEventStatisticsDTO(statistics, _homeAwayCompetitors);
            }

            DecidedByFed = restSES.decided_by_fedSpecified ? restSES.decided_by_fed : (bool?) null;

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
        /// <param name="stageSES">A <see cref="restSportEventStatus" /> instance containing status data about the associated sport event</param>
        public SportEventStatusDTO(stageSportEventStatus stageSES)
        {
            Guard.Argument(stageSES, nameof(stageSES)).NotNull();

            var tempProperties = new Dictionary<string, object>();

            var eventResults = new List<EventResultDTO>();
            if (stageSES.results?.competitor != null && stageSES.results.competitor.Any())
            {
                var i = 0;
                foreach (var resultType in stageSES.results.competitor)
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

                    eventResults.Add(new EventResultDTO(resultType));
                }
                EventResults = eventResults;
            }

            ApplyPropertyValue(true, "WinnerId", stageSES.winner_id, tempProperties);
            ApplyPropertyValue(stageSES.period_of_leaderSpecified, "period_of_leader", stageSES.period_of_leader, tempProperties);

            Properties = new ReadOnlyDictionary<string, object>(tempProperties);

            Status = MessageMapperHelper.GetEnumValue(stageSES.status, EventStatus.Unknown);
            ApplyPropertyValue(true, "Status", (int) Status, tempProperties);

            MatchStatusId = -1;

            WinnerId = !string.IsNullOrEmpty(stageSES.winner_id) ? URN.Parse(stageSES.winner_id) : null;

            PeriodOfLadder = stageSES.period_of_leaderSpecified ? stageSES.period_of_leader : (int?)null;

            ReportingStatus = ReportingStatus.Unknown;

            PeriodScores = null;

            EventClock = null;

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
                //is this ok?
                target.Remove(name);
            }
        }

        //TODO: what to use for final score: aggregate or normal score; should we remove empty values? to reflect last status received
    }
}
