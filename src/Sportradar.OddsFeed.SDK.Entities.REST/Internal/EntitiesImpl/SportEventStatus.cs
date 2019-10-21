/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Dawn;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a status of a sport event
    /// </summary>
    /// <seealso cref="ISportEventStatus" />
    internal class SportEventStatus : EntityPrinter, ISportEventStatus
    {
        private readonly SportEventStatusCI _cacheItem;
        private readonly ILocalizedNamedValueCache _matchStatusCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventStatus"/> class
        /// </summary>
        /// <param name="cacheItem">A <see cref="SportEventStatusCI"/> containing information about sport event status, which will be used to initialize a new instance</param>
        /// <param name="matchStatusCache">A <see cref="ILocalizedNamedValueCache"/> used to retrieve event status</param>
        public SportEventStatus(SportEventStatusCI cacheItem, ILocalizedNamedValueCache matchStatusCache)
        {
            Guard.Argument(cacheItem).NotNull();
            Guard.Argument(matchStatusCache).NotNull();

            _cacheItem = cacheItem;
            _matchStatusCache = matchStatusCache;
        }

        /// <summary>
        /// Gets a <see cref="EventStatus" /> describing the high-level status of the associated sport event
        /// </summary>
        public EventStatus Status => _cacheItem.Status;

        /// <summary>
        /// Gets a value indicating whether a data journalist is present od the associated sport event, or a null reference if the information is not available
        /// </summary>
        public int? IsReported => _cacheItem.IsReported;

        /// <summary>
        /// Gets the score of the home competitor competing on the associated sport event
        /// </summary>
        public decimal? HomeScore => _cacheItem.HomeScore;

        /// <summary>
        /// Gets the score of the away competitor competing on the associated sport event
        /// </summary>
        public decimal? AwayScore => _cacheItem.AwayScore;

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{String, Object}" /> containing additional event status values
        /// </summary>
        public IReadOnlyDictionary<string, object> Properties => _cacheItem.Properties;

        /// <summary>
        /// Gets the value of the property specified by it's name
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        public object GetPropertyValue(string propertyName)
        {
            return _cacheItem.GetPropertyValue(propertyName);
        }

        /// <summary>
        /// Get match status as an asynchronous operation
        /// </summary>
        public async Task<ILocalizedNamedValue> GetMatchStatusAsync()
        {
            if (_cacheItem.MatchStatusId < 0)
            {
                //TODO: write ERRROR
                return null;
            }
            return await _matchStatusCache.GetAsync(_cacheItem.MatchStatusId).ConfigureAwait(false);
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing the id of the current instance</returns>
        protected override string PrintI()
        {
            return $"Status:{Status}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing compacted representation of the current instance</returns>
        protected override string PrintC()
        {
            string result = $"{PrintI()}, IsReported: {IsReported}, HomeScore={HomeScore}, AwayScore={AwayScore}";
            return result;
        }

        /// <summary>
        /// Constructs and return a <see cref="string"/> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing details of the current instance</returns>
        protected override string PrintF()
        {

            var props = string.Join(", ", Properties.Select(c => c.Key));
            string result = $"{PrintC()}, Properties: [{props}]";
            return result;
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }
    }
}
