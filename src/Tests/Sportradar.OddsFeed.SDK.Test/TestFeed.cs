using System;
using System.Collections.Generic;
using System.Text;
using App.Metrics;
using Microsoft.Extensions.Logging;
using Moq;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;
using Unity;

namespace Sportradar.OddsFeed.SDK.Test
{
    internal class TestFeed : Feed
    {
        private readonly IDataRouterManager _dataRouterManager;
        private readonly IProducersProvider _producersProvider;

        /// <inheritdoc />
        protected TestFeed(IDataRouterManager dataRouterManager, IProducersProvider producersProvider, IOddsFeedConfiguration config, bool isReplay, ILoggerFactory loggerFactory = null, IMetricsRoot metricsRoot = null)
            : base(config, isReplay, loggerFactory, metricsRoot)
        {
            var bookmakerDetailsProviderMock = new Mock<BookmakerDetailsProvider>("bookmakerDetailsUriFormat",
                                                                                  new TestDataFetcher(),
                                                                                  new Deserializer<bookmaker_details>(),
                                                                                  new BookmakerDetailsMapperFactory());
            bookmakerDetailsProviderMock.Setup(x => x.GetData(It.IsAny<string>())).Returns(TestConfigurationInternal.GetBookmakerDetails());
            var defaultBookmakerDetailsProvider = bookmakerDetailsProviderMock.Object;

            InternalConfig = new OddsFeedConfigurationInternal(config, defaultBookmakerDetailsProvider);

            _dataRouterManager = dataRouterManager;
            _producersProvider = producersProvider;

            //InitFeed(dataRouterManager, producersProvider);
        }

        /// <inheritdoc />
        public TestFeed(IDataRouterManager dataRouterManager, IProducersProvider producersProvider, IOddsFeedConfiguration config, ILoggerFactory loggerFactory = null, IMetricsRoot metricsRoot = null)
            : this(dataRouterManager, producersProvider, config, false, loggerFactory, metricsRoot)
        {
        }

        /// <inheritdoc />
        protected override void UpdateDependency()
        {
            base.UpdateDependency();
            UnityContainer.RegisterInstance<IDataRouterManager>(_dataRouterManager);
            UnityContainer.RegisterInstance<IProducersProvider>(_producersProvider);
        }

        //protected void InitFeed(IDataRouterManager dataRouterManager, IProducersProvider producersProvider)
        //{
        //    base.InitFeed();

        //    if (FeedInitialized)
        //    {
        //        return;
        //    }

        //    InternalConfig.Load(); // loads bookmaker_details
        //    if (InternalConfig.BookmakerDetails == null)
        //    {
        //        _log.LogError("Token not accepted.");
        //        return;
        //    }
        //    UnityContainer.RegisterTypes(this, InternalConfig);
        //    UnityContainer.RegisterAdditionalTypes();

        //    _feedRecoveryManager = UnityContainer.Resolve<IFeedRecoveryManager>();
        //    _connectionValidator = UnityContainer.Resolve<ConnectionValidator>();

        //    FeedInitialized = true;

        //    UnityContainer.RegisterInstance<IDataRouterManager>(dataRouterManager);
        //    UnityContainer.RegisterInstance<IProducersProvider>(producersProvider);
        //}
    }
}
