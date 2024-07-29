// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities;

public class FeedMessageValidatorTests
{
    private readonly IDeserializer<FeedMessage> _deserializer = new Deserializer<FeedMessage>();
    private readonly IFeedMessageValidator _validator;
    private readonly TimeSpan _timerInterval = TimeSpan.FromSeconds(1);

    public FeedMessageValidatorTests(ITestOutputHelper outputHelper)
    {
        var loggerFactory = new XunitLoggerFactory(outputHelper);
        var testCacheStoreManager = new TestCacheStoreManager();
        var variantMarketDescriptionMemoryCache = testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForVariantMarketDescriptionCache);
        var variantDescriptionMemoryCache = testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForVariantDescriptionListCache);
        var invariantMarketDescriptionMemoryCache = testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForInvariantMarketDescriptionsCache);
        IDataRouterManager dataRouterManager = new TestDataRouterManager(testCacheStoreManager.CacheManager, outputHelper);

        var timer = new SdkTimer("variantMarketCacheTimer", _timerInterval, _timerInterval);
        IMappingValidatorFactory mappingValidatorFactory = new MappingValidatorFactory();
        IMarketDescriptionsCache inVariantMdCache = new InvariantMarketDescriptionCache(invariantMarketDescriptionMemoryCache, dataRouterManager, mappingValidatorFactory, timer, TestData.Cultures, testCacheStoreManager.CacheManager, loggerFactory);
        IMarketDescriptionCache variantMdCache = new VariantMarketDescriptionCache(variantMarketDescriptionMemoryCache, dataRouterManager, mappingValidatorFactory, testCacheStoreManager.CacheManager, loggerFactory);
        IVariantDescriptionsCache variantDescriptionListCache = new VariantDescriptionListCache(variantDescriptionMemoryCache, dataRouterManager, mappingValidatorFactory, timer, TestData.Cultures, testCacheStoreManager.CacheManager, loggerFactory);

        var dataFetcher = new TestDataFetcher();

        var nameCacheSdkTimer = SdkTimer.Create("namedValueCacheTimer", TimeSpan.FromMilliseconds(10), TimeSpan.Zero);
        var betStopReasonsCache = new NamedValueCache("BetstopReasons", ExceptionHandlingStrategy.Throw, new NamedValueDataProvider(UofSdkBootstrap.DataProviderForNamedValueCacheBetStopReason, FileHelper.FindFile("betstop_reasons.xml"), dataFetcher, "betstop_reason"), nameCacheSdkTimer);
        var bettingStatusCache = new NamedValueCache("BettingStatus", ExceptionHandlingStrategy.Throw, new NamedValueDataProvider(UofSdkBootstrap.DataProviderForNamedValueCacheBettingStatus, FileHelper.FindFile("betting_status.xml"), dataFetcher, "betting_status"), nameCacheSdkTimer);

        var namedValueProviderMock = new Mock<INamedValuesProvider>();
        namedValueProviderMock.Setup(x => x.BetStopReasons).Returns(betStopReasonsCache);
        namedValueProviderMock.Setup(x => x.BettingStatuses).Returns(bettingStatusCache);

        _validator = new FeedMessageValidator(
            new MarketCacheProvider(inVariantMdCache, variantMdCache, variantDescriptionListCache, loggerFactory.CreateLogger<MarketCacheProvider>()),
            TestData.Culture,
            namedValueProviderMock.Object,
            TestProducerManager.Create());
    }

    private T GetMessage<T>(string fileName) where T : FeedMessage
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, fileName);
        var message = _deserializer.Deserialize<T>(stream);
        if (message.IsEventRelated)
        {
            message.SportId = TestData.SportId;
        }
        return message;
    }

    [Fact]
    public void AliveIsCorrectlyValidated()
    {
        var alive = GetMessage<alive>("alive.xml");
        alive.product = 50;
        Assert.Equal(ValidationResult.Success, _validator.Validate(alive));

        alive = GetMessage<alive>("alive.xml");
        alive.subscribed = -100;
        Assert.Equal(ValidationResult.ProblemsDetected, _validator.Validate(alive));
    }

    [Fact]
    public void SnapshotCompletedIsCorrectlyValidated()
    {
        var snapshot = GetMessage<snapshot_complete>("snapshot_completed.xml");
        snapshot.product = 50;
        Assert.Equal(ValidationResult.Success, _validator.Validate(snapshot));

        snapshot = GetMessage<snapshot_complete>("snapshot_completed.xml");
        snapshot.request_id = 0;
        Assert.Equal(ValidationResult.Success, _validator.Validate(snapshot));
    }

    [Fact]
    public void FixtureChangeIsCorrectlyValidated()
    {
        var fixtureChange = GetMessage<fixture_change>("fixture_change.xml");
        Assert.Equal(ValidationResult.Success, _validator.Validate(fixtureChange));
        Assert.NotNull(fixtureChange.EventUrn);
        fixtureChange.product = 50;
        Assert.Equal(ValidationResult.Success, _validator.Validate(fixtureChange));

        fixtureChange = GetMessage<fixture_change>("fixture_change.xml");
        fixtureChange.event_id = "invalid_event_id";
        Assert.Equal(ValidationResult.Failure, _validator.Validate(fixtureChange));

        fixtureChange = GetMessage<fixture_change>("fixture_change.xml");
        fixtureChange.change_type = 100;
        Assert.Equal(ValidationResult.ProblemsDetected, _validator.Validate(fixtureChange));
    }

    [Fact]
    public void BetStopIsCorrectlyValidated()
    {
        var betStop = GetMessage<bet_stop>("bet_stop.xml");
        Assert.Equal(ValidationResult.Success, _validator.Validate(betStop));
        Assert.NotNull(betStop.EventUrn);

        betStop.product = 50;
        Assert.Equal(ValidationResult.Success, _validator.Validate(betStop));

        betStop = GetMessage<bet_stop>("bet_stop.xml");
        betStop.event_id = "invalid_event_id";
        Assert.Equal(ValidationResult.Failure, _validator.Validate(betStop));

        betStop = GetMessage<bet_stop>("bet_stop.xml");
        betStop.groups = null;
        Assert.Equal(ValidationResult.Failure, _validator.Validate(betStop));

        betStop = GetMessage<bet_stop>("bet_stop.xml");
        betStop.groups = string.Empty;
        Assert.Equal(ValidationResult.Failure, _validator.Validate(betStop));
    }

    [Fact]
    public void BetCancelIsCorrectlyValidated()
    {
        var betCancel = GetMessage<bet_cancel>("bet_cancel.xml");
        Assert.Equal(ValidationResult.Success, _validator.Validate(betCancel));
        Assert.NotNull(betCancel.EventUrn);
        Assert.Equal(0, betCancel.market.Count(m => m.ValidationFailed));
        Assert.Equal(betCancel.market.Count(m => !string.IsNullOrEmpty(m.specifiers)), betCancel.market.Count(m => m.Specifiers != null));

        betCancel = GetMessage<bet_cancel>("bet_cancel.xml");
        betCancel.product = 50;
        Assert.Equal(ValidationResult.Success, _validator.Validate(betCancel));

        betCancel = GetMessage<bet_cancel>("bet_cancel.xml");
        betCancel.event_id = "some_event_id";
        Assert.Equal(ValidationResult.Failure, _validator.Validate(betCancel));

        betCancel = GetMessage<bet_cancel>("bet_cancel.xml");
        betCancel.market = new market[0];
        Assert.Equal(ValidationResult.Success, _validator.Validate(betCancel));

        betCancel = GetMessage<bet_cancel>("bet_cancel.xml");
        betCancel.market = null;
        Assert.Equal(ValidationResult.Failure, _validator.Validate(betCancel));

        betCancel = GetMessage<bet_cancel>("bet_cancel.xml");
        betCancel.market.First().specifiers = null;
        Assert.Equal(ValidationResult.Success, _validator.Validate(betCancel));

        betCancel = GetMessage<bet_cancel>("bet_cancel.xml");
        betCancel.market.First().specifiers = "some_specifiers";
        Assert.Equal(ValidationResult.ProblemsDetected, _validator.Validate(betCancel));
        Assert.Equal(betCancel.market.Count(c => !c.ValidationFailed), betCancel.market.Length - 1);
    }

    [Fact]
    public void RollbackBetCancelIsCorrectlyValidated()
    {
        var rollbackCancel = GetMessage<rollback_bet_cancel>("rollback_bet_cancel.xml");
        Assert.Equal(ValidationResult.Success, _validator.Validate(rollbackCancel));
        Assert.NotNull(rollbackCancel.EventUrn);
        Assert.Equal(0, rollbackCancel.market.Count(m => m.ValidationFailed));
        Assert.Equal(rollbackCancel.market.Count(m => !string.IsNullOrEmpty(m.specifiers)), rollbackCancel.market.Count(m => m.Specifiers != null));

        rollbackCancel = GetMessage<rollback_bet_cancel>("rollback_bet_cancel.xml");
        rollbackCancel.product = 50;
        Assert.Equal(ValidationResult.Success, _validator.Validate(rollbackCancel));

        rollbackCancel = GetMessage<rollback_bet_cancel>("rollback_bet_cancel.xml");
        rollbackCancel.event_id = "some_event_id";
        Assert.Equal(ValidationResult.Failure, _validator.Validate(rollbackCancel));

        rollbackCancel = GetMessage<rollback_bet_cancel>("rollback_bet_cancel.xml");
        rollbackCancel.market = new market[0];
        Assert.Equal(ValidationResult.Success, _validator.Validate(rollbackCancel));

        rollbackCancel = GetMessage<rollback_bet_cancel>("rollback_bet_cancel.xml");
        rollbackCancel.market = null;
        Assert.Equal(ValidationResult.Failure, _validator.Validate(rollbackCancel));

        rollbackCancel = GetMessage<rollback_bet_cancel>("rollback_bet_cancel.xml");
        rollbackCancel.market.First().specifiers = null;
        Assert.Equal(ValidationResult.Success, _validator.Validate(rollbackCancel));

        rollbackCancel = GetMessage<rollback_bet_cancel>("rollback_bet_cancel.xml");
        rollbackCancel.market.First().specifiers = "some_specifiers";
        Assert.Equal(ValidationResult.ProblemsDetected, _validator.Validate(rollbackCancel));
        Assert.Equal(rollbackCancel.market.Count(c => !c.ValidationFailed), rollbackCancel.market.Length - 1);
    }

    [Fact]
    public void BetSettlementIsCorrectlyValidated()
    {
        var settlement = GetMessage<bet_settlement>("bet_settlement.xml");
        Assert.Equal(ValidationResult.Success, _validator.Validate(settlement));
        Assert.NotNull(settlement.EventUrn);
        Assert.Equal(0, settlement.outcomes.Count(m => m.ValidationFailed));
        Assert.Equal(settlement.outcomes.Count(m => !string.IsNullOrEmpty(m.specifiers)), settlement.outcomes.Count(m => m.Specifiers != null));

        settlement = GetMessage<bet_settlement>("bet_settlement.xml");
        settlement.product = 50;
        Assert.Equal(ValidationResult.Success, _validator.Validate(settlement));

        settlement = GetMessage<bet_settlement>("bet_settlement.xml");
        settlement.event_id = "some_event_id";
        Assert.Equal(ValidationResult.Failure, _validator.Validate(settlement));

        settlement = GetMessage<bet_settlement>("bet_settlement.xml");
        settlement.outcomes = Array.Empty<betSettlementMarket>();
        Assert.Equal(ValidationResult.Success, _validator.Validate(settlement));

        settlement = GetMessage<bet_settlement>("bet_settlement.xml");
        settlement.outcomes = null;
        Assert.Equal(ValidationResult.Success, _validator.Validate(settlement));

        settlement = GetMessage<bet_settlement>("bet_settlement.xml");
        settlement.outcomes.First().specifiers = null;
        Assert.Equal(ValidationResult.Success, _validator.Validate(settlement));

        settlement = GetMessage<bet_settlement>("bet_settlement.xml");
        settlement.outcomes.First().specifiers = "some_specifiers";
        Assert.Equal(ValidationResult.ProblemsDetected, _validator.Validate(settlement));
        Assert.Equal(settlement.outcomes.Count(c => !c.ValidationFailed), settlement.outcomes.Length - 1);
    }

    [Fact]
    public void RollbackBetSettlementIsCorrectlyValidated()
    {
        var rollback = GetMessage<rollback_bet_settlement>("rollback_bet_settlement.xml");
        Assert.Equal(ValidationResult.Success, _validator.Validate(rollback));
        Assert.NotNull(rollback.EventUrn);
        Assert.Equal(0, rollback.market.Count(m => m.ValidationFailed));
        Assert.Equal(rollback.market.Count(m => !string.IsNullOrEmpty(m.specifiers)), rollback.market.Count(m => m.Specifiers != null));

        rollback = GetMessage<rollback_bet_settlement>("rollback_bet_settlement.xml");
        rollback.product = 50;
        Assert.Equal(ValidationResult.Success, _validator.Validate(rollback));

        rollback = GetMessage<rollback_bet_settlement>("rollback_bet_settlement.xml");
        rollback.event_id = "some_event_id";
        Assert.Equal(ValidationResult.Failure, _validator.Validate(rollback));

        rollback = GetMessage<rollback_bet_settlement>("rollback_bet_settlement.xml");
        rollback.market = new market[0];
        Assert.Equal(ValidationResult.Success, _validator.Validate(rollback));

        rollback = GetMessage<rollback_bet_settlement>("rollback_bet_settlement.xml");
        rollback.market = null;
        Assert.Equal(ValidationResult.Failure, _validator.Validate(rollback));

        rollback = GetMessage<rollback_bet_settlement>("rollback_bet_settlement.xml");
        rollback.market.First().specifiers = null;
        Assert.Equal(ValidationResult.Success, _validator.Validate(rollback));

        rollback = GetMessage<rollback_bet_settlement>("rollback_bet_settlement.xml");
        rollback.market.First().specifiers = "some_specifiers";
        Assert.Equal(ValidationResult.ProblemsDetected, _validator.Validate(rollback));
        Assert.Equal(rollback.market.Count(c => !c.ValidationFailed), rollback.market.Length - 1);
    }

    [Fact]
    public void OddsChangeIssuesIsCorrectlyValidated()
    {
        var oddsChange = GetMessage<odds_change>("odds_change.xml");
        Assert.Equal(ValidationResult.Success, _validator.Validate(oddsChange));
        Assert.NotNull(oddsChange.EventUrn);
        Assert.Equal(0, oddsChange.odds.market.Count(m => m.ValidationFailed));
        Assert.Equal(oddsChange.odds.market.Count(m => !string.IsNullOrEmpty(m.specifiers)),
            oddsChange.odds.market.Count(m => m.Specifiers != null));

        oddsChange = GetMessage<odds_change>("odds_change.xml");
        oddsChange.product = 50;
        Assert.Equal(ValidationResult.Success, _validator.Validate(oddsChange));

        oddsChange = GetMessage<odds_change>("odds_change.xml");
        oddsChange.event_id = "some_event_id";
        Assert.Equal(ValidationResult.Failure, _validator.Validate(oddsChange));

        oddsChange = GetMessage<odds_change>("odds_change.xml");
        oddsChange.odds_change_reason = 100;
        oddsChange.odds_change_reasonSpecified = true;
        Assert.Equal(ValidationResult.ProblemsDetected, _validator.Validate(oddsChange));

        oddsChange = GetMessage<odds_change>("odds_change.xml");
        oddsChange.odds = null;
        Assert.Equal(ValidationResult.Success, _validator.Validate(oddsChange));

        oddsChange = GetMessage<odds_change>("odds_change.xml");
        oddsChange.odds.betstop_reason = 100;
        oddsChange.odds.betstop_reasonSpecified = true;
        Assert.Equal(ValidationResult.ProblemsDetected, _validator.Validate(oddsChange));

        oddsChange = GetMessage<odds_change>("odds_change.xml");
        oddsChange.odds.betting_status = 100;
        oddsChange.odds.betting_statusSpecified = true;
        Assert.Equal(ValidationResult.ProblemsDetected, _validator.Validate(oddsChange));

        oddsChange = GetMessage<odds_change>("odds_change.xml");
        oddsChange.odds.market = null;
        Assert.Equal(ValidationResult.Success, _validator.Validate(oddsChange));

        oddsChange = GetMessage<odds_change>("odds_change.xml");
        oddsChange.odds.market.First().outcome = null;
        Assert.Equal(ValidationResult.Success, _validator.Validate(oddsChange));

        oddsChange = GetMessage<odds_change>("odds_change.xml");
        oddsChange.odds.market.First().outcome.First().active = 100;
        Assert.Equal(ValidationResult.ProblemsDetected, _validator.Validate(oddsChange));

        oddsChange = GetMessage<odds_change>("odds_change.xml");
        oddsChange.odds.market.First().specifiers = "some_specifiers";
        Assert.Equal(ValidationResult.ProblemsDetected, _validator.Validate(oddsChange));
        Assert.Equal(oddsChange.odds.market.Count(c => !c.ValidationFailed), oddsChange.odds.market.Length - 1);
    }
}
