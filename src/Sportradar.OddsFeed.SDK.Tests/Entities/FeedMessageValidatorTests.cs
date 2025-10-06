// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Sdk.Extensions;
using Xunit;
using static Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets.MarketDescriptionEndpoints;
using static Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Feed.Messages.PreconfiguredFeedMessages;
using static Sportradar.OddsFeed.SDK.Tests.Common.TestConsts;

namespace Sportradar.OddsFeed.SDK.Tests.Entities;

public class FeedMessageValidatorTests
{
    private readonly Mock<INamedValueCache> _betStopReasonsCache;
    private readonly Mock<INamedValueCache> _bettingStatusCache;
    private readonly Mock<IMarketCacheProvider> _marketCacheProvider;
    private readonly IFeedMessageValidator _validator;

    public FeedMessageValidatorTests()
    {
        _betStopReasonsCache = new Mock<INamedValueCache>();
        _bettingStatusCache = new Mock<INamedValueCache>();

        var namedValueProviderMock = new Mock<INamedValuesProvider>();
        namedValueProviderMock.Setup(x => x.BetStopReasons).Returns(_betStopReasonsCache.Object);
        namedValueProviderMock.Setup(x => x.BettingStatuses).Returns(_bettingStatusCache.Object);

        _marketCacheProvider = new Mock<IMarketCacheProvider>();

        _validator = new FeedMessageValidator(_marketCacheProvider.Object,
                                              CultureEn,
                                              namedValueProviderMock.Object,
                                              TestProducerManager.Create());
    }

    [Fact]
    public void AliveIsCorrectlyValidated()
    {
        var alive = AliveSubscribed();
        alive.product = 50;
        _validator.Validate(alive).ShouldBe(ValidationResult.Success);

        alive = AliveSubscribed();
        alive.subscribed = -100;
        _validator.Validate(alive).ShouldBe(ValidationResult.ProblemsDetected);
    }

    [Fact]
    public void SnapshotCompletedIsCorrectlyValidated()
    {
        var snapshot = SnapshotCompleteRequest();
        snapshot.product = 50;
        _validator.Validate(snapshot).ShouldBe(ValidationResult.Success);

        snapshot = SnapshotCompleteRequest();
        snapshot.request_id = 0;
        _validator.Validate(snapshot).ShouldBe(ValidationResult.Success);
    }

    [Fact]
    public void FixtureChangeIsCorrectlyValidated()
    {
        var fixtureChange = Bk46VsFcHonkaFixtureChange();
        _validator.Validate(fixtureChange).ShouldBe(ValidationResult.Success);
        fixtureChange.EventUrn.ShouldNotBeNull();
        fixtureChange.product = 50;
        _validator.Validate(fixtureChange).ShouldBe(ValidationResult.Success);

        fixtureChange = Bk46VsFcHonkaFixtureChange();
        fixtureChange.event_id = "invalid_event_id";
        _validator.Validate(fixtureChange).ShouldBe(ValidationResult.Failure);

        fixtureChange = Bk46VsFcHonkaFixtureChange();
        fixtureChange.change_type = 100;
        _validator.Validate(fixtureChange).ShouldBe(ValidationResult.ProblemsDetected);
    }

    [Fact]
    public void BetStopIsCorrectlyValidated()
    {
        var betStop = HaukarKaVsKeflavikBetStop();
        _validator.Validate(betStop).ShouldBe(ValidationResult.Success);
        betStop.EventUrn.ShouldNotBeNull();

        betStop.product = 50;
        _validator.Validate(betStop).ShouldBe(ValidationResult.Success);

        betStop = HaukarKaVsKeflavikBetStop();
        betStop.event_id = "invalid_event_id";
        _validator.Validate(betStop).ShouldBe(ValidationResult.Failure);

        betStop = HaukarKaVsKeflavikBetStop();
        betStop.groups = null;
        _validator.Validate(betStop).ShouldBe(ValidationResult.Failure);

        betStop = HaukarKaVsKeflavikBetStop();
        betStop.groups = string.Empty;
        _validator.Validate(betStop).ShouldBe(ValidationResult.Failure);
    }

    [Fact]
    public void BetCancelIsCorrectlyValidated()
    {
        SetupMocksForGetMarketDescriptionForAllMarketsInBetCancel();
        var betCancel = GsApollonVsAeGravasBetCancel();
        _validator.Validate(betCancel).ShouldBe(ValidationResult.Success);
        betCancel.EventUrn.ShouldNotBeNull();
        betCancel.market.ShouldNotContain(m => m.ValidationFailed);
        betCancel.market.Count(m => !string.IsNullOrEmpty(m.specifiers)).ShouldBe(betCancel.market.Count(m => m.Specifiers != null));

        betCancel = GsApollonVsAeGravasBetCancel();
        betCancel.product = 50;
        _validator.Validate(betCancel).ShouldBe(ValidationResult.Success);

        betCancel = GsApollonVsAeGravasBetCancel();
        betCancel.event_id = "some_event_id";
        _validator.Validate(betCancel).ShouldBe(ValidationResult.Failure);

        betCancel = GsApollonVsAeGravasBetCancel();
        betCancel.market = [];
        _validator.Validate(betCancel).ShouldBe(ValidationResult.Success);

        betCancel = GsApollonVsAeGravasBetCancel();
        betCancel.market = null;
        _validator.Validate(betCancel).ShouldBe(ValidationResult.Failure);

        betCancel = GsApollonVsAeGravasBetCancel();
        betCancel.market.First().specifiers = null;
        _validator.Validate(betCancel).ShouldBe(ValidationResult.Success);

        betCancel = GsApollonVsAeGravasBetCancel();
        betCancel.market.First().specifiers = "some_specifiers";
        _validator.Validate(betCancel).ShouldBe(ValidationResult.ProblemsDetected);
        betCancel.market.Count(c => !c.ValidationFailed).ShouldBe(betCancel.market.Length - 1);
    }

    [Fact]
    public void RollbackBetCancelIsCorrectlyValidated()
    {
        SetupMocksForGetMarketDescriptionForAllMarketsInRollbackBetCancel();
        var rollbackCancel = GsApollonVsAeGravasBetCancelRollback();
        _validator.Validate(rollbackCancel).ShouldBe(ValidationResult.Success);
        rollbackCancel.EventUrn.ShouldNotBeNull();
        rollbackCancel.market.ShouldNotContain(m => m.ValidationFailed);
        rollbackCancel.market.Count(m => !string.IsNullOrEmpty(m.specifiers))
                      .ShouldBe(rollbackCancel.market.Count(m => m.Specifiers != null));

        rollbackCancel = GsApollonVsAeGravasBetCancelRollback();
        rollbackCancel.product = 50;
        _validator.Validate(rollbackCancel).ShouldBe(ValidationResult.Success);

        rollbackCancel = GsApollonVsAeGravasBetCancelRollback();
        rollbackCancel.event_id = "some_event_id";
        _validator.Validate(rollbackCancel).ShouldBe(ValidationResult.Failure);

        rollbackCancel = GsApollonVsAeGravasBetCancelRollback();
        rollbackCancel.market = [];
        _validator.Validate(rollbackCancel).ShouldBe(ValidationResult.Success);

        rollbackCancel = GsApollonVsAeGravasBetCancelRollback();
        rollbackCancel.market = null;
        _validator.Validate(rollbackCancel).ShouldBe(ValidationResult.Failure);

        rollbackCancel = GsApollonVsAeGravasBetCancelRollback();
        rollbackCancel.market.First().specifiers = null;
        _validator.Validate(rollbackCancel).ShouldBe(ValidationResult.Success);

        rollbackCancel = GsApollonVsAeGravasBetCancelRollback();
        rollbackCancel.market.First().specifiers = "some_specifiers";
        _validator.Validate(rollbackCancel).ShouldBe(ValidationResult.ProblemsDetected);
        rollbackCancel.market.Count(c => !c.ValidationFailed)
                      .ShouldBe(rollbackCancel.market.Length - 1);
    }

    [Fact]
    public void BetSettlementIsCorrectlyValidated()
    {
        SetupMocksForGetMarketDescriptionForAllMarketsInBetSettlement();
        var settlement = MenaVsGonzalezBetSettlement();
        _validator.Validate(settlement).ShouldBe(ValidationResult.Success);
        settlement.EventUrn.ShouldNotBeNull();
        settlement.outcomes.ShouldNotContain(m => m.ValidationFailed);
        settlement.outcomes.Count(m => !string.IsNullOrEmpty(m.specifiers))
                  .ShouldBe(settlement.outcomes.Count(m => m.Specifiers != null));

        settlement = MenaVsGonzalezBetSettlement();
        settlement.product = 50;
        _validator.Validate(settlement).ShouldBe(ValidationResult.Success);

        settlement = MenaVsGonzalezBetSettlement();
        settlement.event_id = "some_event_id";
        _validator.Validate(settlement).ShouldBe(ValidationResult.Failure);

        settlement = MenaVsGonzalezBetSettlement();
        settlement.outcomes = [];
        _validator.Validate(settlement).ShouldBe(ValidationResult.Success);

        settlement = MenaVsGonzalezBetSettlement();
        settlement.outcomes = null;
        _validator.Validate(settlement).ShouldBe(ValidationResult.Success);

        settlement = MenaVsGonzalezBetSettlement();
        settlement.outcomes.First().specifiers = null;
        _validator.Validate(settlement).ShouldBe(ValidationResult.Success);

        settlement = MenaVsGonzalezBetSettlement();
        settlement.outcomes.First().specifiers = "some_specifiers";
        _validator.Validate(settlement).ShouldBe(ValidationResult.ProblemsDetected);
        settlement.outcomes.Count(c => !c.ValidationFailed)
                  .ShouldBe(settlement.outcomes.Length - 1);
    }

    [Fact]
    public void RollbackBetSettlementIsCorrectlyValidated()
    {
        SetupMocksForGetMarketDescriptionForAllMarketsInRollbackBetSettlement();
        var rollback = GsApollonVsAeGravasBetSettlementRollback();
        _validator.Validate(rollback).ShouldBe(ValidationResult.Success);
        rollback.EventUrn.ShouldNotBeNull();
        rollback.market.ShouldNotContain(m => m.ValidationFailed);
        rollback.market.Count(m => !string.IsNullOrEmpty(m.specifiers))
                .ShouldBe(rollback.market.Count(m => m.Specifiers != null));

        rollback = GsApollonVsAeGravasBetSettlementRollback();
        rollback.product = 50;
        _validator.Validate(rollback).ShouldBe(ValidationResult.Success);

        rollback = GsApollonVsAeGravasBetSettlementRollback();
        rollback.event_id = "some_event_id";
        _validator.Validate(rollback).ShouldBe(ValidationResult.Failure);

        rollback = GsApollonVsAeGravasBetSettlementRollback();
        rollback.market = [];
        _validator.Validate(rollback).ShouldBe(ValidationResult.Success);

        rollback = GsApollonVsAeGravasBetSettlementRollback();
        rollback.market = null;
        _validator.Validate(rollback).ShouldBe(ValidationResult.Failure);

        rollback = GsApollonVsAeGravasBetSettlementRollback();
        rollback.market.First().specifiers = null;
        _validator.Validate(rollback).ShouldBe(ValidationResult.Success);

        rollback = GsApollonVsAeGravasBetSettlementRollback();
        rollback.market.First().specifiers = "some_specifiers";
        _validator.Validate(rollback).ShouldBe(ValidationResult.ProblemsDetected);
        rollback.market.Count(c => !c.ValidationFailed)
                .ShouldBe(rollback.market.Length - 1);
    }

    [Fact]
    public void OddsChangeIssuesIsCorrectlyValidated()
    {
        SetupMocksForGetMarketDescriptionForAllMarketsInOddsChangeMessage();
        var oddsChange = AlNasrVsMuscatOddsChange();
        _validator.Validate(oddsChange).ShouldBe(ValidationResult.Success);
        oddsChange.EventUrn.ShouldNotBeNull();
        oddsChange.odds.market.ShouldNotContain(m => m.ValidationFailed);
        oddsChange.odds.market.Count(m => !string.IsNullOrEmpty(m.specifiers))
                  .ShouldBe(oddsChange.odds.market.Count(m => m.Specifiers != null));

        oddsChange = AlNasrVsMuscatOddsChange();
        oddsChange.product = 50;
        _validator.Validate(oddsChange).ShouldBe(ValidationResult.Success);

        oddsChange = AlNasrVsMuscatOddsChange();
        oddsChange.event_id = "some_event_id";
        _validator.Validate(oddsChange).ShouldBe(ValidationResult.Failure);

        oddsChange = AlNasrVsMuscatOddsChange();
        oddsChange.odds_change_reason = 100;
        oddsChange.odds_change_reasonSpecified = true;
        _validator.Validate(oddsChange).ShouldBe(ValidationResult.ProblemsDetected);

        oddsChange = AlNasrVsMuscatOddsChange();
        oddsChange.odds = null;
        _validator.Validate(oddsChange).ShouldBe(ValidationResult.Success);

        _betStopReasonsCache.Setup(betStopReasonsCache => betStopReasonsCache.IsValueDefined(It.IsAny<int>())).Returns(false);
        _bettingStatusCache.Setup(bettingStatusCache => bettingStatusCache.IsValueDefined(It.IsAny<int>())).Returns(false);
        oddsChange = AlNasrVsMuscatOddsChange();
        oddsChange.odds.betstop_reason = 100;
        oddsChange.odds.betstop_reasonSpecified = true;
        _validator.Validate(oddsChange).ShouldBe(ValidationResult.ProblemsDetected);

        oddsChange = AlNasrVsMuscatOddsChange();
        oddsChange.odds.betting_status = 100;
        oddsChange.odds.betting_statusSpecified = true;
        _validator.Validate(oddsChange).ShouldBe(ValidationResult.ProblemsDetected);

        oddsChange = AlNasrVsMuscatOddsChange();
        oddsChange.odds.market = null;
        _validator.Validate(oddsChange).ShouldBe(ValidationResult.Success);

        oddsChange = AlNasrVsMuscatOddsChange();
        oddsChange.odds.market.First().outcome = null;
        _validator.Validate(oddsChange).ShouldBe(ValidationResult.Success);

        oddsChange = AlNasrVsMuscatOddsChange();
        oddsChange.odds.market.First().outcome.First().active = 100;
        _validator.Validate(oddsChange).ShouldBe(ValidationResult.ProblemsDetected);

        oddsChange = AlNasrVsMuscatOddsChange();
        oddsChange.odds.market.First().specifiers = "some_specifiers";
        _validator.Validate(oddsChange).ShouldBe(ValidationResult.ProblemsDetected);
        oddsChange.odds.market.Count(c => !c.ValidationFailed)
                  .ShouldBe(oddsChange.odds.market.Length - 1);
    }

    private void SetupMocksForGetMarketDescriptionForAllMarketsInRollbackBetSettlement()
    {
        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(312, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription312().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(313, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription313().ToUserMarketDescription(CultureEn));
    }

    private void SetupMocksForGetMarketDescriptionForAllMarketsInBetCancel()
    {
        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(312, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription312().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(313, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription313().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(314, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription314().ToUserMarketDescription(CultureEn));
    }

    private void SetupMocksForGetMarketDescriptionForAllMarketsInRollbackBetCancel()
    {
        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(312, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription312().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(313, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription313().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(314, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription314().ToUserMarketDescription(CultureEn));
    }

    private void SetupMocksForGetMarketDescriptionForAllMarketsInOddsChangeMessage()
    {
        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(1, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription1().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(7, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription7().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(8, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription8().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(10, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription10().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(11, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription11().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(14, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription14().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(18, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription18().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(19, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription19().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(20, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription20().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(21, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription21().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(23, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription23().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(24, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription24().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(26, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription26().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(29, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription29().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(37, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription37().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(60, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription60().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(61, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription61().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(62, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription62().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(68, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription68().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(71, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription71().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(72, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription72().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(73, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription73().ToUserMarketDescription(CultureEn));
    }

    private void SetupMocksForGetMarketDescriptionForAllMarketsInBetSettlement()
    {
        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(189, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription189().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(190, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription190().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(208, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription208().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(209, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription209().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(210, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription210().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(211, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription211().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(212, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription212().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(213, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription213().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(214, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription214().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(215, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription215().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(216, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription216().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(217, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription217().ToUserMarketDescription(CultureEn));

        _marketCacheProvider.Setup(m => m.GetMarketDescriptionAsync(218, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<List<CultureInfo>>(), It.IsAny<bool>()))
                            .ReturnsAsync(GetMarketDescription218().ToUserMarketDescription(CultureEn));
    }
}
