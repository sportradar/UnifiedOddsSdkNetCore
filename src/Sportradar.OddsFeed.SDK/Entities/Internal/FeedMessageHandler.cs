/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Runtime.Caching;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    internal class FeedMessageHandler : IFeedMessageHandler
    {
        private readonly MemoryCache _fixtureCache;

        private readonly CacheItemPolicy _cacheItemPolicy;

        private readonly object _lock;

        public FeedMessageHandler(MemoryCache fixtureCache, CacheItemPolicy cacheItemPolicy)
        {
            Guard.Argument(fixtureCache, nameof(fixtureCache)).NotNull();
            Guard.Argument(cacheItemPolicy, nameof(cacheItemPolicy)).NotNull();

            _fixtureCache = fixtureCache;
            _cacheItemPolicy = cacheItemPolicy;
            _lock = new object();
        }

        /// <summary>
        /// Stops the processing fixture change (so it is not dispatched twice)
        /// </summary>
        /// <param name="fixtureChange">The fixture change</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public bool StopProcessingFixtureChange(fixture_change fixtureChange)
        {
            if (fixtureChange == null)
            {
                return false;
            }

            lock (_lock)
            {
                var fixtureCacheId = GenerateFixtureChangeId(fixtureChange);
                if (_fixtureCache.Contains(fixtureCacheId))
                {
                    return true;
                }

                _fixtureCache.Add(fixtureCacheId, fixtureCacheId, _cacheItemPolicy);
                return false;
            }
        }

        private string GenerateFixtureChangeId(fixture_change fixtureChange)
        {
            return $"{fixtureChange.EventId}_{fixtureChange.ProducerId}_{fixtureChange.GeneratedAt}";
        }
    }
}
