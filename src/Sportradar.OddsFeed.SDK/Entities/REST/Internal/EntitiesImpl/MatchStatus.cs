// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    /// <summary>
    /// Class MatchStatus
    /// </summary>
    /// <seealso cref="CompetitionStatus" />
    /// <seealso cref="IMatchStatus" />
    internal class MatchStatus : CompetitionStatus, IMatchStatus
    {
        /// <summary>
        /// Gets the <see cref="IEventClock" /> instance describing the timings in the current event
        /// </summary>
        /// <value>The <see cref="IEventClock" /> instance describing the timings in the current event</value>
        public IEventClock EventClock { get; }

        /// <summary>
        /// Gets the list of <see cref="IPeriodScore" />
        /// </summary>
        /// <value>The list of <see cref="IPeriodScore" /></value>
        public IEnumerable<IPeriodScore> PeriodScores { get; }

        /// <summary>
        /// Gets the score of the home competitor competing on the associated sport event
        /// </summary>
        /// <value>The score of the home competitor competing on the associated sport event</value>
        public decimal? HomeScore { get; }

        /// <summary>
        /// Gets the score of the away competitor competing on the associated sport event
        /// </summary>
        /// <value>The score of the away competitor competing on the associated sport event</value>
        public decimal? AwayScore { get; }

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
        /// Returns match statistics
        /// </summary>
        public IMatchStatistics Statistics { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchStatus"/> class
        /// </summary>
        /// <param name="ci">The cache item</param>
        /// <param name="matchStatusesCache">The match statuses cache</param>
        public MatchStatus(SportEventStatusCacheItem ci, ILocalizedNamedValueCache matchStatusesCache)
            : base(ci, matchStatusesCache)
        {
            if (ci.EventClock != null)
            {
                EventClock = new EventClock(ci.EventClock);
            }
            if (ci.PeriodScores != null)
            {
                PeriodScores = ci.PeriodScores.Select(s => new PeriodScore(s, MatchStatusCache));
            }
            HomeScore = ci.HomeScore;
            AwayScore = ci.AwayScore;
            HomePenaltyScore = ci.HomePenaltyScore;
            AwayPenaltyScore = ci.AwayPenaltyScore;
            DecidedByFed = ci.DecidedByFed;

            if (ci?.SportEventStatistics != null)
            {
                Statistics = new MatchStatistics(ci.SportEventStatistics.TotalStatisticsDtos,
                                                 ci.SportEventStatistics.PeriodStatisticsDtos);
            }
        }

        /// <summary>
        /// Get match status as an asynchronous operation
        /// </summary>
        /// <param name="culture">The culture used to fetch status id and description</param>
        /// <returns>ILocalizedNamedValue</returns>
        public async Task<ILocalizedNamedValue> GetMatchStatusAsync(CultureInfo culture)
        {
            // SportEventStatusCacheItem and MatchStatusCache cannot be null
            //return SportEventStatusCacheItem == null || SportEventStatusCacheItem.MatchStatusId < 0 || MatchStatusCache == null
            return SportEventStatusCacheItem.MatchStatusId < 0
                ? null
                : await MatchStatusCache.GetAsync(SportEventStatusCacheItem.MatchStatusId, new List<CultureInfo> { culture }).ConfigureAwait(false);
        }
    }
}
