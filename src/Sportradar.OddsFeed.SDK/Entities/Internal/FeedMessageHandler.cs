/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using System.Runtime.Caching;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    internal class FeedMessageHandler : IFeedMessageHandler
    {
        /// <summary>
        /// The execution log
        /// </summary>
        //private static readonly ILogger ExecutionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(FeedMessageHandler));

        private readonly MemoryCache _fixtureCache;

        private readonly CacheItemPolicy _cacheItemPolicy;

        private readonly object _lock;

        public FeedMessageHandler(MemoryCache fixtureCache, CacheItemPolicy cacheItemPolicy)
        {
            Guard.Argument(fixtureCache, nameof()).NotNull();
            Guard.Argument(cacheItemPolicy, nameof()).NotNull();

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
                    //ExecutionLog.LogDebug($"Processing fixtureChange for id={fixtureChange.EventId}, producer={fixtureChange.ProducerId} and timestamp={fixtureChange.GeneratedAt} stopped. Already processed.");
                    return true;
                }

                _fixtureCache.Add(fixtureCacheId, fixtureCacheId, _cacheItemPolicy);
                //ExecutionLog.LogDebug($"Processing fixtureChange for id={fixtureChange.EventId}, producer={fixtureChange.ProducerId} and timestamp={fixtureChange.GeneratedAt}.");
                return false;
            }
        }

        private string GenerateFixtureChangeId(fixture_change fixtureChange)
        {
            return $"{fixtureChange.EventId}_{fixtureChange.ProducerId}_{fixtureChange.GeneratedAt}";
        }
    }
}