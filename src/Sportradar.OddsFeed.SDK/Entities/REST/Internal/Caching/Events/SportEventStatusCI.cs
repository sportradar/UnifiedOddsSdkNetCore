/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    /// Class SportEventStatusCI
    /// </summary>
    /// <seealso cref="ISportEventStatusCI" />
    public class SportEventStatusCI : ISportEventStatusCI
    {
        /// <summary>
        /// Gets a <see cref="EventStatus"/> describing the high-level status of the associated sport event
        /// </summary>
        public EventStatus Status => FeedStatusDTO?.Status ?? SapiStatusDTO?.Status ?? EventStatus.Unknown;

        /// <summary>
        /// Gets a value indicating whether a data journalist is present on the associated sport event, or a
        /// null reference if the information is not available
        /// </summary>
        public int? IsReported => FeedStatusDTO?.IsReported ?? SapiStatusDTO?.IsReported;

        /// <summary>
        /// Gets the score of the home competitor competing on the associated sport event
        /// </summary>
        public decimal? HomeScore => FeedStatusDTO?.HomeScore ?? SapiStatusDTO?.HomeScore;

        /// <summary>
        /// Gets the score of the away competitor competing on the associated sport event
        /// </summary>
        public decimal? AwayScore => FeedStatusDTO?.AwayScore ?? SapiStatusDTO?.AwayScore;

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing additional event status values
        /// </summary>
        /// <value>a <see cref="IReadOnlyDictionary{String, Object}"/> containing additional event status values</value>
        public IReadOnlyDictionary<string, object> Properties => FeedStatusDTO?.Properties ?? SapiStatusDTO?.Properties;

        /// <summary>
        /// Gets the match status for specific locale
        /// </summary>
        public int MatchStatusId => FeedStatusDTO?.MatchStatusId ?? SapiStatusDTO?.MatchStatusId ?? 0;

        /// <summary>
        /// Gets the winner identifier
        /// </summary>
        /// <value>The winner identifier</value>
        public URN WinnerId => FeedStatusDTO?.WinnerId ?? SapiStatusDTO?.WinnerId;

        /// <summary>
        /// Gets the reporting status
        /// </summary>
        /// <value>The reporting status</value>
        public ReportingStatus ReportingStatus => FeedStatusDTO?.ReportingStatus ?? SapiStatusDTO?.ReportingStatus ?? ReportingStatus.Unknown;

        /// <summary>
        /// Gets the period scores
        /// </summary>
        public IEnumerable<PeriodScoreDTO> PeriodScores => FeedStatusDTO?.PeriodScores ?? SapiStatusDTO?.PeriodScores;

        /// <summary>
        /// Gets the event clock
        /// </summary>
        /// <value>The event clock</value>
        public EventClockDTO EventClock => FeedStatusDTO?.EventClock ?? SapiStatusDTO?.EventClock;

        /// <summary>
        /// Gets the event results
        /// </summary>
        /// <value>The event results</value>
        public IEnumerable<EventResultDTO> EventResults => FeedStatusDTO?.EventResults ?? SapiStatusDTO?.EventResults;

        /// <summary>
        /// Gets the sport event statistics
        /// </summary>
        /// <value>The sport event statistics</value>
        public SportEventStatisticsDTO SportEventStatistics => FeedStatusDTO?.SportEventStatistics ?? SapiStatusDTO?.SportEventStatistics;

        /// <summary>
        /// Gets the indicator for competitors if there are home or away
        /// </summary>
        /// <value>The indicator for competitors if there are home or away</value>
        public IDictionary<HomeAway, URN> HomeAwayCompetitors => FeedStatusDTO?._homeAwayCompetitors ?? SapiStatusDTO?._homeAwayCompetitors;

        /// <summary>
        /// Gets the penalty score of the home competitor competing on the associated sport event (for Ice Hockey)
        /// </summary>
        public int? HomePenaltyScore => FeedStatusDTO?.HomePenaltyScore ?? SapiStatusDTO?.HomePenaltyScore;

        /// <summary>
        /// Gets the penalty score of the away competitor competing on the associated sport event (for Ice Hockey)
        /// </summary>
        public int? AwayPenaltyScore => FeedStatusDTO?.AwayPenaltyScore ?? SapiStatusDTO?.AwayPenaltyScore;

        /// <summary>
        /// Gets the indicator wither the event is decided by fed
        /// </summary>
        public bool? DecidedByFed => FeedStatusDTO?.DecidedByFed ?? SapiStatusDTO?.DecidedByFed;

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
        /// Gets the <see cref="SportEventStatusDTO"/> received from the feed
        /// </summary>
        /// <value>The <see cref="SportEventStatusDTO"/> received from the feed</value>
        internal SportEventStatusDTO FeedStatusDTO { get; private set; }

        /// <summary>
        /// Gets the <see cref="SportEventStatusDTO"/> received from the Sports API
        /// </summary>
        /// <value>The <see cref="SportEventStatusDTO"/> received from the Sports API</value>
        internal SportEventStatusDTO SapiStatusDTO { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventStatusCI"/> class
        /// </summary>
        /// <param name="feedStatusDTO">The <see cref="SportEventStatusDTO"/> received from the feed</param>
        /// <param name="sapiStatusDTO">The <see cref="SportEventStatusDTO"/> received from the Sports API</param>
        public SportEventStatusCI(SportEventStatusDTO feedStatusDTO, SportEventStatusDTO sapiStatusDTO)
        {
            if (feedStatusDTO == null && sapiStatusDTO == null)
            {
                throw new ArgumentNullException(nameof(feedStatusDTO));
            }

            FeedStatusDTO = feedStatusDTO;
            SapiStatusDTO = sapiStatusDTO;

            UpdatePeriodStatistics();
        }

        /// <summary>
        /// Save the status received from the feed
        /// </summary>
        /// <param name="feedDTO">The <see cref="SportEventStatusDTO"/> received from the feed</param>
        internal void SetFeedStatus(SportEventStatusDTO feedDTO)
        {
            if (feedDTO != null)
            {
                FeedStatusDTO = feedDTO;

                UpdatePeriodStatistics();
            }
        }

        /// <summary>
        /// Save the status received from the Sports API
        /// </summary>
        /// <param name="sapiDTO">The <see cref="SportEventStatusDTO"/> received from the Sports API</param>
        internal void SetSapiStatus(SportEventStatusDTO sapiDTO)
        {
            if (sapiDTO != null)
            {
                SapiStatusDTO = sapiDTO;

                UpdatePeriodStatistics();
            }
        }

        private void UpdatePeriodStatistics()
        {
            if (SapiStatusDTO?.SportEventStatistics?.PeriodStatisticsDTOs != null)
            {
                if (FeedStatusDTO != null)
                {
                    if (FeedStatusDTO.SportEventStatistics == null)
                    {
                        FeedStatusDTO.SportEventStatistics = SapiStatusDTO.SportEventStatistics;
                    }
                    else if (FeedStatusDTO.SportEventStatistics.PeriodStatisticsDTOs == null)
                    {
                        FeedStatusDTO.SportEventStatistics.PeriodStatisticsDTOs = SapiStatusDTO.SportEventStatistics.PeriodStatisticsDTOs;
                    }
                }
            }
        }
    }
}