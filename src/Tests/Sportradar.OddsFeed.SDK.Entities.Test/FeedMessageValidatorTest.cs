/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Linq;
using System.Runtime.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.Test
{
    [TestClass]
    public class FeedMessageValidatorTest
    {
        private readonly IDeserializer<FeedMessage> _deserializer = new Deserializer<FeedMessage>();

        private IFeedMessageValidator _validator;

        private MemoryCache _variantMemoryCache;
        private MemoryCache _invariantMemoryCache;
        private MemoryCache _variantDescriptionsMemoryCache;
        private IMarketDescriptionCache _variantMdCache;
        private IMarketDescriptionCache _inVariantMdCache;
        private IVariantDescriptionCache _variantDescriptionListCache;
        private IMappingValidatorFactory _mappingValidatorFactory;

        private readonly TimeSpan _timerInterval = TimeSpan.FromSeconds(1);
        private SdkTimer _timer;
        private CacheManager _cacheManager;
        private IDataRouterManager _dataRouterManager;

        [TestInitialize]
        public void Init()
        {
            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager);

            _variantMemoryCache = new MemoryCache("VariantCache");
            _invariantMemoryCache = new MemoryCache("InVariantCache");
            _variantDescriptionsMemoryCache = new MemoryCache("VariantDescriptionListCache");

            _timer = new SdkTimer(_timerInterval, _timerInterval);
            _mappingValidatorFactory = new MappingValidatorFactory();
            _inVariantMdCache = new InvariantMarketDescriptionCache(_invariantMemoryCache, _dataRouterManager, _mappingValidatorFactory, _timer, TestData.Cultures, _cacheManager);
            _variantMdCache = new VariantMarketDescriptionCache(_variantMemoryCache, _dataRouterManager, _mappingValidatorFactory, _cacheManager);
            _variantDescriptionListCache = new VariantDescriptionListCache(_variantDescriptionsMemoryCache, _dataRouterManager, _mappingValidatorFactory, _timer, TestData.Cultures, _cacheManager);

            var dataFetcher = new TestDataFetcher();

            var betStopReasonsCache = new NamedValueCache(new NamedValueDataProvider(FileHelper.FindFile("betstop_reasons.xml"), dataFetcher, "betstop_reason"),  ExceptionHandlingStrategy.THROW);

            var bettingStatusCache = new NamedValueCache(new NamedValueDataProvider(FileHelper.FindFile("betting_status.xml"), dataFetcher, "betting_status"),
                ExceptionHandlingStrategy.THROW);

            var namedValueProviderMock = new Mock<INamedValuesProvider>();
            namedValueProviderMock.Setup(x => x.BetStopReasons).Returns(betStopReasonsCache);
            namedValueProviderMock.Setup(x => x.BettingStatuses).Returns(bettingStatusCache);

             _validator = new FeedMessageValidator(
                 new MarketCacheProvider(_inVariantMdCache, _variantMdCache, _variantDescriptionListCache),
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

        [TestMethod]
        public void AliveIsCorrectlyValidated()
        {
            var alive = GetMessage<alive>("alive.xml");
            alive.product = 50;
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(alive), "Validation of product should be ok");

            alive = GetMessage<alive>("alive.xml");
            alive.subscribed = -100;
            Assert.AreEqual(ValidationResult.PROBLEMS_DETECTED, _validator.Validate(alive), "Validation of subscribed should fail");
        }

        [TestMethod]
        public void SnapshotCompletedIsCorrectlyValidated()
        {
            var snapshot = GetMessage<snapshot_complete>("snapshot_completed.xml");
            snapshot.product = 50;
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(snapshot), "Validation of product should be ok");

            snapshot = GetMessage<snapshot_complete>("snapshot_completed.xml");
            snapshot.request_id = 0;
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(snapshot), "Validation of request_id should fail");
        }

        [TestMethod]
        public void FixtureChangeIsCorrectlyValidated()
        {
            var fixtureChange = GetMessage<fixture_change>("fixture_change.xml");
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(fixtureChange), "Validation should succeed");
            Assert.IsNotNull(fixtureChange.EventURN, "EventURN of unchanged message should not be null");
            fixtureChange.product = 50;
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(fixtureChange), "Validation of product should be ok");

            fixtureChange = GetMessage<fixture_change>("fixture_change.xml");
            fixtureChange.event_id = "invalid_event_id";
            Assert.AreEqual(ValidationResult.FAILURE, _validator.Validate(fixtureChange), "Validation of event_id should fail");

            fixtureChange = GetMessage<fixture_change>("fixture_change.xml");
            fixtureChange.change_type = 100;
            Assert.AreEqual(ValidationResult.PROBLEMS_DETECTED, _validator.Validate(fixtureChange), "Validation of change_type should cause problems");
        }

        [TestMethod]
        public void BetStopIsCorrectlyValidated()
        {
            var betStop = GetMessage<bet_stop>("bet_stop.xml");
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(betStop), "Validation of message should be successful");
            Assert.IsNotNull(betStop.EventURN, "EventURN of unchanged message should not be null");

            betStop.product = 50;
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(betStop), "Validation of product should be ok");

            betStop = GetMessage<bet_stop>("bet_stop.xml");
            betStop.event_id = "invalid_event_id";
            Assert.AreEqual(ValidationResult.FAILURE, _validator.Validate(betStop), "Validation of event_id should fail");

            betStop = GetMessage<bet_stop>("bet_stop.xml");
            betStop.groups = null;
            Assert.AreEqual(ValidationResult.FAILURE, _validator.Validate(betStop), "Validation of null groups should fail");

            betStop = GetMessage<bet_stop>("bet_stop.xml");
            betStop.groups = string.Empty;
            Assert.AreEqual(ValidationResult.FAILURE, _validator.Validate(betStop), "Validation of empty groups should fail");
        }

        [TestMethod]
        public void BetCancelIsCorrectlyValidated()
        {
            var betCancel = GetMessage<bet_cancel>("bet_cancel.xml");
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(betCancel), "Validation of unchanged message should succeed");
            Assert.IsNotNull(betCancel.EventURN, "EventURN of unchanged message should not be null");
            Assert.AreEqual(0, betCancel.market.Count(m => m.ValidationFailed), "All markets on unchanged message should be valid");
            Assert.AreEqual(betCancel.market.Count(m => !string.IsNullOrEmpty(m.specifiers)),
                betCancel.market.Count(m => m.Specifiers != null),
                "The number of markets with parsed specifiers should match to the number of markets with define specifiers");

            betCancel = GetMessage<bet_cancel>("bet_cancel.xml");
            betCancel.product = 50;
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(betCancel), "Validation of product should be ok");

            betCancel = GetMessage<bet_cancel>("bet_cancel.xml");
            betCancel.event_id = "some_event_id";
            Assert.AreEqual(ValidationResult.FAILURE, _validator.Validate(betCancel), "Validation of event_id should fail");

            betCancel = GetMessage<bet_cancel>("bet_cancel.xml");
            betCancel.market = new market[0];
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(betCancel), "Validation of empty markets should succeed");

            betCancel = GetMessage<bet_cancel>("bet_cancel.xml");
            betCancel.market = null;
            Assert.AreEqual(ValidationResult.FAILURE, _validator.Validate(betCancel), "Validation of null markets should fail");

            betCancel = GetMessage<bet_cancel>("bet_cancel.xml");
            betCancel.market.First().specifiers = null;
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(betCancel), "Validation of null specifiers should succeed");

            betCancel = GetMessage<bet_cancel>("bet_cancel.xml");
            betCancel.market.First().specifiers = "some_specifiers";
            Assert.AreEqual(ValidationResult.PROBLEMS_DETECTED, _validator.Validate(betCancel), "Validation of specifiers should cause problems");
            Assert.AreEqual(betCancel.market.Count(c => !c.ValidationFailed), betCancel.market.Length - 1, "The number of valid markets is not correct");
        }

        [TestMethod]
        public void RollbackBetCancelIsCorrectlyValidated()
        {
            var rollbackCancel = GetMessage<rollback_bet_cancel>("rollback_bet_cancel.xml");
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(rollbackCancel), "Validation of unchanged message should succeed");
            Assert.IsNotNull(rollbackCancel.EventURN, "EventURN of unchanged message should not be null");
            Assert.AreEqual(0, rollbackCancel.market.Count(m => m.ValidationFailed), "All markets on unchanged message should be valid");
            Assert.AreEqual(rollbackCancel.market.Count(m => !string.IsNullOrEmpty(m.specifiers)),
                rollbackCancel.market.Count(m => m.Specifiers != null),
                "The number of markets with parsed specifiers should match to the number of markets with define specifiers");

            rollbackCancel = GetMessage<rollback_bet_cancel>("rollback_bet_cancel.xml");
            rollbackCancel.product = 50;
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(rollbackCancel), "Validation of product should be ok");

            rollbackCancel = GetMessage<rollback_bet_cancel>("rollback_bet_cancel.xml");
            rollbackCancel.event_id = "some_event_id";
            Assert.AreEqual(ValidationResult.FAILURE, _validator.Validate(rollbackCancel), "Validation of event_id should fail");

            rollbackCancel = GetMessage<rollback_bet_cancel>("rollback_bet_cancel.xml");
            rollbackCancel.market = new market[0];
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(rollbackCancel), "Validation of empty markets should succeed");

            rollbackCancel = GetMessage<rollback_bet_cancel>("rollback_bet_cancel.xml");
            rollbackCancel.market = null;
            Assert.AreEqual(ValidationResult.FAILURE, _validator.Validate(rollbackCancel), "Validation of null markets should fail");

            rollbackCancel = GetMessage<rollback_bet_cancel>("rollback_bet_cancel.xml");
            rollbackCancel.market.First().specifiers = null;
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(rollbackCancel), "Validation of null specifiers should succeed");

            rollbackCancel = GetMessage<rollback_bet_cancel>("rollback_bet_cancel.xml");
            rollbackCancel.market.First().specifiers = "some_specifiers";
            Assert.AreEqual(ValidationResult.PROBLEMS_DETECTED, _validator.Validate(rollbackCancel), "Validation of specifiers should cause problems");
            Assert.AreEqual(rollbackCancel.market.Count(c => !c.ValidationFailed), rollbackCancel.market.Length - 1, "The number of valid markets is not correct");
        }

        [TestMethod]
        public void BetSettlementIsCorrectlyValidated()
        {
            var settlement = GetMessage<bet_settlement>("bet_settlement.xml");
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(settlement), "Validation of unchanged message should succeed");
            Assert.IsNotNull(settlement.EventURN, "EventURN of unchanged message should not be null");
            Assert.AreEqual(0, settlement.outcomes.Count(m => m.ValidationFailed), "All markets on unchanged message should be valid");
            Assert.AreEqual(settlement.outcomes.Count(m => !string.IsNullOrEmpty(m.specifiers)),
                settlement.outcomes.Count(m => m.Specifiers != null),
                "The number of markets with parsed specifiers should match to the number of markets with define specifiers");

            settlement = GetMessage<bet_settlement>("bet_settlement.xml");
            settlement.product = 50;
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(settlement), "Validation of product should be ok");

            settlement = GetMessage<bet_settlement>("bet_settlement.xml");
            settlement.event_id = "some_event_id";
            Assert.AreEqual(ValidationResult.FAILURE, _validator.Validate(settlement), "Validation of event_id should fail");

            settlement = GetMessage<bet_settlement>("bet_settlement.xml");
            settlement.outcomes = new betSettlementMarket[0];
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(settlement), "Validation of empty markets should succeed");

            settlement = GetMessage<bet_settlement>("bet_settlement.xml");
            settlement.outcomes = null;
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(settlement), "Validation of null markets should succeed");

            settlement = GetMessage<bet_settlement>("bet_settlement.xml");
            settlement.outcomes.First().specifiers = null;
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(settlement), "Validation of null specifiers should succeed");

            settlement = GetMessage<bet_settlement>("bet_settlement.xml");
            settlement.outcomes.First().specifiers = "some_specifiers";
            Assert.AreEqual(ValidationResult.PROBLEMS_DETECTED, _validator.Validate(settlement), "Validation of specifiers should cause problems");
            Assert.AreEqual(settlement.outcomes.Count(c => !c.ValidationFailed), settlement.outcomes.Length - 1, "The number of valid markets is not correct");
        }

        [TestMethod]
        public void RollbackBetSettlementIsCorrectlyValidated()
        {
            var rollback = GetMessage<rollback_bet_settlement>("rollback_bet_settlement.xml");
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(rollback), "Validation of unchanged message should succeed");
            Assert.IsNotNull(rollback.EventURN, "EventURN of unchanged message should not be null");
            Assert.AreEqual(0, rollback.market.Count(m => m.ValidationFailed), "All markets on unchanged message should be valid");
            Assert.AreEqual(rollback.market.Count(m => !string.IsNullOrEmpty(m.specifiers)),
                rollback.market.Count(m => m.Specifiers != null),
                "The number of markets with parsed specifiers should match to the number of markets with define specifiers");

            rollback = GetMessage<rollback_bet_settlement>("rollback_bet_settlement.xml");
            rollback.product = 50;
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(rollback), "Validation of product should be ok");

            rollback = GetMessage<rollback_bet_settlement>("rollback_bet_settlement.xml");
            rollback.event_id = "some_event_id";
            Assert.AreEqual(ValidationResult.FAILURE, _validator.Validate(rollback), "Validation of event_id should fail");

            rollback = GetMessage<rollback_bet_settlement>("rollback_bet_settlement.xml");
            rollback.market = new market[0];
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(rollback), "Validation of empty markets should succeed");

            rollback = GetMessage<rollback_bet_settlement>("rollback_bet_settlement.xml");
            rollback.market = null;
            Assert.AreEqual(ValidationResult.FAILURE, _validator.Validate(rollback), "Validation of null markets should fail");

            rollback = GetMessage<rollback_bet_settlement>("rollback_bet_settlement.xml");
            rollback.market.First().specifiers = null;
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(rollback), "Validation of null specifiers should succeed");

            rollback = GetMessage<rollback_bet_settlement>("rollback_bet_settlement.xml");
            rollback.market.First().specifiers = "some_specifiers";
            Assert.AreEqual(ValidationResult.PROBLEMS_DETECTED, _validator.Validate(rollback), "Validation of specifiers should cause problems");
            Assert.AreEqual(rollback.market.Count(c => !c.ValidationFailed), rollback.market.Length - 1, "The number of valid markets is not correct");
        }

        [TestMethod]
        public void OddsChangeIssuesIsCorrectlyValidated()
        {
            var oddsChange = GetMessage<odds_change>("odds_change.xml");
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(oddsChange), "Validation of unchanged message should succeed");
            Assert.IsNotNull(oddsChange.EventURN, "EventURN of unchanged message should not be null");
            Assert.AreEqual(0, oddsChange.odds.market.Count(m => m.ValidationFailed), "All markets on unchanged message should be valid");
            Assert.AreEqual(oddsChange.odds.market.Count(m => !string.IsNullOrEmpty(m.specifiers)),
                oddsChange.odds.market.Count(m => m.Specifiers != null),
                "The number of markets with parsed specifiers should match to the number of markets with define specifiers");

            oddsChange = GetMessage<odds_change>("odds_change.xml");
            oddsChange.product = 50;
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(oddsChange), "Validation of product should be ok");

            oddsChange = GetMessage<odds_change>("odds_change.xml");
            oddsChange.event_id = "some_event_id";
            Assert.AreEqual(ValidationResult.FAILURE, _validator.Validate(oddsChange), "Validation of event_id should fail");

            oddsChange = GetMessage<odds_change>("odds_change.xml");
            oddsChange.odds_change_reason = 100;
            oddsChange.odds_change_reasonSpecified = true;
            Assert.AreEqual(ValidationResult.PROBLEMS_DETECTED, _validator.Validate(oddsChange), "Validation of odds_change_reason should cause problems");

            oddsChange = GetMessage<odds_change>("odds_change.xml");
            oddsChange.odds = null;
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(oddsChange), "Validation of null odds should succeed");

            oddsChange = GetMessage<odds_change>("odds_change.xml");
            oddsChange.odds.betstop_reason = 100;
            oddsChange.odds.betstop_reasonSpecified = true;
            Assert.AreEqual(ValidationResult.PROBLEMS_DETECTED, _validator.Validate(oddsChange), "Validation of betstop_reason should cause problems");

            oddsChange = GetMessage<odds_change>("odds_change.xml");
            oddsChange.odds.betting_status = 100;
            oddsChange.odds.betting_statusSpecified = true;
            Assert.AreEqual(ValidationResult.PROBLEMS_DETECTED, _validator.Validate(oddsChange), "Validation of betting_status should cause problems");

            oddsChange = GetMessage<odds_change>("odds_change.xml");
            oddsChange.odds.market = null;
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(oddsChange), "Validation of null markets should succeed");

            oddsChange = GetMessage<odds_change>("odds_change.xml");
            oddsChange.odds.market.First().outcome = null;
            Assert.AreEqual(ValidationResult.SUCCESS, _validator.Validate(oddsChange), "Validation of null outcomes should succeed");

            oddsChange = GetMessage<odds_change>("odds_change.xml");
            oddsChange.odds.market.First().outcome.First().active = 100;
            Assert.AreEqual(ValidationResult.PROBLEMS_DETECTED, _validator.Validate(oddsChange), "Validation of outcome[0].active should cause problems");

            oddsChange = GetMessage<odds_change>("odds_change.xml");
            oddsChange.odds.market.First().specifiers = "some_specifiers";
            Assert.AreEqual(ValidationResult.PROBLEMS_DETECTED, _validator.Validate(oddsChange), "Validation of specifiers should cause problems");
            Assert.AreEqual(oddsChange.odds.market.Count(c => !c.ValidationFailed), oddsChange.odds.market.Length - 1, "The number of valid markets is not correct");
        }
    }
}
