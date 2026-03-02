// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Feed.Messages;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Markets;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.OutMarket;

public class MarketRollbackBetSettlementTests
{
    private const int AnyProducerId = 1;
    private const int AnyMarketId = 213;
    private static readonly Urn AnySportId = Urn.Parse("sr:sport:5");

    [Fact]
    public void MarketWithoutOutcomesIsMapped()
    {
        var sportEvent = new Mock<ISportEvent>().Object;
        IReadOnlyCollection<CultureInfo> cultures = [TestConsts.CultureEn];

        var marketFactory = MarketFactoryBuilder.BuilderStubbingOutSportEventAndCaches
                                           .StubbingOutCaches()
                                           .Build();

        var rollbackBetSettlementMarket = MarketBuilder.Create()
                                                       .WithId(AnyMarketId)
                                                       .Build();

        var market = marketFactory.GetMarketForRollbackSettlement(sportEvent, rollbackBetSettlementMarket, AnyProducerId, AnySportId, cultures);

        market.ShouldNotBeNull();
        market.Id.ShouldBe(AnyMarketId);
        market.Specifiers.ShouldBeNull();
        market.AdditionalInfo.ShouldBeNull();
        market.VoidReason.ShouldBeNull();
    }

    [Fact]
    public void MarketWithOutcomesIsMapped()
    {
        var sportEvent = new Mock<ISportEvent>().Object;
        IReadOnlyCollection<CultureInfo> cultures = [TestConsts.CultureEn];

        var marketFactory = MarketFactoryBuilder.BuilderStubbingOutSportEventAndCaches
                                           .StubbingOutCaches()
                                           .Build();

        var rollbackBetSettlementMarket = MarketBuilder.Create()
                                                       .WithId(AnyMarketId)
                                                       .WithSpecifiers("turn=1")
                                                       .AddOutcome("1")
                                                       .AddOutcome("2")
                                                       .Build();

        var market = marketFactory.GetMarketForRollbackSettlement(sportEvent, rollbackBetSettlementMarket, AnyProducerId, AnySportId, cultures);

        market.ShouldNotBeNull();
        market.ShouldBeAssignableTo<IMarketRollbackSettlement>();
        var marketRollbackSettlement = (MarketRollbackSettlement)market;
        marketRollbackSettlement.Id.ShouldBe(AnyMarketId);
        marketRollbackSettlement.Specifiers.ShouldNotBeNull();
        marketRollbackSettlement.Specifiers.ShouldHaveSingleItem();
        marketRollbackSettlement.Specifiers.Single().ShouldBe(new KeyValuePair<string, string>("turn", "1"));
        marketRollbackSettlement.AdditionalInfo.ShouldBeNull();
        marketRollbackSettlement.VoidReason.ShouldBeNull();
        marketRollbackSettlement.Outcomes.ShouldNotBeNull();
        marketRollbackSettlement.Outcomes.ShouldBeOfSize(2);
        marketRollbackSettlement.Outcomes.First().Id.ShouldBe("1");
        marketRollbackSettlement.Outcomes.Skip(1).First().Id.ShouldBe("2");
    }

    [Fact]
    public void MarketWithExtendedSpecifiersAreMapped()
    {
        var sportEvent = new Mock<ISportEvent>().Object;
        IReadOnlyCollection<CultureInfo> cultures = [TestConsts.CultureEn];

        var marketFactory = MarketFactoryBuilder.BuilderStubbingOutSportEventAndCaches
                                           .StubbingOutCaches()
                                           .Build();

        var rollbackBetSettlementMarket = MarketBuilder.Create()
                                                       .WithId(AnyMarketId)
                                                       .WithSpecifiers("turn=1")
                                                       .WithExtendedSpecifiers("anything=2")
                                                       .Build();

        var market = marketFactory.GetMarketForRollbackSettlement(sportEvent, rollbackBetSettlementMarket, AnyProducerId, AnySportId, cultures);

        market.ShouldNotBeNull();
        market.ShouldBeAssignableTo<IMarketRollbackSettlement>();
        var marketRollbackSettlement = (MarketRollbackSettlement)market;
        marketRollbackSettlement.Id.ShouldBe(AnyMarketId);
        marketRollbackSettlement.Specifiers.ShouldNotBeNull();
        marketRollbackSettlement.Specifiers.ShouldHaveSingleItem();
        marketRollbackSettlement.Specifiers.Single().ShouldBe(new KeyValuePair<string, string>("turn", "1"));
        marketRollbackSettlement.AdditionalInfo.ShouldNotBeNull();
        marketRollbackSettlement.AdditionalInfo.ShouldHaveSingleItem();
        marketRollbackSettlement.AdditionalInfo.Single().ShouldBe(new KeyValuePair<string, string>("anything", "2"));
        marketRollbackSettlement.VoidReason.ShouldBeNull();
        marketRollbackSettlement.Outcomes.ShouldBeNull();
    }

    [Fact]
    public void MarketWithVoidReasonIsMapped()
    {
        const int voidReasonId = 1;
        const string voidReasonDescription = "void reason description";

        var sportEvent = new Mock<ISportEvent>().Object;
        IReadOnlyCollection<CultureInfo> cultures = [TestConsts.CultureEn];

        var mockVoidReasonCache = new Mock<INamedValueCache>();
        mockVoidReasonCache.Setup(s => s.GetNamedValue(1)).Returns(new NamedValue(voidReasonId, voidReasonDescription));

        var marketFactory = MarketFactoryBuilder.BuilderStubbingOutSportEventAndCaches
                                           .StubbingOutCaches()
                                           .WithVoidReasonCache(mockVoidReasonCache.Object)
                                           .Build();

        var rollbackBetSettlementMarket = MarketBuilder.Create()
                                                       .WithId(AnyMarketId)
                                                       .WithSpecifiers("turn=1")
                                                       .WithVoidReason(voidReasonId)
                                                       .Build();

        var market = marketFactory.GetMarketForRollbackSettlement(sportEvent, rollbackBetSettlementMarket, AnyProducerId, AnySportId, cultures);

        market.ShouldNotBeNull();
        market.ShouldBeAssignableTo<IMarketRollbackSettlement>();
        var marketRollbackSettlement = (MarketRollbackSettlement)market;
        marketRollbackSettlement.Id.ShouldBe(AnyMarketId);
        marketRollbackSettlement.Specifiers.ShouldNotBeNull();
        marketRollbackSettlement.Specifiers.ShouldHaveSingleItem();
        marketRollbackSettlement.Specifiers.Single().ShouldBe(new KeyValuePair<string, string>("turn", "1"));
        marketRollbackSettlement.AdditionalInfo.ShouldBeNull();
        marketRollbackSettlement.VoidReason.ShouldNotBeNull();
        marketRollbackSettlement.VoidReason.Id.ShouldBe(voidReasonId);
        marketRollbackSettlement.VoidReason.Description.ShouldBe(voidReasonDescription);
        marketRollbackSettlement.Outcomes.ShouldBeNull();
    }
}
