/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representation for sport event status (primarily received via feed message)
    /// </summary>
    internal class SportEventStatusDTO
    {
        private const string ThrowProperty = "Throw";
        private const string TryProperty = "Try";
        private const string AwayBatterProperty = "AwayBatter";
        private const string AwayDismissalsProperty = "AwayDismissals";
        private const string AwayGameScoreProperty = "AwayGameScore";
        private const string AwayLegScoreProperty = "AwayLegScore";
        private const string AwayPenaltyRunsProperty = "AwayPenaltyRuns";
        private const string AwayRemainingBowlsProperty = "AwayRemainingBowls";
        private const string AwaySuspendProperty = "AwaySuspend";
        private const string HomeBatterProperty = "HomeBatter";
        private const string HomeDismissalsProperty = "HomeDismissals";
        private const string HomeGameScoreProperty = "HomeGameScore";
        private const string HomeLegScoreProperty = "HomeLegScore";
        private const string HomePenaltyRunsProperty = "HomePenaltyRuns";
        private const string HomeRemainingBowlsProperty = "HomeRemainingBowls";
        private const string HomeSuspendProperty = "HomeSuspend";
        private const string BallsProperty = "Balls";
        private const string BasesProperty = "Bases";
        private const string MatchStatus = "MatchStatus";

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
        public IDictionary<HomeAway, URN> HomeAwayCompetitors { get; }

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

            HomeAwayCompetitors = homeAwayCompetitors;

            var tempProperties = new Dictionary<string, object>();

            ApplyPropertyValue(ses.throwSpecified, ThrowProperty, ses.@throw, tempProperties);
            ApplyPropertyValue(ses.trySpecified, TryProperty, ses.@try, tempProperties);
            ApplyPropertyValue(ses.away_batterSpecified, AwayBatterProperty, ses.away_batter, tempProperties);
            ApplyPropertyValue(ses.away_dismissalsSpecified, AwayDismissalsProperty, ses.away_dismissals, tempProperties);
            ApplyPropertyValue(ses.away_gamescoreSpecified, AwayGameScoreProperty, ses.away_gamescore, tempProperties);
            ApplyPropertyValue(ses.away_legscoreSpecified, AwayLegScoreProperty, ses.away_legscore, tempProperties);
            ApplyPropertyValue(ses.away_penalty_runsSpecified, AwayPenaltyRunsProperty, ses.away_penalty_runs, tempProperties);
            ApplyPropertyValue(ses.away_remaining_bowlsSpecified, AwayRemainingBowlsProperty, ses.away_remaining_bowls, tempProperties);
            ApplyPropertyValue(ses.away_suspendSpecified, AwaySuspendProperty, ses.away_suspend, tempProperties);
            ApplyPropertyValue(ses.away_scoreSpecified, "AwayScore", ses.away_score, tempProperties); // BELOW
            ApplyPropertyValue(ses.ballsSpecified, BallsProperty, ses.balls, tempProperties);
            ApplyPropertyValue(!string.IsNullOrEmpty(ses.bases), BasesProperty, ses.bases, tempProperties);
            ApplyPropertyValue(ses.current_ct_teamSpecified, "CurrentCtTeam", ses.current_ct_team, tempProperties);
            ApplyPropertyValue(ses.current_endSpecified, "CurrentEnd", ses.current_end, tempProperties);
            ApplyPropertyValue(ses.current_serverSpecified, "CurrentServer", ses.current_server, tempProperties);
            ApplyPropertyValue(ses.deliverySpecified, "Delivery", ses.delivery, tempProperties);
            ApplyPropertyValue(ses.expedite_modeSpecified, "ExpediteMode", ses.expedite_mode, tempProperties);
            ApplyPropertyValue(ses.home_batterSpecified, HomeBatterProperty, ses.home_batter, tempProperties);
            ApplyPropertyValue(ses.home_dismissalsSpecified, HomeDismissalsProperty, ses.home_dismissals, tempProperties);
            ApplyPropertyValue(ses.home_gamescoreSpecified, HomeGameScoreProperty, ses.home_gamescore, tempProperties);
            ApplyPropertyValue(ses.home_legscoreSpecified, HomeLegScoreProperty, ses.home_legscore, tempProperties);
            ApplyPropertyValue(ses.home_penalty_runsSpecified, HomePenaltyRunsProperty, ses.home_penalty_runs, tempProperties);
            ApplyPropertyValue(ses.home_remaining_bowlsSpecified, HomeRemainingBowlsProperty, ses.home_remaining_bowls, tempProperties);
            ApplyPropertyValue(ses.home_suspendSpecified, HomeSuspendProperty, ses.home_suspend, tempProperties);
            ApplyPropertyValue(ses.home_scoreSpecified, "HomeScore", ses.home_score, tempProperties); // BELOW
            ApplyPropertyValue(ses.inningsSpecified, "Innings", ses.innings, tempProperties);
            ApplyPropertyValue(true, MatchStatus, ses.match_status, tempProperties); //BELOW
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
                ? (int?)ses.reporting
                : null;

            HomeScore = ses.home_scoreSpecified
                ? (decimal?)ses.home_score
                : null;

            AwayScore = ses.away_scoreSpecified
                ? (decimal?)ses.away_score
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
                            HomePenaltyScore = (int)periodScoreDTO.HomeScore;
                            AwayPenaltyScore = (int)periodScoreDTO.AwayScore;
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
        /// <param name="restSes">A <see cref="restSportEventStatus" /> instance containing status data about the associated sport event</param>
        /// <param name="statistics"></param>
        /// <param name="homeAwayCompetitors"></param>
        public SportEventStatusDTO(restSportEventStatus restSes, matchStatistics statistics, IDictionary<HomeAway, URN> homeAwayCompetitors)
        {
            Guard.Argument(restSes, nameof(restSes)).NotNull();

            HomeAwayCompetitors = homeAwayCompetitors;

            var tempProperties = new Dictionary<string, object>();

            if (restSes.clock != null)
            {
                var i = 0;
                foreach (var clock in restSes.clock)
                {
                    i++;
                    ApplyPropertyValue(true, $"Clock{i}_MatchTime", clock.match_time, tempProperties);
                    ApplyPropertyValue(true, $"Clock{i}_StoppageTime", clock.stoppage_time, tempProperties);
                    ApplyPropertyValue(true, $"Clock{i}_StoppageTimeAnnounced", clock.stoppage_time_announced, tempProperties);
                }
            }
            ApplyPropertyValue(restSes.periodSpecified, "Period", restSes.period, tempProperties);
            if (restSes.period_scores != null && restSes.period_scores.Any())
            {
                var periodScores = new List<PeriodScoreDTO>();
                foreach (var periodScore in restSes.period_scores)
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
            ApplyPropertyValue(!string.IsNullOrEmpty(restSes.winner_id), "WinnerId", restSes.winner_id, tempProperties);
            ApplyPropertyValue(!string.IsNullOrEmpty(restSes.winning_reason), "WinningReason", restSes.winning_reason, tempProperties);
            ApplyPropertyValue(!string.IsNullOrEmpty(restSes.aggregate_home_score), "AggregateHomeScore", restSes.aggregate_home_score, tempProperties);
            ApplyPropertyValue(!string.IsNullOrEmpty(restSes.aggregate_away_score), "AggregateAwayScore", restSes.aggregate_away_score, tempProperties);
            ApplyPropertyValue(!string.IsNullOrEmpty(restSes.aggregate_winner_id), "AggregateWinnerId", restSes.aggregate_winner_id, tempProperties);
            ApplyPropertyValue(!string.IsNullOrEmpty(restSes.home_score), "HomeScore", restSes.home_score, tempProperties); // BELOW
            ApplyPropertyValue(!string.IsNullOrEmpty(restSes.away_score), "AwayScore", restSes.away_score, tempProperties); // BELOW

            Properties = new ReadOnlyDictionary<string, object>(tempProperties);

            if (restSes.status_codeSpecified)
            {
                Status = MessageMapperHelper.GetEnumValue(restSes.status_code, EventStatus.Unknown);
            }
            else
            {
                Status = int.TryParse(restSes.status, out var statusId)
                    ? MessageMapperHelper.GetEnumValue(statusId, EventStatus.Unknown)
                    : MessageMapperHelper.GetEnumValue(restSes.status, EventStatus.Unknown);
            }
            ApplyPropertyValue(true, "Status", (int)Status, tempProperties);    //BELOW

            HomeScore = decimal.TryParse(restSes.home_score, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out var homeScore)
                            ? (decimal?)homeScore
                            : null;

            AwayScore = decimal.TryParse(restSes.away_score, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out var awayScore)
                            ? (decimal?)awayScore
                            : null;

            var matchStatusId = -1;
            if (restSes.match_status_codeSpecified)
            {
                matchStatusId = restSes.match_status_code;
                ApplyPropertyValue(true, MatchStatus, matchStatusId, tempProperties);
            }
            if (matchStatusId < 0 && !string.IsNullOrEmpty(restSes.match_status))
            {
                //TODO: status here is received as "2nd_set", not even like descriptions on API (there are no "_" between words)
                if (int.TryParse(restSes.match_status, out matchStatusId))
                {
                    ApplyPropertyValue(true, MatchStatus, matchStatusId, tempProperties);
                }
                else if (restSes.match_status.Equals("not_started", StringComparison.InvariantCultureIgnoreCase))
                {
                    matchStatusId = 0;
                    ApplyPropertyValue(true, MatchStatus, matchStatusId, tempProperties);
                }
                else
                {
                    //ignored
                }
            }
            MatchStatusId = matchStatusId;

            if (!string.IsNullOrEmpty(restSes.winner_id))
            {
                WinnerId = URN.Parse(restSes.winner_id);
            }

            PeriodOfLadder = null;

            ReportingStatus = ReportingStatus.Unknown;

            EventClock = null;

            EventResults = null;
            if (restSes.results != null)
            {
                var eventResults = new List<EventResultDTO>();
                foreach (var result in restSes.results)
                {
                    eventResults.Add(new EventResultDTO(result));
                }
                EventResults = eventResults;
            }

            if (statistics != null)
            {
                SportEventStatistics = new SportEventStatisticsDTO(statistics, HomeAwayCompetitors);
            }

            DecidedByFed = restSes.decided_by_fedSpecified ? restSes.decided_by_fed : (bool?)null;

            // load home and away penalty score from the penalty period score
            if (HomePenaltyScore == null && AwayPenaltyScore == null && PeriodScores != null && PeriodScores.Any())
            {
                try
                {
                    foreach (var periodScoreDTO in PeriodScores)
                    {
                        if (periodScoreDTO.Type.HasValue && periodScoreDTO.Type.Value == PeriodType.Penalties)
                        {
                            HomePenaltyScore = (int)periodScoreDTO.HomeScore;
                            AwayPenaltyScore = (int)periodScoreDTO.AwayScore;
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
        /// <param name="stageSes">A <see cref="restSportEventStatus" /> instance containing status data about the associated sport event</param>
        public SportEventStatusDTO(stageSportEventStatus stageSes)
        {
            Guard.Argument(stageSes, nameof(stageSes)).NotNull();

            var tempProperties = new Dictionary<string, object>();

            var eventResults = new List<EventResultDTO>();
            if (stageSes.results?.competitor != null && stageSes.results.competitor.Any())
            {
                var i = 0;
                foreach (var resultType in stageSes.results.competitor)
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

            ApplyPropertyValue(true, "WinnerId", stageSes.winner_id, tempProperties);
            ApplyPropertyValue(stageSes.period_of_leaderSpecified, "period_of_leader", stageSes.period_of_leader, tempProperties);

            Properties = new ReadOnlyDictionary<string, object>(tempProperties);

            Status = MessageMapperHelper.GetEnumValue(stageSes.status, EventStatus.Unknown);
            ApplyPropertyValue(true, "Status", (int)Status, tempProperties);

            MatchStatusId = -1;

            WinnerId = !string.IsNullOrEmpty(stageSes.winner_id) ? URN.Parse(stageSes.winner_id) : null;

            PeriodOfLadder = stageSes.period_of_leaderSpecified ? stageSes.period_of_leader : (int?)null;

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
        /// Adds the provided <c>name</c> and <c>value</c> to the provided <see cref="IDictionary{String, Object}"/>
        /// if so specified by the <c>specified</c>"/> field
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
