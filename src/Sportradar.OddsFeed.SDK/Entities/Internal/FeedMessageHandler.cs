// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    internal class FeedMessageHandler : IFeedMessageHandler
    {
        private readonly ICacheStore<string> _fixtureCache;

        private readonly object _lock = new object();

        public FeedMessageHandler(ICacheStore<string> fixtureCache)
        {
            Guard.Argument(fixtureCache, nameof(fixtureCache)).NotNull();

            _fixtureCache = fixtureCache;
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

                _fixtureCache.Add(fixtureCacheId, fixtureCacheId);
                return false;
            }
        }

        private string GenerateFixtureChangeId(fixture_change fixtureChange)
        {
            return $"{fixtureChange.EventId}_{fixtureChange.ProducerId.ToString()}_{fixtureChange.GeneratedAt.ToString()}";
        }
    }
}
