// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Caching;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.CriticalUnit;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Markets;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Feed.Messages;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Sdk.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.CriticalUnit;

public class OddsChangeHeadersTests
{
    private readonly ITestOutputHelper _outputHelper;

    public OddsChangeHeadersTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task OddsChangeMessageContainsAmqpHeaders()
    {
        var uofConfig = UofConfigurations.SingleLanguage;

        var stubRabbitChannel = new StubRabbitChannel();
        var loggerFactory = new XunitLoggerFactory(_outputHelper, LogLevel.Debug);

        var oddsChange = OddsChanges.GetWithSingleMarketAndSpecifier();
        var apiMarket = oddsChange.odds.market.First();
        var apiMarketDescription = MarketDescriptionBuilder
                                  .Create(apiMarket.id, "Market 1")
                                  .WithGroups("all|score|set")
                                  .AddOutcome(o => o.WithId(apiMarket.outcome.First().id).WithName($"Outcome {apiMarket.outcome.First().id}"))
                                  .AddOutcome(o => o.WithId(apiMarket.outcome.Last().id).WithName($"Outcome {apiMarket.outcome.Last().id}"))
                                  .AddSpecifier("turn", "integer")
                                  .Build();
        var marketDescription = apiMarketDescription.ToUserMarketDescription(TestConsts.CultureEn);

        var sportEvent = new Mock<ISportEvent>();
        var mockMarketWithOdds = new Mock<IMarketWithOdds>();
        var mockSportEventStatusCache = new Mock<ISportEventStatusCache>();
        var marketCacheProvider = MockMarketCacheProviderBuilder.Create().WithReturning(marketDescription).Build();
        var cacheManager = MockCacheManagerBuilder.Create().WithSaveDtoForSportEventStatus(oddsChange, TestConsts.CultureEn).Build();

        var mockSportEntityFactory = new Mock<ISportEntityFactory>();
        mockSportEntityFactory.Setup(sef => sef.BuildSportEvent<ISportEvent>(sportEvent.Object.Id,
                                                                                  It.IsAny<Urn>(),
                                                                                  uofConfig.Languages,
                                                                                  uofConfig.ExceptionHandlingStrategy))
                                   .Returns(sportEvent.Object);

        var mockMarketFactory = new Mock<IMarketFactory>();
        mockMarketFactory.Setup(mf => mf.GetMarketWithOdds(sportEvent.Object, oddsChange.odds.market.First(), It.IsAny<int>(), It.IsAny<Urn>(), uofConfig.Languages))
                         .Returns(mockMarketWithOdds.Object);

        var feedUnit = FeedUnitBuilder.Create()
                                      .WithConfiguration(uofConfig)
                                      .WithLoggerFactory(loggerFactory)
                                      .WithProducerManager(new TestProducerManager(uofConfig))
                                      .WithSportEventCache(new Mock<ISportEventCache>().Object)
                                      .WithNamedValuesProvider(new Mock<INamedValuesProvider>().Object)
                                      .WithMarketCacheProvider(marketCacheProvider)
                                      .WithCacheManager(cacheManager)
                                      .WithSportEventStatusCache(mockSportEventStatusCache.Object)
                                      .WithSportEntityFactory(mockSportEntityFactory.Object)
                                      .WithMarketFactory(mockMarketFactory.Object)
                                      .AddSession(MessageInterest.AllMessages, stubRabbitChannel)
                                      .Build();

        var session = feedUnit.GetSession(MessageInterest.AllMessages);

        IOddsChange<ISportEvent> receivedOddsChange = null;
        session.OnOddsChange += (_, e) => receivedOddsChange = e.GetOddsChange();

        feedUnit.Open();
        await stubRabbitChannel.WaitTillOpened();

        var amqpHeaders = new Dictionary<string, object>
        {
            { "Product", "ODDS_THIRD_PARTIES_PROVIDER|nvenue" },
            { "NULL_HEADER", null },
            { "custom_header", 5 },
            { "producer", "some-custom-text-as-byte"u8.ToArray() }
        };
        stubRabbitChannel.SendMessage(oddsChange, amqpHeaders);

        receivedOddsChange.ShouldNotBeNull();

        var v2 = receivedOddsChange as IMessageV2;
        v2.ShouldNotBeNull();

        v2.MessageHeaders.ShouldNotBeNull();
        v2.MessageHeaders.ContainsKey("Product").ShouldBeTrue();
        v2.MessageHeaders["Product"].ShouldBe("ODDS_THIRD_PARTIES_PROVIDER|nvenue");
        v2.MessageHeaders.ContainsKey("NULL_HEADER").ShouldBeTrue();
        v2.MessageHeaders["NULL_HEADER"].ShouldBeNull();
        v2.MessageHeaders.ContainsKey("custom_header").ShouldBeTrue();
        v2.MessageHeaders["custom_header"].ShouldBe(5.ToString());
        v2.MessageHeaders.ContainsKey("producer").ShouldBeTrue();
        v2.MessageHeaders["producer"].ShouldBe("some-custom-text-as-byte");

        feedUnit.Close();
    }

    [Fact]
    public async Task OddsChangeMessageWithoutProductHeaderHasNoCustomMessageHeaders()
    {
        var uofConfig = UofConfigurations.SingleLanguage;

        var stubRabbitChannel = new StubRabbitChannel();
        var loggerFactory = new XunitLoggerFactory(_outputHelper, LogLevel.Debug);

        var oddsChange = OddsChanges.GetWithSingleMarketAndSpecifier();
        var apiMarket = oddsChange.odds.market.First();
        var apiMarketDescription = MarketDescriptionBuilder
                                  .Create(apiMarket.id, "Market 1")
                                  .WithGroups("all|score|set")
                                  .AddOutcome(o => o.WithId(apiMarket.outcome.First().id).WithName($"Outcome {apiMarket.outcome.First().id}"))
                                  .AddOutcome(o => o.WithId(apiMarket.outcome.Last().id).WithName($"Outcome {apiMarket.outcome.Last().id}"))
                                  .AddSpecifier("turn", "integer")
                                  .Build();
        var marketDescription = apiMarketDescription.ToUserMarketDescription(TestConsts.CultureEn);

        var sportEvent = new Mock<ISportEvent>();
        var mockMarketWithOdds = new Mock<IMarketWithOdds>();
        var mockSportEventStatusCache = new Mock<ISportEventStatusCache>();
        var marketCacheProvider = MockMarketCacheProviderBuilder.Create().WithReturning(marketDescription).Build();

        var cacheManager = MockCacheManagerBuilder.Create().WithSaveDtoForSportEventStatus(oddsChange, TestConsts.CultureEn).Build();

        var mockSportEntityFactory = new Mock<ISportEntityFactory>();
        mockSportEntityFactory.Setup(sef => sef.BuildSportEvent<ISportEvent>(sportEvent.Object.Id,
                                                                                  It.IsAny<Urn>(),
                                                                                  uofConfig.Languages,
                                                                                  uofConfig.ExceptionHandlingStrategy))
                                   .Returns(sportEvent.Object);

        var mockMarketFactory = new Mock<IMarketFactory>();
        mockMarketFactory.Setup(mf => mf.GetMarketWithOdds(sportEvent.Object, oddsChange.odds.market.First(), It.IsAny<int>(), It.IsAny<Urn>(), uofConfig.Languages))
                         .Returns(mockMarketWithOdds.Object);

        var feedUnit = FeedUnitBuilder.Create()
                                      .WithConfiguration(uofConfig)
                                      .WithLoggerFactory(loggerFactory)
                                      .WithProducerManager(new TestProducerManager(uofConfig))
                                      .WithSportEventCache(new Mock<ISportEventCache>().Object)
                                      .WithNamedValuesProvider(new Mock<INamedValuesProvider>().Object)
                                      .WithMarketCacheProvider(marketCacheProvider)
                                      .WithCacheManager(cacheManager)
                                      .WithSportEventStatusCache(mockSportEventStatusCache.Object)
                                      .WithSportEntityFactory(mockSportEntityFactory.Object)
                                      .WithMarketFactory(mockMarketFactory.Object)
                                      .AddSession(MessageInterest.AllMessages, stubRabbitChannel)
                                      .Build();

        var session = feedUnit.GetSession(MessageInterest.AllMessages);

        IOddsChange<ISportEvent> receivedOddsChange = null;
        session.OnOddsChange += (_, e) => receivedOddsChange = e.GetOddsChange();

        feedUnit.Open();
        await stubRabbitChannel.WaitTillOpened();

        stubRabbitChannel.SendMessage(oddsChange);

        receivedOddsChange.ShouldNotBeNull();

        var v2 = receivedOddsChange as IMessageV2;
        v2.ShouldNotBeNull();

        v2.MessageHeaders.ShouldNotBeNull();
        v2.MessageHeaders.ShouldHaveSingleItem();
        v2.MessageHeaders.ShouldContainKey("timestamp_in_ms");

        feedUnit.Close();
    }
}
