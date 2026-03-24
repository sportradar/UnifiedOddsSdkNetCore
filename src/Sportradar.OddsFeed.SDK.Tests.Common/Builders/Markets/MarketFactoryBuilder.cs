// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Markets;

internal static class MarketFactoryBuilder
{
    internal sealed class BuilderStubbingOutSportEventAndCaches
    {
        private IMarketCacheProvider _marketCacheProvider;
        private ISportEventStatusCache _sportEventStatusCache;
        private INamedValueCache _voidReasonCache;

        public static BuilderStubbingOutSportEventAndCaches StubbingOutCaches()
        {
            return new BuilderStubbingOutSportEventAndCaches();
        }

        public BuilderStubbingOutSportEventAndCaches With(IMarketCacheProvider marketCacheProvider)
        {
            _marketCacheProvider = marketCacheProvider;
            return this;
        }

        public BuilderStubbingOutSportEventAndCaches With(ISportEventStatusCache sportEventStatusCache)
        {
            _sportEventStatusCache = sportEventStatusCache;
            return this;
        }

        public BuilderStubbingOutSportEventAndCaches WithVoidReasonCache(INamedValueCache voidReasonCache)
        {
            _voidReasonCache = voidReasonCache;
            return this;
        }

        public IMarketFactory Build()
        {
            var marketCacheProvider = _marketCacheProvider ?? new Mock<IMarketCacheProvider>().Object;
            var strategy = ExceptionHandlingStrategy.Throw;
            var loggerFactory = NullLoggerFactory.Instance;
            var profileCache = new Mock<IProfileCache>().Object;

            var nameExpressionFactory = new NameExpressionFactory(new OperandFactory(), profileCache);
            var nameProviderFactory = new NameProviderFactory(marketCacheProvider, profileCache, nameExpressionFactory, strategy, loggerFactory);

            var eventStatusCache = _sportEventStatusCache ?? new Mock<ISportEventStatusCache>().Object;
            var mappingProviderFactory = new MarketMappingProviderFactory(marketCacheProvider, eventStatusCache, strategy);

            var voidReasonCache = _voidReasonCache ?? new Mock<INamedValueCache>().Object;
            var namedValuesProvider = new NamedValuesProvider(voidReasonCache,
                new Mock<INamedValueCache>().Object,
                new Mock<INamedValueCache>().Object,
                new Mock<ILocalizedNamedValueCache>().Object);

            return new MarketFactory(marketCacheProvider,
                nameProviderFactory,
                mappingProviderFactory,
                namedValuesProvider,
                voidReasonCache,
                strategy);
        }
    }
}
