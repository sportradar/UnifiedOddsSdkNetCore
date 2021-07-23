/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Class MatchStatus
    /// </summary>
    /// <seealso cref="CompetitionStatus" />
    /// <seealso cref="IMatchStatus" />
    internal class MatchStatus : CompetitionStatus, IMatchStatusV1
    {
        private readonly decimal? _homeScore;
        private readonly decimal? _awayScore;

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
        decimal? IMatchStatusV1.HomeScore => _homeScore;

        /// <summary>
        /// Gets the score of the away competitor competing on the associated sport event
        /// </summary>
        /// <value>The score of the away competitor competing on the associated sport event</value>
        decimal? IMatchStatusV1.AwayScore => _awayScore;

        /// <summary>
        /// Gets the score of the home competitor competing on the associated sport event
        /// </summary>
        /// <value>The score of the home competitor competing on the associated sport event</value>
        public decimal HomeScore => _homeScore ?? 0;

        /// <summary>
        /// Gets the score of the away competitor competing on the associated sport event
        /// </summary>
        /// <value>The score of the away competitor competing on the associated sport event</value>
        public decimal AwayScore => _awayScore ?? 0;

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
        /// Get match status as an asynchronous operation
        /// </summary>
        /// <param name="culture">The culture used to fetch status id and description</param>
        /// <returns>ILocalizedNamedValue</returns>
        public async Task<ILocalizedNamedValue> GetMatchStatusAsync(CultureInfo culture)
        {
            return SportEventStatusCI == null || SportEventStatusCI.MatchStatusId < 0  || MatchStatusCache == null
                ? null
                : await MatchStatusCache.GetAsync(SportEventStatusCI.MatchStatusId, new List<CultureInfo> {culture}).ConfigureAwait(false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchStatus"/> class
        /// </summary>
        /// <param name="ci">The cache item</param>
        /// <param name="matchStatusesCache">The match statuses cache</param>
        public MatchStatus(SportEventStatusCI ci, ILocalizedNamedValueCache matchStatusesCache)
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
            _homeScore = ci.HomeScore;
            _awayScore = ci.AwayScore;
            HomePenaltyScore = ci.HomePenaltyScore;
            AwayPenaltyScore = ci.AwayPenaltyScore;
            DecidedByFed = ci.DecidedByFed;
        }
    }
}
