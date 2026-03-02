// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Markets;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.OutMarket;

public class MarketBetSettlementTests
{
    private static readonly Urn AnySportId = Urn.Parse("sr:sport:5");
    private const int AnyProducerId = 1;
    private const int AnyMarketId = 213;

    [Fact]
    public void OutcomeResultsAreMappedCorrectlyForPartialSettlement()
    {
        var marketFactory = MarketFactoryBuilder.BuilderStubbingOutSportEventAndCaches
                                           .StubbingOutCaches()
                                           .Build();

        const int wonOutcomeId = 70;
        const int lostOutcomeId = 71;
        const int undecidedYetOutcomeId = 72;
        var betSettlementWithOutcomes = BetSettlementMarketBuilder.Create()
                                                                  .WithId(AnyMarketId)
                                                                  .AddOutcome(wonOutcomeId, 1)
                                                                  .AddOutcome(lostOutcomeId, 0)
                                                                  .AddOutcome(undecidedYetOutcomeId, -1)
                                                                  .Build();

        var sportEvent = new Mock<ISportEvent>().Object;
        IReadOnlyCollection<CultureInfo> cultures = [TestData.Culture];

        var market = marketFactory.GetMarketWithResults(sportEvent, betSettlementWithOutcomes, AnyProducerId, AnySportId, cultures);

        market.OutcomeSettlements.ShouldNotBeNull();
        market.OutcomeSettlements.ShouldBeOfSize(3);
        market.OutcomeSettlements.First(x => x.Id == wonOutcomeId.ToString()).OutcomeResult.ShouldBe(OutcomeResult.Won);
        market.OutcomeSettlements.First(x => x.Id == lostOutcomeId.ToString()).OutcomeResult.ShouldBe(OutcomeResult.Lost);
        market.OutcomeSettlements.First(x => x.Id == undecidedYetOutcomeId.ToString()).OutcomeResult.ShouldBe(OutcomeResult.UndecidedYet);
    }

    [Fact]
    public void UnexpectedValuesInOutcomesResultsMapsToUnsupportedBySdk()
    {
        var marketFactory = MarketFactoryBuilder.BuilderStubbingOutSportEventAndCaches
                                           .StubbingOutCaches()
                                           .Build();

        const int outcomeId = 70;
        const int unexpectedOutcomeResult = 999;
        var betSettlementWithOutcomes = BetSettlementMarketBuilder.Create()
                                                                  .WithId(AnyMarketId)
                                                                  .AddOutcome(outcomeId, unexpectedOutcomeResult)
                                                                  .Build();

        var sportEvent = new Mock<ISportEvent>().Object;
        IReadOnlyCollection<CultureInfo> cultures = [TestData.Culture];

        var market = marketFactory.GetMarketWithResults(sportEvent, betSettlementWithOutcomes, AnyProducerId, AnySportId, cultures);

        market.OutcomeSettlements.ShouldNotBeNull();
        market.OutcomeSettlements.ShouldHaveSingleItem();
        market.OutcomeSettlements.First(x => x.Id == outcomeId.ToString()).OutcomeResult.ShouldBe(OutcomeResult.UnsupportedBySdk);
    }
}
