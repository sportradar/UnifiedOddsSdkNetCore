// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.CriticalUnit;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.CriticalUnit;

public class BetStopHeadersTests
{
    private readonly ITestOutputHelper _outputHelper;

    public BetStopHeadersTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task BetStopMessageContainsAmqpHeaders()
    {
        var uofConfig = UofConfigurations.SingleLanguage;

        var stubRabbitChannel = new StubRabbitChannel();
        var loggerFactory = new XunitLoggerFactory(_outputHelper, LogLevel.Debug);

        var betStop = new bet_stop
        {
            product = 1,
            event_id = "sr:match:9578495",
            timestamp = 1710226442742,
            groups = "all",
            SportId = Urn.Parse("sr:sport:1"),
            SentAt = 1710226442742,
            ReceivedAt = 1710226442742
        };

        var sportEvent = new Mock<ISportEvent>();

        var mockSportEntityFactory = new Mock<ISportEntityFactory>();
        mockSportEntityFactory.Setup(sef => sef.BuildSportEvent<ISportEvent>(It.IsAny<Urn>(),
                                                                                  It.IsAny<Urn>(),
                                                                                  It.IsAny<IReadOnlyCollection<System.Globalization.CultureInfo>>(),
                                                                                  It.IsAny<ExceptionHandlingStrategy>()))
                                   .Returns(sportEvent.Object);

        var feedUnit = FeedUnitBuilder.Create()
                                      .WithConfiguration(uofConfig)
                                      .WithLoggerFactory(loggerFactory)
                                      .WithProducerManager(new TestProducerManager(uofConfig))
                                      .WithSportEventCache(new Mock<ISportEventCache>().Object)
                                      .WithNamedValuesProvider(new Mock<INamedValuesProvider>().Object)
                                      .WithMarketCacheProvider(new Mock<IMarketCacheProvider>().Object)
                                      .WithCacheManager(new Mock<ICacheManager>().Object)
                                      .WithSportEventStatusCache(new Mock<ISportEventStatusCache>().Object)
                                      .WithSportEntityFactory(mockSportEntityFactory.Object)
                                      .WithMarketFactory(new Mock<IMarketFactory>().Object)
                                      .AddSession(MessageInterest.AllMessages, stubRabbitChannel)
                                      .Build();

        var session = feedUnit.GetSession(MessageInterest.AllMessages);

        IBetStop<ISportEvent> receivedBetStop = null;
        session.OnBetStop += (_, e) => receivedBetStop = e.GetBetStop();

        feedUnit.Open();
        await stubRabbitChannel.WaitTillOpened();

        var amqpHeaders = new Dictionary<string, object>
        {
            { "Product", "ODDS_THIRD_PARTIES_PROVIDER|nvenue" },
            { "NULL_HEADER", null },
            { "custom_header", 5 }
        };
        stubRabbitChannel.SendMessage(betStop, amqpHeaders);

        receivedBetStop.ShouldNotBeNull();

        var v2 = receivedBetStop as IMessageV2;
        v2.ShouldNotBeNull();

        v2.MessageHeaders.ShouldNotBeNull();
        v2.MessageHeaders.ContainsKey("Product").ShouldBeTrue();
        v2.MessageHeaders["Product"].ShouldBe("ODDS_THIRD_PARTIES_PROVIDER|nvenue");
        v2.MessageHeaders.ContainsKey("NULL_HEADER").ShouldBeTrue();
        v2.MessageHeaders["NULL_HEADER"].ShouldBeNull();
        v2.MessageHeaders.ContainsKey("custom_header").ShouldBeTrue();
        v2.MessageHeaders["custom_header"].ShouldBe(5.ToString());

        feedUnit.Close();
    }
}
