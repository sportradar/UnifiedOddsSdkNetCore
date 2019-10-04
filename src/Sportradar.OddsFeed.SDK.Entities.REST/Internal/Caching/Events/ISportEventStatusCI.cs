/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    /// Defines a contract for classes implementing sport event status cache item
    /// </summary>
    public interface ISportEventStatusCI
    {
        /// <summary>
        /// Gets a <see cref="EventStatus"/> describing the high-level status of the associated sport event
        /// </summary>
        EventStatus Status { get; }

        /// <summary>
        /// Gets a value indicating whether a data journalist is present on the associated sport event, or a
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
        /// Gets a <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing additional event status values
        /// </summary>
        /// <value>a <see cref="IReadOnlyDictionary{String, Object}"/> containing additional event status values</value>
        IReadOnlyDictionary<string, object> Properties { get; }

        /// <summary>
        /// Gets the match status for specific locale
        /// </summary>
        int MatchStatusId { get; }

        /// <summary>
        /// Gets the winner identifier
        /// </summary>
        /// <value>The winner identifier</value>
        URN WinnerId { get; }

        /// <summary>
        /// Gets the reporting status
        /// </summary>
        /// <value>The reporting status</value>
        ReportingStatus ReportingStatus { get; }

        /// <summary>
        /// Gets the period scores
        /// </summary>
        IEnumerable<PeriodScoreDTO> PeriodScores { get; }

        /// <summary>
        /// Gets the event clock
        /// </summary>
        /// <value>The event clock</value>
        EventClockDTO EventClock { get; }

        /// <summary>
        /// Gets the event results
        /// </summary>
        /// <value>The event results</value>
        IEnumerable<EventResultDTO> EventResults { get; }

        /// <summary>
        /// Gets the sport event statistics
        /// </summary>
        /// <value>The sport event statistics</value>
        SportEventStatisticsDTO SportEventStatistics { get; }

        /// <summary>
        /// Gets the indicator for competitors if there are home or away
        /// </summary>
        /// <value>The indicator for competitors if there are home or away</value>
        IDictionary<HomeAway, URN> HomeAwayCompetitors { get; }

        /// <summary>
        /// Gets the value of the property specified by it's name
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>A <see cref="object"/> representation of the value of the specified property, or a null reference
        /// if the value of the specified property was not specified</returns>
        object GetPropertyValue(string propertyName);

        /// <summary>
        /// Gets the penalty score of the home competitor competing on the associated sport event (for Ice Hockey)
        /// </summary>
        int? HomePenaltyScore { get; }

        /// <summary>
        /// Gets the penalty score of the away competitor competing on the associated sport event (for Ice Hockey)
        /// </summary>
        int? AwayPenaltyScore { get; }

        /// <summary>
        /// Gets the indicator wither the event is decided by fed
        /// </summary>
        bool? DecidedByFed { get; }
    }
}