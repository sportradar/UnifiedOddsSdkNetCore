// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
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

public class RollbackBetSettlementHeadersTests
{
    private readonly ITestOutputHelper _outputHelper;

    public RollbackBetSettlementHeadersTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task RollbackBetSettlementMessageContainsAmqpHeaders()
    {
        var uofConfig = UofConfigurations.SingleLanguage;

        var stubRabbitChannel = new StubRabbitChannel();
        var loggerFactory = new XunitLoggerFactory(_outputHelper, LogLevel.Debug);

        var rollbackBetSettlement = new rollback_bet_settlement
        {
            product = 1,
            event_id = "sr:match:10237855",
            timestamp = 1710226442742,
            request_id = 1,
            request_idSpecified = true,
            market = new[] { new market { id = 312, specifiers = "setnr=4|pointnr=45" } },
            SentAt = 1710226442742,
            ReceivedAt = 1710226442742,
            SportId = Urn.Parse("sr:sport:23")
        };

        var mockSportEntityFactory = new Mock<ISportEntityFactory>();
        var mockMarketFactory = new Mock<IMarketFactory>();
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
                                      .WithMarketFactory(mockMarketFactory.Object)
                                      .AddSession(MessageInterest.AllMessages, stubRabbitChannel)
                                      .Build();

        var session = feedUnit.GetSession(MessageInterest.AllMessages);

        IRollbackBetSettlement<ISportEvent> receivedRollback = null;
        session.OnRollbackBetSettlement += (_, e) => receivedRollback = e.GetBetSettlementRollback();

        feedUnit.Open();
        await stubRabbitChannel.WaitTillOpened();

        var amqpHeaders = new Dictionary<string, object>
        {
            { "Product", "ODDS_THIRD_PARTIES_PROVIDER|nvenue" },
            { "NULL_HEADER", null },
            { "custom_header", 5 }
        };
        stubRabbitChannel.SendMessage(rollbackBetSettlement, amqpHeaders);

        receivedRollback.ShouldNotBeNull();

        var v2 = receivedRollback as IMessageV2;
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
