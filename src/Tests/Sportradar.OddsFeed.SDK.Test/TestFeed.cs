using App.Metrics;
using Microsoft.Extensions.Logging;
using Moq;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.Extended;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace Sportradar.OddsFeed.SDK.Test
{
    /// <summary>
    /// Class TestFeed through which we can also control what is received from API
    /// Implements the <see cref="FeedExt" />
    /// </summary>
    /// <remarks>Override default classes for accessing API endpoints (DataRouterManager, ProducersProvider, RecoveryDataPoster)</remarks>
    /// <seealso cref="FeedExt" />
    internal class TestFeed : FeedExt
    {
        public readonly IDataRouterManager DataRouterManager;
        public readonly IProducersProvider ProducersProvider;
        public readonly TestDataFetcher RecoveryDataPoster;

        /// <inheritdoc />
        public TestFeed(IDataRouterManager dataRouterManager, IProducersProvider producersProvider, IOddsFeedConfiguration config, ILoggerFactory loggerFactory = null, IMetricsRoot metricsRoot = null)
            : base(config, loggerFactory, metricsRoot)
        {
            var bookmakerDetailsProviderMock = new Mock<BookmakerDetailsProvider>("bookmakerDetailsUriFormat",
                                                                                  new TestDataFetcher(),
                                                                                  new Deserializer<bookmaker_details>(),
                                                                                  new BookmakerDetailsMapperFactory());
            bookmakerDetailsProviderMock.Setup(x => x.GetData(It.IsAny<string>())).Returns(TestConfigurationInternal.GetBookmakerDetails());
            var defaultBookmakerDetailsProvider = bookmakerDetailsProviderMock.Object;

            InternalConfig = new OddsFeedConfigurationInternal(config, defaultBookmakerDetailsProvider);

            DataRouterManager = dataRouterManager;
            ProducersProvider = producersProvider;

            RecoveryDataPoster = new TestDataFetcher();
        }

        /// <inheritdoc />
        protected override void UpdateDependency()
        {
            base.UpdateDependency();
            UnityContainer.RegisterInstance(DataRouterManager);
            UnityContainer.RegisterInstance(ProducersProvider);
            UnityContainer.RegisterType<IRecoveryRequestIssuer, RecoveryRequestIssuer>(
                                                                                  new ContainerControlledLifetimeManager(),
                                                                                  new InjectionConstructor(
                                                                                                           RecoveryDataPoster,
                                                                                                           new ResolvedParameter<ISequenceGenerator>(),
                                                                                                           InternalConfig,
                                                                                                           new ResolvedParameter<IProducerManager>()));
        }
    }
}
