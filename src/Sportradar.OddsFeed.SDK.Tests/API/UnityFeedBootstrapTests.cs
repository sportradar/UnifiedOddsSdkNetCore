using System;
using System.IO;
using Castle.Core.Internal;
using Moq;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API
{
    public class UnityFeedBootstrapTests
    {
        private readonly IUnityContainer _childContainer1;

        private readonly IUnityContainer _childContainer2;

        public UnityFeedBootstrapTests()
        {
            IUnityContainer container = new UnityContainer().EnableDiagnostic();
            var config = TestConfigurationInternal.GetConfig();
            var dispatcher = new Mock<IGlobalEventDispatcher>().Object;

            container.RegisterBaseTypes(config, null, null);

            // we need to override initial loading of bookmaker details and producers
            var bookmakerDetailsProviderMock = new Mock<BookmakerDetailsProvider>("bookmakerDetailsUriFormat",
                                                                                  new TestDataFetcher(),
                                                                                  new Deserializer<bookmaker_details>(),
                                                                                  new BookmakerDetailsMapperFactory());
            bookmakerDetailsProviderMock.Setup(x => x.GetData(It.IsAny<string>())).Returns(TestConfigurationInternal.GetBookmakerDetails());
            var defaultBookmakerDetailsProvider = bookmakerDetailsProviderMock.Object;
            container.RegisterInstance<IDataProvider<BookmakerDetailsDTO>>(defaultBookmakerDetailsProvider, new ContainerControlledLifetimeManager());
            container.RegisterInstance(defaultBookmakerDetailsProvider, new ContainerControlledLifetimeManager());

            var newConfig = new OddsFeedConfigurationInternal(config, defaultBookmakerDetailsProvider);
            newConfig.Load();
            container.RegisterInstance<IOddsFeedConfiguration>(newConfig, new ContainerControlledLifetimeManager());
            container.RegisterInstance<IOddsFeedConfigurationInternal>(newConfig, new ContainerControlledLifetimeManager());

            container.RegisterTypes(dispatcher, config);

            //override
            container.RegisterType<IProducerManager, ProducerManager>(new ContainerControlledLifetimeManager(), new InjectionConstructor(new TestProducersProvider(), config));

            container.RegisterAdditionalTypes();

            _childContainer1 = container.CreateChildContainer();
            _childContainer2 = container.CreateChildContainer();
        }

        [Fact]
        public void ConfigurationIsResolved()
        {
            var config = _childContainer1.Resolve<IOddsFeedConfigurationInternal>();
            Assert.True(!string.IsNullOrEmpty(config.ApiBaseUri));
            Assert.True(!string.IsNullOrEmpty(config.ApiHost));
            Assert.True(!string.IsNullOrEmpty(config.Host));
            Assert.True(!string.IsNullOrEmpty(config.VirtualHost));
            Assert.True(!string.IsNullOrEmpty(config.ReplayApiHost));
        }

        [Fact]
        public void EachSessionGetsDifferentInstanceOfCacheMessageProcessor()
        {
            var statusCache1 = _childContainer1.Resolve<IFeedMessageProcessor>("CacheMessageProcessor");
            Assert.NotNull(statusCache1);
            Assert.IsType<CacheMessageProcessor>(statusCache1);

            var statusCache2 = _childContainer2.Resolve<IFeedMessageProcessor>("CacheMessageProcessor");
            Assert.NotEqual(statusCache1, statusCache2);
        }

        [Fact]
        public void EachSessionGetsSameInstanceOfFeedMessageHandler()
        {
            var messageHandler1 = _childContainer1.Resolve<IFeedMessageHandler>();
            Assert.NotNull(messageHandler1);
            Assert.IsType<FeedMessageHandler>(messageHandler1);

            var messageHandler2 = _childContainer2.Resolve<IFeedMessageHandler>();
            Assert.Equal(messageHandler1, messageHandler2);
        }

        //TODO
        //[Fact]
        public void CompositeMessageProcessorIsResolvedCorrectly()
        {
            var processor1 = _childContainer1.Resolve<CompositeMessageProcessor>();
            Assert.NotNull(processor1);
            Assert.IsType<CompositeMessageProcessor>(processor1);

            var processor12 = _childContainer1.Resolve<CompositeMessageProcessor>();
            Assert.Same(processor1, processor12);

            var processor2 = _childContainer2.Resolve<CompositeMessageProcessor>();
            Assert.NotSame(processor1, processor2);
        }

        [Fact]
        public void SportEventFactoryIsResolvedProperly()
        {
            var cache = _childContainer1.Resolve<ISportEntityFactory>();
            Assert.NotNull(cache);
            Assert.IsType<SportEntityFactory>(cache);

            var cache1 = _childContainer2.Resolve<ISportEntityFactory>();
            Assert.Equal(cache, cache1);
        }

        [Fact]
        public void MessageMapperIsResolvedProperly()
        {
            var cache = _childContainer1.Resolve<IFeedMessageMapper>();
            Assert.NotNull(cache);
            Assert.IsType<FeedMessageMapper>(cache);

            var cache1 = _childContainer2.Resolve<IFeedMessageMapper>();
            Assert.Equal(cache, cache1);
        }

        //TODO
        //[Fact]
        //public void OddsFeedSessionResolvedProperly()
        //{
        //    var session = _childContainer1.Resolve<IOddsFeedSession>(new ParameterOverride("messageInterest", MessageInterest.HighPriorityMessages));
        //    Assert.NotNull(session);
        //    Assert.IsType<OddsFeedSession>(session);

        //    var session1 = _childContainer2.Resolve<IOddsFeedSession>(new ParameterOverride("messageInterest", MessageInterest.LowPriorityMessages));
        //    Assert.NotEqual(session, session1);
        //}

        [Fact]
        public void MarketNameProvidersAndCachesAreResolved()
        {
            var provider = _childContainer1.Resolve<IDataProvider<EntityList<MarketDescriptionDTO>>>();
            Assert.NotNull(provider);

            var invariantCache = _childContainer1.Resolve<IMarketDescriptionCache>("InvariantMarketDescriptionsCache");
            Assert.NotNull(invariantCache);
            Assert.IsType<InvariantMarketDescriptionCache>(invariantCache);

            var variantCache = _childContainer1.Resolve<IMarketDescriptionCache>("VariantMarketDescriptionCache");
            Assert.NotNull(variantCache);
            Assert.IsType<VariantMarketDescriptionCache>(variantCache);

            var cacheSelector = _childContainer1.Resolve<IMarketCacheProvider>();
            Assert.NotNull(cacheSelector);
            Assert.IsType<MarketCacheProvider>(cacheSelector);

            var profileCache = _childContainer1.Resolve<IProfileCache>();
            Assert.NotNull(profileCache);
            Assert.IsType<ProfileCache>(profileCache);

            var expressionFactory = _childContainer1.Resolve<INameExpressionFactory>();
            Assert.NotNull(expressionFactory);

            var nameProviderFactory = _childContainer1.Resolve<INameProviderFactory>();
            Assert.NotNull(nameProviderFactory);
        }

        [Fact]
        public void BookmakerDetailsProviderIsResolvedProperly()
        {
            var fetcher = _childContainer1.Resolve<BookmakerDetailsProvider>();
            Assert.NotNull(fetcher);
            Assert.IsAssignableFrom<BookmakerDetailsProvider>(fetcher);

            var fetcher1 = _childContainer2.Resolve<BookmakerDetailsProvider>();
            Assert.Equal(fetcher, fetcher1);
        }

        //TODO: check this
        //[Fact]
        //public void SdkStatisticsWriterIsResolvedProperly()
        //{
        //    var stats1 = _childContainer1.Resolve<MetricsReporter>();
        //    Assert.NotNull(stats1, "Resolved //MetricsReporter cannot be a null reference");
        //    Assert.IsInstanceOfType(stats1, typeof(MetricsReporter), "Resolved SdkStats must be instance of //MetricsReporter");

        //    var stats2 = _childContainer2.Resolve<MetricsReporter>();
        //    Assert.Equal(stats1, stats2, "MetricsReporter instances resolved on different containers must be equal");
        //}

        [Fact]
        public void FeedSystemSessionIsResolvedProperly()
        {
            var stats1 = _childContainer1.Resolve<FeedSystemSession>();
            Assert.NotNull(stats1);
            Assert.IsType<FeedSystemSession>(stats1);

            var stats2 = _childContainer2.Resolve<FeedSystemSession>();
            Assert.Equal(stats1, stats2);
        }

        //TODO
        //[Fact]
        public void FeedRecoveryManagerIsResolvedProperly()
        {
            var stats1 = _childContainer1.Resolve<IFeedRecoveryManager>();
            Assert.NotNull(stats1);
            Assert.IsType<FeedRecoveryManager>(stats1);

            var stats2 = _childContainer2.Resolve<IFeedRecoveryManager>();
            Assert.Equal(stats1, stats2);
        }

        //TODO
        //[Fact]
        //public void SessionMessageManagerIsResolvedProperly()
        //{
        //    var stats1 = _childContainer1.Resolve<IFeedMessageProcessor>("SessionMessageManager");
        //    Assert.NotNull(stats1);
        //    Assert.IsAssignableFrom<IFeedMessageProcessor>(stats1);
        //    Assert.IsAssignableFrom<ISessionMessageManager>(stats1);

        //    var stats2 = _childContainer2.Resolve<IFeedMessageProcessor>("SessionMessageManager");
        //    Assert.NotSame(stats1, stats2);
        //}

        [Fact]
        public void DataRouterManagerIsResolvedProperly()
        {
            var stats1 = _childContainer1.Resolve<IDataRouterManager>();
            Assert.NotNull(stats1);
            Assert.IsType<DataRouterManager>(stats1);

            var stats2 = _childContainer2.Resolve<IDataRouterManager>();
            Assert.Equal(stats1, stats2);
        }

        [Fact]
        public void HttpClientAreResolvedProperly()
        {
            var dataFetcher1 = _childContainer1.Resolve<IDataFetcher>();
            Assert.NotNull(dataFetcher1);
            Assert.IsType<LogHttpDataFetcher>(dataFetcher1);

            var dataFetcher2 = _childContainer1.Resolve<IDataFetcher>("RecoveryDataFetcher");
            Assert.NotNull(dataFetcher2);
            Assert.IsType<LogHttpDataFetcher>(dataFetcher2);

            var dataFetcher3 = _childContainer1.Resolve<LogHttpDataFetcher>("FastLogHttpDataFetcher");
            Assert.NotNull(dataFetcher3);
            Assert.IsType<LogHttpDataFetcher>(dataFetcher3);

            Assert.NotEqual(dataFetcher1, dataFetcher2);
            Assert.NotEqual(dataFetcher2, dataFetcher3);
            Assert.NotEqual(dataFetcher1, dataFetcher3);
        }

        [Fact]
        public void HttpClientFastIsTransient()
        {
            var dataFetcher1 = _childContainer1.Resolve<LogHttpDataFetcher>("FastLogHttpDataFetcher");
            var dataFetcher2 = _childContainer1.Resolve<LogHttpDataFetcher>("FastLogHttpDataFetcher");
            var dataFetcher3 = _childContainer1.Resolve<LogHttpDataFetcher>("FastLogHttpDataFetcher");
            Assert.NotNull(dataFetcher1);
            Assert.IsType<LogHttpDataFetcher>(dataFetcher1);
            Assert.NotNull(dataFetcher2);
            Assert.IsType<LogHttpDataFetcher>(dataFetcher2);
            Assert.NotNull(dataFetcher3);
            Assert.IsType<LogHttpDataFetcher>(dataFetcher3);

            Assert.NotEqual(dataFetcher1, dataFetcher2);
            Assert.NotEqual(dataFetcher2, dataFetcher3);
            Assert.NotEqual(dataFetcher1, dataFetcher3);
            Assert.NotStrictEqual(dataFetcher1, dataFetcher2);
            Assert.NotStrictEqual(dataFetcher2, dataFetcher3);
            Assert.NotStrictEqual(dataFetcher1, dataFetcher3);
        }

        //TODO: requires network
        //[Fact]
        public void HttpClientTimeoutsWorkCorrectly()
        {
            var config = _childContainer1.Resolve<IOddsFeedConfiguration>();
            var dataFetcher1 = _childContainer1.Resolve<IDataFetcher>();

            Assert.Equal(30, config.HttpClientTimeout);
            var fullUrl = TestData.GetDelayedUrl(config.HttpClientTimeout * 300);
            var result = dataFetcher1.GetData(new Uri(fullUrl));
            var responseContent = new StreamReader(result).ReadToEnd();
            Assert.False(responseContent.IsNullOrEmpty());

            fullUrl = TestData.GetDelayedUrl(config.HttpClientTimeout * 1100);
            var ex = Assert.Throws<AggregateException>(() => result = dataFetcher1.GetData(new Uri(fullUrl)));
            Assert.Contains("task was canceled", ex.Message);
            Assert.NotNull(result);
        }

        //TODO: requires network
        //[Fact]
        public void HttpClientRecoveryTimeoutsWorkCorrectly()
        {
            var config = _childContainer1.Resolve<IOddsFeedConfiguration>();
            var dataFetcher1 = _childContainer1.Resolve<IDataFetcher>("RecoveryDataFetcher");

            Assert.Equal(30, config.RecoveryHttpClientTimeout);
            var fullUrl = TestData.GetDelayedUrl(config.RecoveryHttpClientTimeout * 300);
            var result = dataFetcher1.GetData(new Uri(fullUrl));
            var responseContent = new StreamReader(result).ReadToEnd();
            Assert.False(responseContent.IsNullOrEmpty());

            fullUrl = TestData.GetDelayedUrl(config.RecoveryHttpClientTimeout * 1100);
            var ex = Assert.Throws<AggregateException>(() => result = dataFetcher1.GetData(new Uri(fullUrl)));
            Assert.Contains("task was canceled", ex.Message);
            Assert.NotNull(result);
        }

        //TODO: requires network
        //[Fact]
        public void HttpClientFastTimeoutsWorkCorrectly()
        {
            var dataFetcher1 = _childContainer1.Resolve<LogHttpDataFetcher>("FastLogHttpDataFetcher");

            Assert.Equal(5, OperationManager.FastHttpClientTimeout.TotalSeconds);
            var fullUrl = TestData.GetDelayedUrl((int)OperationManager.FastHttpClientTimeout.TotalSeconds * 300);
            var result = dataFetcher1.GetData(new Uri(fullUrl));
            var responseContent = new StreamReader(result).ReadToEnd();
            Assert.False(responseContent.IsNullOrEmpty());

            fullUrl = TestData.GetDelayedUrl((int)OperationManager.FastHttpClientTimeout.TotalSeconds * 1100);
            var ex = Assert.Throws<AggregateException>(() => result = dataFetcher1.GetData(new Uri(fullUrl)));
            Assert.Contains("task was canceled", ex.Message);
            Assert.NotNull(result);
        }
    }
}
