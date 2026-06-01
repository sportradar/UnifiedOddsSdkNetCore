// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Moq;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.CashOut;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Configuration;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;

internal static class CashOutProbabilitiesProviderBuilderExtensions
{
    public static CashOutProbabilitiesProviderBuilder AddDefaultDependencies(this CashOutProbabilitiesProviderBuilder builder)
    {
        var configuration = BuildConfigurationWithLanguage();

        builder.WithConfiguration(configuration)
               .WithCashOutFetcher(new Mock<IDataFetcher>().Object)
               .WithSportEntityFactory(BuildDefaultSportEntityFactory())
               .WithMarketFactory(new Mock<IMarketFactory>().Object)
               .WithBetstopReasonsCache(BuildDefaultNamedValuesCache())
               .WithBettingStatusCache(BuildDefaultNamedValuesCache());

        return builder;
    }

    public static CashOutProbabilitiesProviderBuilder WithConfigurationWithLanguage(this CashOutProbabilitiesProviderBuilder builder)
    {
        var configuration = BuildConfigurationWithLanguage();
        var producerManager = new Mock<IProducerManager>();
        producerManager.Setup(p => p.GetProducer(It.IsAny<int>())).Returns(new Mock<IProducer>().Object);
        return builder.WithConfiguration(configuration)
                      .WithProducerManager(producerManager.Object);
    }

    private static IUofConfiguration BuildConfigurationWithLanguage()
    {
        var configuration = new Mock<IUofConfiguration>();
        configuration.Setup(c => c.Languages).Returns(new List<CultureInfo> { TestConsts.CultureEn });
        return configuration.Object;
    }

    private static INamedValueCache BuildDefaultNamedValuesCache()
    {
        return new Mock<INamedValueCache>().Object;
    }

    private static ISportEntityFactory BuildDefaultSportEntityFactory()
    {
        var sportEntityFactoryMock = new Mock<ISportEntityFactory>();
        sportEntityFactoryMock.Setup(f => f.BuildSportEvent<ISportEvent>(
                                         It.IsAny<Urn>(),
                                         It.IsAny<Urn>(),
                                         It.IsAny<IReadOnlyCollection<CultureInfo>>(),
                                         It.IsAny<ExceptionHandlingStrategy>()))
                              .Returns(new Mock<ISportEvent>().Object);
        return sportEntityFactoryMock.Object;
    }
}
