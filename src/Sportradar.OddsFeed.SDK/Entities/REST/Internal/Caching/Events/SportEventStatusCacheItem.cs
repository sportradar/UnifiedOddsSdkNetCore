// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events
{
    /// <summary>
    /// Class SportEventStatusCacheItem
    /// </summary>
    /// <seealso cref="ISportEventStatusCacheItem" />
    internal class SportEventStatusCacheItem : ISportEventStatusCacheItem
    {
        /// <summary>
        /// Gets a <see cref="EventStatus"/> describing the high-level status of the associated sport event
        /// </summary>
        public EventStatus Status => FeedStatusDto?.Status ?? SapiStatusDto?.Status ?? EventStatus.Unknown;

        /// <summary>
        /// Gets a value indicating whether a data journalist is present on the associated sport event, or a
        /// null reference if the information is not available
        /// </summary>
        public int? IsReported => FeedStatusDto?.IsReported ?? SapiStatusDto?.IsReported;

        /// <summary>
        /// Gets the score of the home competitor competing on the associated sport event
        /// </summary>
        public decimal? HomeScore => FeedStatusDto?.HomeScore ?? SapiStatusDto?.HomeScore;

        /// <summary>
        /// Gets the score of the away competitor competing on the associated sport event
        /// </summary>
        public decimal? AwayScore => FeedStatusDto?.AwayScore ?? SapiStatusDto?.AwayScore;

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing additional event status values
        /// </summary>
        /// <value>a <see cref="IReadOnlyDictionary{String, Object}"/> containing additional event status values</value>
        public IReadOnlyDictionary<string, object> Properties => FeedStatusDto?.Properties ?? SapiStatusDto?.Properties;

        /// <summary>
        /// Gets the match status for specific locale
        /// </summary>
        public int MatchStatusId => FeedStatusDto?.MatchStatusId ?? SapiStatusDto?.MatchStatusId ?? 0;

        /// <summary>
        /// Gets the winner identifier
        /// </summary>
        /// <value>The winner identifier</value>
        public Urn WinnerId => FeedStatusDto?.WinnerId ?? SapiStatusDto?.WinnerId;

        /// <summary>
        /// Gets the reporting status
        /// </summary>
        /// <value>The reporting status</value>
        public ReportingStatus ReportingStatus => FeedStatusDto?.ReportingStatus ?? SapiStatusDto?.ReportingStatus ?? ReportingStatus.Unknown;

        /// <summary>
        /// Gets the period scores
        /// </summary>
        public IEnumerable<PeriodScoreDto> PeriodScores => FeedStatusDto?.PeriodScores ?? SapiStatusDto?.PeriodScores;

        /// <summary>
        /// Gets the event clock
        /// </summary>
        /// <value>The event clock</value>
        public EventClockDto EventClock => FeedStatusDto?.EventClock ?? SapiStatusDto?.EventClock;

        /// <summary>
        /// Gets the event results
        /// </summary>
        /// <value>The event results</value>
        public IEnumerable<EventResultDto> EventResults => FeedStatusDto?.EventResults ?? SapiStatusDto?.EventResults;

        /// <summary>
        /// Gets the sport event statistics
        /// </summary>
        /// <value>The sport event statistics</value>
        public SportEventStatisticsDto SportEventStatistics => FeedStatusDto?.SportEventStatistics ?? SapiStatusDto?.SportEventStatistics;

        /// <summary>
        /// Gets the indicator for competitors if there are home or away
        /// </summary>
        /// <value>The indicator for competitors if there are home or away</value>
        public IDictionary<HomeAway, Urn> HomeAwayCompetitors => FeedStatusDto?.HomeAwayCompetitors ?? SapiStatusDto?.HomeAwayCompetitors;

        /// <summary>
        /// Gets the penalty score of the home competitor competing on the associated sport event (for Ice Hockey)
        /// </summary>
        public int? HomePenaltyScore => FeedStatusDto?.HomePenaltyScore ?? SapiStatusDto?.HomePenaltyScore;

        /// <summary>
        /// Gets the penalty score of the away competitor competing on the associated sport event (for Ice Hockey)
        /// </summary>
        public int? AwayPenaltyScore => FeedStatusDto?.AwayPenaltyScore ?? SapiStatusDto?.AwayPenaltyScore;

        /// <summary>
        /// Gets the indicator wither the event is decided by fed
        /// </summary>
        public bool? DecidedByFed => FeedStatusDto?.DecidedByFed ?? SapiStatusDto?.DecidedByFed;

        /// <summary>
        /// Gets the period of ladder.
        /// </summary>
        /// <value>The period of ladder.</value>
        public int? PeriodOfLadder => FeedStatusDto?.PeriodOfLadder ?? SapiStatusDto?.PeriodOfLadder;

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
        /// Gets the <see cref="SportEventStatusDto"/> received from the feed
        /// </summary>
        /// <value>The <see cref="SportEventStatusDto"/> received from the feed</value>
        internal SportEventStatusDto FeedStatusDto { get; private set; }

        /// <summary>
        /// Gets the <see cref="SportEventStatusDto"/> received from the Sports API
        /// </summary>
        /// <value>The <see cref="SportEventStatusDto"/> received from the Sports API</value>
        internal SportEventStatusDto SapiStatusDto { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventStatusCacheItem"/> class
        /// </summary>
        /// <param name="feedStatusDto">The <see cref="SportEventStatusDto"/> received from the feed</param>
        /// <param name="sapiStatusDto">The <see cref="SportEventStatusDto"/> received from the Sports API</param>
        public SportEventStatusCacheItem(SportEventStatusDto feedStatusDto, SportEventStatusDto sapiStatusDto)
        {
            if (feedStatusDto == null && sapiStatusDto == null)
            {
                throw new ArgumentNullException(nameof(feedStatusDto));
            }

            FeedStatusDto = feedStatusDto;
            SapiStatusDto = sapiStatusDto;

            UpdatePeriodStatistics();
        }

        /// <summary>
        /// Save the status received from the feed
        /// </summary>
        /// <param name="feedDto">The <see cref="SportEventStatusDto"/> received from the feed</param>
        internal void SetFeedStatus(SportEventStatusDto feedDto)
        {
            if (feedDto != null)
            {
                FeedStatusDto = feedDto;

                UpdatePeriodStatistics();
            }
        }

        /// <summary>
        /// Save the status received from the Sports API
        /// </summary>
        /// <param name="sapiDto">The <see cref="SportEventStatusDto"/> received from the Sports API</param>
        internal void SetSapiStatus(SportEventStatusDto sapiDto)
        {
            if (sapiDto != null)
            {
                SapiStatusDto = sapiDto;

                UpdatePeriodStatistics();
            }
        }

        private void UpdatePeriodStatistics()
        {
            if (SapiStatusDto?.SportEventStatistics?.PeriodStatisticsDtos != null && FeedStatusDto != null)
            {
                if (FeedStatusDto.SportEventStatistics == null)
                {
                    FeedStatusDto.SportEventStatistics = SapiStatusDto.SportEventStatistics;
                }
                else if (FeedStatusDto.SportEventStatistics.PeriodStatisticsDtos == null)
                {
                    FeedStatusDto.SportEventStatistics.PeriodStatisticsDtos = SapiStatusDto.SportEventStatistics.PeriodStatisticsDtos;
                }
            }
            if (SapiStatusDto?.SportEventStatistics?.TotalStatisticsDtos != null && FeedStatusDto != null)
            {
                if (FeedStatusDto.SportEventStatistics == null)
                {
                    FeedStatusDto.SportEventStatistics = SapiStatusDto.SportEventStatistics;
                }
                else if (FeedStatusDto.SportEventStatistics.TotalStatisticsDtos == null)
                {
                    FeedStatusDto.SportEventStatistics.TotalStatisticsDtos = SapiStatusDto.SportEventStatistics.TotalStatisticsDtos;
                }
            }
        }
    }
}
