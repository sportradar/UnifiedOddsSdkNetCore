/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// A factory used to construct <see cref="IMarketMappingProvider"/> instances
    /// </summary>
    internal class MarketMappingProviderFactory : IMarketMappingProviderFactory
    {
        /// <summary>
        /// A <see cref="IMarketCacheProvider"/> instance used to retrieve market descriptors
        /// </summary>
        private readonly IMarketCacheProvider _marketCacheProvider;

        /// <summary>
        /// The event status cache
        /// </summary>
        private readonly ISportEventStatusCache _eventStatusCache;

        /// <summary>
        /// A <see cref="ExceptionHandlingStrategy"/> describing the mode in which the SDK is running
        /// </summary>
        private readonly ExceptionHandlingStrategy _exceptionStrategy;

        private readonly IProducerManager _producerManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="NameProviderFactory"/> class.
        /// </summary>
        /// <param name="marketCacheProvider">A <see cref="IMarketCacheProvider"/> instance used to retrieve market descriptors</param>
        /// <param name="eventStatusCache">A <see cref="ISportEventStatusCache"/> instance used to retrieve event status data</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> specifying how to handle potential exceptions thrown to the user code</param>
        /// <param name="producerManager">An <see cref="IProducerManager"/> used to get <see cref="IProducer"/></param>
        public MarketMappingProviderFactory(IMarketCacheProvider marketCacheProvider, ISportEventStatusCache eventStatusCache, ExceptionHandlingStrategy exceptionStrategy, IProducerManager producerManager)
        {
            Guard.Argument(marketCacheProvider).NotNull();
            Guard.Argument(eventStatusCache).NotNull();
            Guard.Argument(producerManager).NotNull();

            _marketCacheProvider = marketCacheProvider;
            _eventStatusCache = eventStatusCache;
            _exceptionStrategy = exceptionStrategy;
            _producerManager = producerManager;
        }

        /// <summary>
        /// Builds and returns a new instance of the <see cref="INameProvider" />
        /// </summary>
        /// <param name="sportEvent">A <see cref="ISportEvent" /> instance representing associated sport @event</param>
        /// <param name="marketId">A market identifier, identifying the market associated with the returned instance</param>
        /// <param name="specifiers">A <see cref="IReadOnlyDictionary{String, String}" /> representing specifiers of the associated market</param>
        /// <param name="producer">A <see cref="IProducer"/> of the <see cref="ISportEvent"/></param>
        /// <param name="sportId">A sportId of the <see cref="ISportEvent"/></param>
        /// <returns>Returns an instance of <see cref="IMarketMappingProvider"/></returns>
        public IMarketMappingProvider BuildMarketMappingProvider(ISportEvent sportEvent, int marketId, IReadOnlyDictionary<string, string> specifiers, IProducer producer, URN sportId)
        {
            return new MarketMappingProvider(_marketCacheProvider, _eventStatusCache, sportEvent, marketId, specifiers, producer, sportId, _exceptionStrategy, _producerManager);
        }
    }
}