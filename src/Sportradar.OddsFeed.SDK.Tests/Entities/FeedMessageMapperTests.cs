// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Enums;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities;

public class FeedMessageMapperTests
{
    private readonly IFeedMessageMapper _mapper;
    private readonly IFeedMessageValidator _validator;
    private readonly IDeserializer<FeedMessage> _deserializer;
    private readonly ICollection<CultureInfo> _cultures = [TestData.Culture];
    private const int AnyIdNumber = 111;

    public FeedMessageMapperTests(ITestOutputHelper outputHelper)
    {
        var nameProviderFactoryMock = new Mock<INameProviderFactory>();
        var nameProviderMock = new Mock<INameProvider>();
        nameProviderFactoryMock.Setup(m => m.BuildNameProvider(It.IsAny<ISportEvent>(), It.IsAny<int>(), It.IsAny<IReadOnlyDictionary<string, string>>())).Returns(nameProviderMock.Object);

        var namedValuesCacheMock = new Mock<INamedValueCache>();
        namedValuesCacheMock.Setup(x => x.IsValueDefined(It.IsAny<int>())).Returns(true);
        namedValuesCacheMock.Setup(x => x.GetNamedValue(It.IsAny<int>())).Returns((int id) => new NamedValue(id, "somevalue"));

        var namedValuesProviderMock = new Mock<INamedValuesProvider>();
        namedValuesProviderMock.Setup(x => x.VoidReasons).Returns(namedValuesCacheMock.Object);
        namedValuesProviderMock.Setup(x => x.BetStopReasons).Returns(namedValuesCacheMock.Object);
        namedValuesProviderMock.Setup(x => x.BettingStatuses).Returns(namedValuesCacheMock.Object);

        var sportEntityFactoryBuilder = new TestSportEntityFactoryBuilder(outputHelper, ScheduleData.Cultures3);

        var marketFactory = new MarketFactory(new Mock<IMarketCacheProvider>().Object,
                                              nameProviderFactoryMock.Object,
                                              new Mock<IMarketMappingProviderFactory>().Object,
                                              namedValuesProviderMock.Object,
                                              namedValuesCacheMock.Object,
                                              ExceptionHandlingStrategy.Throw);

        _mapper = new FeedMessageMapper(sportEntityFactoryBuilder.SportEntityFactory,
                                        marketFactory,
                                        TestProducerManager.Create(),
                                        namedValuesProviderMock.Object,
                                        ExceptionHandlingStrategy.Throw);

        _deserializer = new Deserializer<FeedMessage>();
        _validator = new StubFeedMessageValidator();
    }

    [Fact]
    public void AliveIsMapped()
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "alive.xml");
        var message = _deserializer.Deserialize<alive>(stream);
        TestData.FillMessageTimestamp(message);
        _validator.Validate(message);
        var entity = _mapper.MapAlive(message);
        Assert.NotNull(entity);
    }

    [Fact]
    public void BetSettlementIsMapped()
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "bet_settlement.xml");
        var message = _deserializer.Deserialize<bet_settlement>(stream);
        TestData.FillMessageTimestamp(message);
        message.SportId = Urn.Parse("sr:sport:1000");
        _validator.Validate(message);
        var entity = _mapper.MapBetSettlement<ICompetition>(message, _cultures, null);
        Assert.NotNull(entity);
    }

    [Fact]
    public void BetStopIsMapped()
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "bet_stop.xml");
        var message = _deserializer.Deserialize<bet_stop>(stream);
        TestData.FillMessageTimestamp(message);
        message.SportId = Urn.Parse("sr:sport:1000");
        _validator.Validate(message);
        var entity = _mapper.MapBetStop<ICompetition>(message, _cultures, null);
        Assert.NotNull(entity);
    }

    [Fact]
    public void FixtureChangeIsMapped()
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "fixture_change.xml");
        var message = _deserializer.Deserialize<fixture_change>(stream);
        TestData.FillMessageTimestamp(message);
        message.SportId = Urn.Parse("sr:sport:1000");
        _validator.Validate(message);
        var entity = _mapper.MapFixtureChange<ICompetition>(message, _cultures, null);
        Assert.NotNull(entity);
    }

    [Fact]
    public void OddsChangeIsMapped()
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "odds_change.xml");
        var message = _deserializer.Deserialize<odds_change>(stream);
        TestData.FillMessageTimestamp(message);
        message.SportId = Urn.Parse("sr:sport:1000");
        _validator.Validate(message);
        var entity = _mapper.MapOddsChange<ICompetition>(message, _cultures, null);
        Assert.NotNull(entity);
    }

    [Fact]
    public void ProbabilityIsMapped()
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "probabilities.xml");
        var message = _deserializer.Deserialize<cashout>(stream);
        TestData.FillMessageTimestamp(message);
        message.SportId = Urn.Parse("sr:sport:1000");
        var entity = _mapper.MapCashOutProbabilities<ICompetition>(message, _cultures, null);
        Assert.NotNull(entity);
    }

    [Fact]
    public void OddsOnProbabilityCanBeOmitted()
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "probabilities.xml");
        var message = _deserializer.Deserialize<cashout>(stream);
        TestData.FillMessageTimestamp(message);
        message.SportId = UrnCreate.SportId(1000);
        message.odds = null;
        var entity = _mapper.MapCashOutProbabilities<ICompetition>(message, _cultures, null);
        Assert.NotNull(entity);
        Assert.Null(entity.Markets);
        Assert.Null(entity.BetStopReason);
        Assert.Null(entity.BettingStatus);
    }

    [Fact]
    public void OptionalFieldsOnProbabilitiesCanBeOmitted()
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "probabilities.xml");
        var message = _deserializer.Deserialize<cashout>(stream);
        TestData.FillMessageTimestamp(message);
        message.SportId = UrnCreate.SportId(1000);
        message.odds.betstop_reasonSpecified = false;
        message.odds.betting_statusSpecified = false;
        var entity = _mapper.MapCashOutProbabilities<ICompetition>(message, _cultures, null);
        Assert.NotNull(entity);
        Assert.NotNull(entity.Markets);
        Assert.Null(entity.BetStopReason);
        Assert.Null(entity.BettingStatus);
    }

    [Fact]
    public void OptionalFieldsOnProbabilitiesAreMapped()
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "probabilities.xml");
        var message = _deserializer.Deserialize<cashout>(stream);
        TestData.FillMessageTimestamp(message);
        message.SportId = UrnCreate.SportId(1000);
        message.odds.betstop_reasonSpecified = true;
        message.odds.betstop_reason = 2;
        message.odds.betting_statusSpecified = true;
        message.odds.betting_status = 3;
        var entity = _mapper.MapCashOutProbabilities<ICompetition>(message, _cultures, null);
        Assert.NotNull(entity);
        Assert.NotNull(entity.Markets);
        Assert.NotNull(entity.BetStopReason);
        Assert.Equal(2, entity.BetStopReason.Id);
        Assert.NotNull(entity.BettingStatus);
        Assert.Equal(3, entity.BettingStatus.Id);
    }

    [Fact]
    public void ProbabilityWithMarketWithCashoutStatusIsMapped()
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "probabilities.xml");
        var message = _deserializer.Deserialize<cashout>(stream);
        TestData.FillMessageTimestamp(message);
        message.SportId = UrnCreate.SportId(1000);
        var entity = _mapper.MapCashOutProbabilities<ICompetition>(message, _cultures, null);
        Assert.NotNull(entity);
        Assert.NotNull(entity.Markets);
        Assert.Equal(1, entity.Markets.Count(w => w.CashoutStatus != null));
        var marketWithCashoutStatus = entity.Markets.First(w => w.CashoutStatus != null);
        Assert.Equal(CashoutStatus.Available, marketWithCashoutStatus.CashoutStatus);
    }

    [Fact]
    public void ProbabilityWithMarketWithPlayerOutcomesIsMappedToOutcomeProbabilities()
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "probabilities.xml");
        var message = _deserializer.Deserialize<cashout>(stream);
        TestData.FillMessageTimestamp(message);
        message.SportId = UrnCreate.SportId(1000);
        var oddsChangeMarket = message.odds.market.First(w => w.id == 11);
        oddsChangeMarket.outcome.First().team = 1;
        oddsChangeMarket.outcome.First().teamSpecified = true;
        oddsChangeMarket.outcome.Skip(1).First().team = 2;
        oddsChangeMarket.outcome.Skip(1).First().teamSpecified = true;

        var entity = _mapper.MapCashOutProbabilities<ICompetition>(message, _cultures, null);

        entity.ShouldNotBeNull();
        entity.Markets.ShouldNotBeNull();
        entity.Markets.ShouldBeOfSize(129);
        var marketWithPlayerOutcomes = entity.Markets.FirstOrDefault(m => m.Id == 11);
        marketWithPlayerOutcomes.ShouldNotBeNull();
        marketWithPlayerOutcomes.OutcomeProbabilities.ShouldNotBeNull();
        marketWithPlayerOutcomes.OutcomeProbabilities.ShouldBeOfSize(2);
        marketWithPlayerOutcomes.OutcomeProbabilities.First().ShouldBeAssignableTo<IOutcomeProbabilities>();
    }

    [Fact]
    public void ProbabilityWithMarketWithAdditionalProbabilitiesIsMapped()
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "probabilities.xml");
        var message = _deserializer.Deserialize<cashout>(stream);
        TestData.FillMessageTimestamp(message);
        message.SportId = UrnCreate.SportId(1000);
        var oddsChangeMarket = message.odds.market.First(w => w.id == 11);
        oddsChangeMarket.outcome.First().half_win_probabilities = 1.1;
        oddsChangeMarket.outcome.First().half_win_probabilitiesSpecified = true;
        oddsChangeMarket.outcome.Skip(1).First().half_win_probabilities = 2;
        oddsChangeMarket.outcome.Skip(1).First().half_win_probabilitiesSpecified = true;

        var entity = _mapper.MapCashOutProbabilities<ICompetition>(message, _cultures, null);

        entity.ShouldNotBeNull();
        entity.Markets.ShouldNotBeNull();
        entity.Markets.ShouldBeOfSize(129);
        var marketWithPlayerOutcomes = entity.Markets.FirstOrDefault(m => m.Id == 11);
        marketWithPlayerOutcomes.ShouldNotBeNull();
        marketWithPlayerOutcomes.OutcomeProbabilities.First().AdditionalProbabilities.ShouldNotBeNull();
        marketWithPlayerOutcomes.OutcomeProbabilities.First().AdditionalProbabilities.HalfWin.ShouldBe(oddsChangeMarket.outcome.First().half_win_probabilities);
        marketWithPlayerOutcomes.OutcomeProbabilities.Skip(1).First().AdditionalProbabilities.HalfWin.ShouldBe(oddsChangeMarket.outcome.Skip(1).First().half_win_probabilities);
    }

    [Fact]
    public void OddsChangeWithMarketWithCashoutStatusIsMapped()
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "odds_change.xml");
        var message = _deserializer.Deserialize<odds_change>(stream);
        TestData.FillMessageTimestamp(message);
        message.SportId = UrnCreate.SportId(1000);
        _validator.Validate(message);
        var entity = _mapper.MapOddsChange<ICompetition>(message, _cultures, null);
        Assert.NotNull(entity);
        Assert.NotNull(entity.Markets);
        Assert.Equal(1, entity.Markets.Count(w => w.CashoutStatus != null));
    }

    [Fact]
    public void OddsChangeWithMarketWithPlayerOutcomesIsMappedToPlayerOutcomeOdds()
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "odds_change.xml");
        var message = _deserializer.Deserialize<odds_change>(stream);
        TestData.FillMessageTimestamp(message);
        message.SportId = UrnCreate.SportId(1000);
        var oddsChangeMarket = message.odds.market.First(w => w.id == 26);
        oddsChangeMarket.outcome.First().team = 1;
        oddsChangeMarket.outcome.First().teamSpecified = true;
        oddsChangeMarket.outcome.Skip(1).First().team = 2;
        oddsChangeMarket.outcome.Skip(1).First().teamSpecified = true;

        _validator.Validate(message);
        var entity = _mapper.MapOddsChange<ICompetition>(message, _cultures, null);

        entity.ShouldNotBeNull();
        entity.Markets.ShouldNotBeNull();
        entity.Markets.ShouldBeOfSize(42);
        var marketWithPlayerOutcomes = entity.Markets.FirstOrDefault(m => m.Id == 26);
        marketWithPlayerOutcomes.ShouldNotBeNull();
        marketWithPlayerOutcomes.OutcomeOdds.ShouldNotBeNull();
        marketWithPlayerOutcomes.OutcomeOdds.ShouldBeOfSize(2);
        marketWithPlayerOutcomes.OutcomeOdds.First().ShouldBeAssignableTo<IPlayerOutcomeOdds>();
        var firstOutcome = marketWithPlayerOutcomes.OutcomeOdds.First() as IPlayerOutcomeOdds;
        firstOutcome.ShouldNotBeNull();
        firstOutcome.HomeOrAwayTeam.ShouldBe(HomeAway.Home);
    }

    [Fact]
    public void WhenOddsChangeIsMissingItThrows()
    {
        Should.Throw<ArgumentNullException>(() => _mapper.MapOddsChange<ICompetition>(null, _cultures, null));
    }

    [Fact]
    public void OddsChangeEventTypeForMatchIsMapped()
    {
        var eventUrn = UrnCreate.MatchId(AnyIdNumber);

        TestMessageEventGeneration<IMatch>(eventUrn);
    }

    [Fact]
    public void OddsChangeEventTypeForStageIsMapped()
    {
        var eventUrn = UrnCreate.StageId(AnyIdNumber);

        TestMessageEventGeneration<IStage>(eventUrn);
    }

    [Fact]
    public void OddsChangeEventTypeForTournamentIsMapped()
    {
        var eventUrn = UrnCreate.TournamentId(AnyIdNumber);

        TestMessageEventGeneration<ITournament>(eventUrn);
    }

    [Fact]
    public void OddsChangeEventTypeForBasicTournamentIsMapped()
    {
        var eventUrn = UrnCreate.SimpleTournamentId(AnyIdNumber);

        TestMessageEventGeneration<IBasicTournament>(eventUrn);
    }

    [Fact]
    public void OddsChangeEventTypeForSeasonIsMapped()
    {
        var eventUrn = UrnCreate.SeasonId(AnyIdNumber);

        TestMessageEventGeneration<ISeason>(eventUrn);
    }

    [Fact]
    public void OddsChangeEventTypeForDrawIsMapped()
    {
        var eventUrn = UrnCreate.DrawId(AnyIdNumber);

        TestMessageEventGeneration<IDraw>(eventUrn);
    }

    [Fact]
    public void OddsChangeEventTypeForLotteryIsMapped()
    {
        var eventUrn = UrnCreate.LotteryId(AnyIdNumber);

        TestMessageEventGeneration<ILottery>(eventUrn);
    }

    [Fact]
    public void OddsChangeEventTypeForUnknownIsMapped()
    {
        var eventUrn = Urn.Parse("sr:unknown:11111");

        TestMessageEventGeneration<ISportEvent>(eventUrn);
    }

    [Fact]
    public void OddsChangeEventTypeForOtherUrnTypeThrows()
    {
        var eventUrn = UrnCreate.PlayerId(AnyIdNumber);

        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "odds_change.xml");
        var message = _deserializer.Deserialize<odds_change>(stream);
        TestData.FillMessageTimestamp(message);
        message.SportId = UrnCreate.SportId(1000);

        message.event_id = eventUrn.ToString();

        _validator.Validate(message);

        var action = () => _mapper.MapOddsChange<ISportEvent>(message, _cultures, null);

        action.ShouldThrow<InvalidOperationException>();
    }

    private void TestMessageEventGeneration<T>(Urn eventUrn) where T : ISportEvent
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "odds_change.xml");
        var message = _deserializer.Deserialize<odds_change>(stream);
        TestData.FillMessageTimestamp(message);
        message.SportId = UrnCreate.SportId(1000);

        message.event_id = eventUrn.ToString();

        _validator.Validate(message);
        var userFeedMessage = _mapper.MapOddsChange<T>(message, _cultures, null);

        userFeedMessage.ShouldNotBeNull();
        userFeedMessage.ShouldBeAssignableTo<IOddsChange<T>>();
        userFeedMessage.Event.Id.ToString().ShouldBe(message.EventId);
    }

    [Fact]
    public void BetCancelIsMapped()
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "bet_cancel.xml");
        var message = _deserializer.Deserialize<bet_cancel>(stream);
        TestData.FillMessageTimestamp(message);
        message.SportId = UrnCreate.SportId(1000);
        _validator.Validate(message);
        var entity = _mapper.MapBetCancel<ICompetition>(message, _cultures, null);
        Assert.NotNull(entity);
        Assert.NotNull(entity.Markets);
        Assert.Equal(3, entity.Markets.Count());
    }

    [Fact]
    public void RollbackBetCancelIsMapped()
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "rollback_bet_cancel.xml");
        var message = _deserializer.Deserialize<rollback_bet_cancel>(stream);
        TestData.FillMessageTimestamp(message);
        message.SportId = UrnCreate.SportId(1000);
        _validator.Validate(message);
        var entity = _mapper.MapRollbackBetCancel<ICompetition>(message, _cultures, null);
        Assert.NotNull(entity);
        Assert.NotNull(entity.Markets);
        Assert.Equal(2, entity.Markets.Count());
    }

    [Fact]
    public void RollbackBetSettlementIsMapped()
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "rollback_bet_settlement.xml");
        var message = _deserializer.Deserialize<rollback_bet_settlement>(stream);
        TestData.FillMessageTimestamp(message);
        message.SportId = UrnCreate.SportId(1000);
        _validator.Validate(message);
        var entity = _mapper.MapRollbackBetSettlement<ICompetition>(message, _cultures, null);
        Assert.NotNull(entity);
        Assert.NotNull(entity.Markets);
        Assert.Equal(2, entity.Markets.Count());
    }

    [Fact]
    public void SnapshotCompleteIsMapped()
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "snapshot_completed.xml");
        var message = _deserializer.Deserialize<snapshot_complete>(stream);
        TestData.FillMessageTimestamp(message);
        message.SportId = UrnCreate.SportId(1000);
        _validator.Validate(message);
        var entity = _mapper.MapSnapShotCompleted(message);

        entity.ShouldNotBeNull();
        entity.RequestId.ShouldBePositive();
    }
}
