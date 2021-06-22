/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;
using Unity.Lifetime;
using Unity.Injection;
using Unity.Resolution;

// ReSharper disable RedundantTypeArgumentsOfMethod

namespace Sportradar.OddsFeed.SDK.API.Test
{
    [TestClass]
    public class UnityFeedBootstrapTest
    {
        /// <summary>
        /// Mocked dispatcher must be assigned to this variable so the GC does not collect it
        /// since the container only holds a weak reference to it (ExternallyControlledLifetimeManager)
        /// </summary>
        private static IGlobalEventDispatcher _dispatcher;

        private static IUnityContainer _childContainer1;

        private static IUnityContainer _childContainer2;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            IUnityContainer container = new UnityContainer().EnableDiagnostic();
            var config = TestConfigurationInternal.GetConfig();
            _dispatcher = new Mock<IGlobalEventDispatcher>().Object;
            
            container.RegisterBaseTypes(config, null, null);

            // we need to override initial loading of bookmaker details and producers
            var bookmakerDetailsProviderMock = new Mock<BookmakerDetailsProvider>("bookmakerDetailsUriFormat",
                new TestDataFetcher(),
                new Deserializer<bookmaker_details>(),
                new BookmakerDetailsMapperFactory());
            bookmakerDetailsProviderMock.Setup(x => x.GetData(It.IsAny<string>())).Returns(TestConfigurationInternal.GetBookmakerDetails());
            var defaultBookmakerDetailsProvider = bookmakerDetailsProviderMock.Object;
            container.RegisterInstance<IDataProvider<BookmakerDetailsDTO>>(defaultBookmakerDetailsProvider, new ContainerControlledLifetimeManager());
            container.RegisterInstance<BookmakerDetailsProvider>(defaultBookmakerDetailsProvider, new ContainerControlledLifetimeManager());

            var newConfig = new OddsFeedConfigurationInternal(config, defaultBookmakerDetailsProvider);
            newConfig.Load();
            container.RegisterInstance<IOddsFeedConfiguration>(newConfig, new ContainerControlledLifetimeManager());
            container.RegisterInstance<IOddsFeedConfigurationInternal>(newConfig, new ContainerControlledLifetimeManager());
            
            container.RegisterTypes(_dispatcher, config);
            
            //override
            container.RegisterType<IProducerManager, ProducerManager>(new ContainerControlledLifetimeManager(),
                                                                      new InjectionConstructor(new TestProducersProvider(), config));

            container.RegisterAdditionalTypes();

            _childContainer1 = container.CreateChildContainer();
            _childContainer2 = container.CreateChildContainer();
        }

        [TestMethod]
        public void configuration_is_resolved()
        {
            var config = _childContainer1.Resolve<IOddsFeedConfigurationInternal>();
            Assert.IsTrue(!string.IsNullOrEmpty(config.ApiBaseUri));
            Assert.IsTrue(!string.IsNullOrEmpty(config.ApiHost));
            Assert.IsTrue(!string.IsNullOrEmpty(config.Host));
            Assert.IsTrue(!string.IsNullOrEmpty(config.VirtualHost));
            Assert.IsTrue(!string.IsNullOrEmpty(config.ReplayApiHost));
        }

        //[TestMethod]
        //public void OddsFeedIsConstructedSuccessfully()
        //{
        //    // hard one; Feed ctor registers types again
        //    var config = _childContainer1.Resolve<IOddsFeedConfiguration>();
        //    var feed = new Feed(config);

        //    Assert.IsNotNull(feed);
        //    Assert.IsNotNull(feed.EventRecoveryRequestIssuer);
        //    Assert.IsNotNull(feed.SportDataProvider);
        //}

        [TestMethod]
        public void each_session_gets_different_instance_of_CacheMessageProcessor()
        {
            var statusCache1 = _childContainer1.Resolve<IFeedMessageProcessor>("CacheMessageProcessor");
            Assert.IsNotNull(statusCache1, "Resolved CacheMessageProcessor cannot be a null reference");
            Assert.IsInstanceOfType(statusCache1, typeof(CacheMessageProcessor), "statusCache1 must be instance of CacheMessageProcessor");

            var statusCache2 = _childContainer2.Resolve<IFeedMessageProcessor>("CacheMessageProcessor");
            Assert.AreNotEqual(statusCache1, statusCache2, "CacheMessageProcessor instances resolved on different containers must not be equal");
        }

        [TestMethod]
        public void Each_session_gets_same_instance_of_FeedMessageHandler()
        {
            var messageHandler1 = _childContainer1.Resolve<IFeedMessageHandler>();
            Assert.IsNotNull(messageHandler1, "Resolved IFeedMessageHandler cannot be a null reference");
            Assert.IsInstanceOfType(messageHandler1, typeof(FeedMessageHandler), "messageHandler1 must be instance of FeedMessageHandler");

            var messageHandler2 = _childContainer2.Resolve<IFeedMessageHandler>();
            Assert.AreEqual(messageHandler1, messageHandler2, "IFeedMessageHandler instances resolved on different containers must be equal");
        }

        [TestMethod]
        public void CompositeMessageProcessorIsResolvedCorrectly()
        {
            var processor1 = _childContainer1.Resolve<CompositeMessageProcessor>();
            Assert.IsNotNull(processor1, "Resolved IFeedMessageProcessor cannot be a null reference");
            Assert.IsInstanceOfType(processor1, typeof(CompositeMessageProcessor), "Resolved IFeedMessageProcessor must be instance of CompositeMessageProcessor");

            var processor12 = _childContainer1.Resolve<CompositeMessageProcessor>();
            Assert.AreEqual(processor1, processor12, "IFeedMessageProcessor instances resolved on the same container must be equal");

            var processor2 = _childContainer2.Resolve<CompositeMessageProcessor>();
            Assert.AreNotEqual(processor1, processor2, "IOddsFeedMessageProcessor instances resolved on different containers must not be equal");
        }

        [TestMethod]
        public void SportEventFactoryIsResolvedProperly()
        {
            var cache = _childContainer1.Resolve<ISportEntityFactory>();
            Assert.IsNotNull(cache, "Resolved ISportEntityFactory cannot be a null reference");
            Assert.IsInstanceOfType(cache, typeof(SportEntityFactory), "Resolved ISportEventFactory must be instance of SportEntityFactory");

            var cache1 = _childContainer2.Resolve<ISportEntityFactory>();
            Assert.AreEqual(cache, cache1, "ISportEntityFactory instances resolved on different containers must be equal");
        }

        [TestMethod]
        public void MessageMapperIsResolvedProperly()
        {
            var cache = _childContainer1.Resolve<IFeedMessageMapper>();
            Assert.IsNotNull(cache, "Resolved IFeedMessageMapper cannot be a null reference");
            Assert.IsInstanceOfType(cache, typeof(FeedMessageMapper), "Resolved FeedMessageMapper must be instance of SportEventFactory");

            var cache1 = _childContainer2.Resolve<IFeedMessageMapper>();
            Assert.AreEqual(cache, cache1, "IFeedMessageMapper instances resolved on different containers must be equal");
        }

        [TestMethod]
        public void OddsFeedSessionResolvedProperly()
        {
            var session = _childContainer1.Resolve<IOddsFeedSession>(new ParameterOverride("messageInterest", MessageInterest.HighPriorityMessages));
            Assert.IsNotNull(session, "Resolved IOddsFeedSession cannot be a null reference");
            Assert.IsInstanceOfType(session, typeof(OddsFeedSession), "Resolved session must be instance of OddsFeedSession");

            var session1 = _childContainer2.Resolve<IOddsFeedSession>(new ParameterOverride("messageInterest", MessageInterest.LowPriorityMessages));
            Assert.AreNotEqual(session, session1, "IFeedMessageMapper instances resolved on different containers must be equal");
        }

        [TestMethod]
        public void Something()
        {
            var provider = _childContainer1.Resolve<IDataProvider<EntityList<MarketDescriptionDTO>>>();
            Assert.IsNotNull(provider);

            var invariantCache = _childContainer1.Resolve<IMarketDescriptionCache>("InvariantMarketDescriptionsCache");
            Assert.IsNotNull(invariantCache);
            Assert.IsInstanceOfType(invariantCache, typeof(InvariantMarketDescriptionCache));

            var variantCache = _childContainer1.Resolve<IMarketDescriptionCache>("VariantMarketDescriptionCache");
            Assert.IsNotNull(variantCache);
            Assert.IsInstanceOfType(variantCache, typeof(VariantMarketDescriptionCache));

            var cacheSelector = _childContainer1.Resolve<IMarketCacheProvider>();
            Assert.IsNotNull(cacheSelector);
            Assert.IsInstanceOfType(cacheSelector, typeof(MarketCacheProvider));

            var profileCache = _childContainer1.Resolve<IProfileCache>(); 
            Assert.IsNotNull(profileCache);
            Assert.IsInstanceOfType(profileCache, typeof(ProfileCache));

            var expressionFactory = _childContainer1.Resolve<INameExpressionFactory>();
            Assert.IsNotNull(expressionFactory);

            var nameProviderFactory = _childContainer1.Resolve<INameProviderFactory>();
            Assert.IsNotNull(nameProviderFactory);
        }

        [TestMethod]
        public void BookmakerDetailsProviderIsResolvedProperly()
        {
            var fetcher = _childContainer1.Resolve<BookmakerDetailsProvider>();
            Assert.IsNotNull(fetcher, "Resolved BookmakerDetailsProvider cannot be a null reference");
            Assert.IsInstanceOfType(fetcher, typeof(BookmakerDetailsProvider), "Resolved BookmakerDetailsProvider must be instance of BookmakerDetailsProvider");

            var fetcher1 = _childContainer2.Resolve<BookmakerDetailsProvider>();
            Assert.AreEqual(fetcher, fetcher1, "BookmakerDetailsProvider instances resolved on different containers must be equal");
        }

        //TODO: check this
        //[TestMethod]
        //public void SdkStatisticsWriterIsResolvedProperly()
        //{
        //    var stats1 = _childContainer1.Resolve<MetricsReporter>();
        //    Assert.IsNotNull(stats1, "Resolved //MetricsReporter cannot be a null reference");
        //    Assert.IsInstanceOfType(stats1, typeof(MetricsReporter), "Resolved SdkStats must be instance of //MetricsReporter");

        //    var stats2 = _childContainer2.Resolve<MetricsReporter>();
        //    Assert.AreEqual(stats1, stats2, "MetricsReporter instances resolved on different containers must be equal");
        //}

        [TestMethod]
        public void FeedSystemSessionIsResolvedProperly()
        {
            var stats1 = _childContainer1.Resolve<FeedSystemSession>();
            Assert.IsNotNull(stats1, "Resolved FeedSystemSession cannot be a null reference");
            Assert.IsInstanceOfType(stats1, typeof(FeedSystemSession), "Resolved FeedSystemSession must be instance of FeedSystemSession");

            var stats2 = _childContainer2.Resolve<FeedSystemSession>();
            Assert.AreEqual(stats1, stats2, "FeedSystemSession instances resolved on different containers must be equal");
        }

        [TestMethod]
        public void FeedRecoveryManagerIsResolvedProperly()
        {
            var stats1 = _childContainer1.Resolve<IFeedRecoveryManager>();
            Assert.IsNotNull(stats1, "Resolved FeedRecoveryManager cannot be a null reference");
            Assert.IsInstanceOfType(stats1, typeof(FeedRecoveryManager), "Resolved FeedRecoveryManager must be instance of FeedRecoveryManager");

            var stats2 = _childContainer2.Resolve<IFeedRecoveryManager>();
            Assert.AreEqual(stats1, stats2, "FeedRecoveryManager instances resolved on different containers must be equal");
        }

        [TestMethod]
        public void SessionMessageManagerIsResolvedProperly()
        {
            var stats1 = _childContainer1.Resolve<IFeedMessageProcessor>("SessionMessageManager");
            Assert.IsNotNull(stats1, "Resolved SessionMessageManager cannot be a null reference");
            Assert.IsInstanceOfType(stats1, typeof(IFeedMessageProcessor), "Resolved SessionMessageManager must be instance of IFeedMessageProcessor");
            Assert.IsInstanceOfType(stats1, typeof(ISessionMessageManager), "Resolved SessionMessageManager must be instance of ISessionMessageManager");

            var stats2 = _childContainer2.Resolve<IFeedMessageProcessor>("SessionMessageManager");
            Assert.AreNotEqual(stats1, stats2, "SessionMessageManager instances resolved on different containers must not be equal");
        }

        [TestMethod]
        public void DataRouterManagerIsResolvedProperly()
        {
            var stats1 = _childContainer1.Resolve<IDataRouterManager>();
            Assert.IsNotNull(stats1, "Resolved IDataRouterManager cannot be a null reference");
            Assert.IsInstanceOfType(stats1, typeof(DataRouterManager), "Resolved IDataRouterManager must be instance of DataRouterManager");

            var stats2 = _childContainer2.Resolve<IDataRouterManager>();
            Assert.AreEqual(stats1, stats2, "IDataRouterManager instances resolved on different containers must be equal");
        }
    }
}
