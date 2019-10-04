/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing the status of a sport event
    /// </summary>
    public interface ISportEventStatus
    {
        /// <summary>
        /// Gets a <see cref="EventStatus"/> describing the high-level status of the associated sport event
        /// </summary>
        EventStatus Status { get; }

        /// <summary>
        /// Gets a value indicating whether a data journalist is present od the associated sport event, or a
        /// null reference if the information is not available
        /// </summary>
        int? IsReported { get; }

        /// <summary>
        /// Gets the score of the home competitor competing on the associated sport event
        /// </summary>
        decimal? HomeScore { get; }

        /// <summary>
        /// Gets the score of the away competitor competing on the associated sport event
        /// </summary>
        decimal? AwayScore { get; }

        /// <summary>
        /// Gets the value of the property specified by it's name
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>A <see cref="object"/> representation of the value of the specified property, or a null reference
        /// if the value of the specified property was not specified</returns>
        object GetPropertyValue(string propertyName);

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{String, Object}"/> containing additional event status values
        /// </summary>
        /// <value>a <see cref="IReadOnlyDictionary{String, Object}"/> containing additional event status values</value>
        /// <remarks><para>List of possible keys:</para>
        /// <list type="bullet">
        /// <item><description>AggregateAwayScore</description></item>
        /// <item><description>AggregateHomeScore</description></item>
        /// <item><description>AggregateWinnerId</description></item>
        /// <item><description>AwayBatter</description></item>
        /// <item><description>AwayDismissals</description></item>
        /// <item><description>AwayGameScore</description></item>
        /// <item><description>AwayLegScore</description></item>
        /// <item><description>AwayPenaltyRuns</description></item>
        /// <item><description>AwayRemainingBowls</description></item>
        /// <item><description>AwayScore</description></item>
        /// <item><description>AwaySuspend</description></item>
        /// <item><description>Balls</description></item>
        /// <item><description>Bases</description></item>
        /// <item><description>Clock_MatchTime</description></item>
        /// <item><description>Clock_RemainingTime</description></item>
        /// <item><description>Clock_RemainingTimeInPeriod</description></item>
        /// <item><description>Clock_StoppageTime</description></item>
        /// <item><description>Clock_StoppageTimeAnnounced</description></item>
        /// <item><description>Clock_Stopped</description></item>
        /// <item><description>Clock{ClockNumber}_MatchTime</description></item>
        /// <item><description>Clock{ClockNumber}_StoppageTime</description></item>
        /// <item><description>Clock{ClockNumber}_StoppageTimeAnnounced</description></item>
        /// <item><description>CurrentCtTeam</description></item>
        /// <item><description>CurrentEnd</description></item>
        /// <item><description>CurrentServer</description></item>
        /// <item><description>DecidedByFed</description></item>
        /// <item><description>Delivery</description></item>
        /// <item><description>ExpeditedMode</description></item>
        /// <item><description>HomeBatter</description></item>
        /// <item><description>HomeDismissals</description></item>
        /// <item><description>HomeGameScore</description></item>
        /// <item><description>HomeLegScore</description></item>
        /// <item><description>HomePenaltyRuns</description></item>
        /// <item><description>HomeRemainingBowls</description></item>
        /// <item><description>HomeScore</description></item>
        /// <item><description>HomeSuspend</description></item>
        /// <item><description>Innings</description></item>
        /// <item><description>Outs</description></item>
        /// <item><description>Over</description></item>
        /// <item><description>Period</description></item>
        /// <item><description>PeriodScore{PeriodScoreNumber}_AwayScore</description></item>
        /// <item><description>PeriodScore{PeriodScoreNumber}_HomeScore</description></item>
        /// <item><description>PeriodScore{PeriodScoreNumber}_MatchStatusCode</description></item>
        /// <item><description>PeriodScore{PeriodScoreNumber}_Number</description></item>
        /// <item><description>Position</description></item>
        /// <item><description>Possession</description></item>
        /// <item><description>RemainingReds</description></item>
        /// <item><description>Reporting</description></item>
        /// <item><description>Result{ResultNumber}_AwayScore</description></item>
        /// <item><description>Result{ResultNumber}_Climber</description></item>
        /// <item><description>Result{ResultNumber}_ClimberRanking</description></item>
        /// <item><description>Result{ResultNumber}_HomeScore</description></item>
        /// <item><description>Result{ResultNumber}_Id</description></item>
        /// <item><description>Result{ResultNumber}_MatchStatusCode</description></item>
        /// <item><description>Result{ResultNumber}_Points</description></item>
        /// <item><description>Result{ResultNumber}_Sprint</description></item>
        /// <item><description>Result{ResultNumber}_SprintRanking</description></item>
        /// <item><description>Result{ResultNumber}_Status</description></item>
        /// <item><description>Result{ResultNumber}_StatusComment</description></item>
        /// <item><description>Result{ResultNumber}_Time</description></item>
        /// <item><description>Result{ResultNumber}_TimeRanking</description></item>
        /// <item><description>Status</description></item>
        /// <item><description>Strikes</description></item>
        /// <item><description>Throw</description></item>
        /// <item><description>Tiebreak</description></item>
        /// <item><description>Try</description></item>
        /// <item><description>Visit</description></item>
        /// <item><description>WinnerId</description></item>
        /// <item><description>WinningReason</description></item>
        /// <item><description>Yards</description></item>
        /// </list>
        /// </remarks>
        IReadOnlyDictionary<string, object> Properties { get; }

        /// <summary>
        /// Gets the match status for specific locale
        /// </summary>
        /// <returns>Returns the match status for specific locale</returns>
        Task<ILocalizedNamedValue> GetMatchStatusAsync();
    }
}
