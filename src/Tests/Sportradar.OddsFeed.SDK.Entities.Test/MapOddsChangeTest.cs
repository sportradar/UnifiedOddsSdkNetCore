/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.Test
{
    [TestClass]
    public class MapOddsChangeTest : MapEntityTestBase
    {
        private static readonly IEnumerable<CultureInfo> Cultures = new[] { TestData.Culture };

        private static readonly IFeedMessageValidator Validator = new TestFeedMessageValidator();

        [TestMethod]
        public void OddsChangeWithoutOddsIsMapped()
        {
            var record = Load<odds_change>("odds_change.xml", URN.Parse("sr:sport:1000"), Cultures);
            TestData.FillMessageTimestamp(record);
            record.odds.market = null;
            var entity = Mapper.MapOddsChange<ICompetition>(record, Cultures, null);
            Assert.IsNotNull(entity, "OddsChange without odds could not be mapped");
        }

        [TestMethod]
        public void TestOddsChangeMapping()
        {
            var record = Load<odds_change>("odds_change.xml", URN.Parse("sr:sport:1000"), Cultures);
            TestData.FillMessageTimestamp(record);
            Validator.Validate(record);
            var entity = Mapper.MapOddsChange<ICompetition>(record, Cultures, null);
            var assertHelper = new AssertHelper(entity);
            TestEntityValues(entity, record, assertHelper);
        }

        private void TestEntityValues(IOddsChange<ICompetition> entity, odds_change record, AssertHelper assertHelper)
        {
            TestEventMessageProperties(assertHelper, entity, record.timestamp, record.product, record.event_id, record.RequestId);
            assertHelper.AreEqual(() => entity.ChangeReason,
                                  MessageMapperHelper.GetEnumValue<OddsChangeReason>(record.odds_change_reason));

            if (record.odds.betstop_reasonSpecified)
            {
                assertHelper.AreEqual(() => entity.BetStopReason.Id, record.odds.betstop_reason);
            }
            else
            {
                assertHelper.IsNull(() => entity.BetStopReason);
            }

            if (record.odds.betting_statusSpecified)
            {
                assertHelper.AreEqual(() => entity.BettingStatus.Id, record.odds.betting_status);
            }
            else
            {
                assertHelper.IsNull(() => entity.BettingStatus);
            }

            if (record.odds?.market == null || record.odds.market.Length == 0)
            {
                Assert.IsNull(entity.Markets);
            }
            else
            {
                foreach (var marketRecord in record.odds.market)
                {
                    var market = FindMarket(entity.Markets, marketRecord.id, marketRecord.specifiers);
                    Assert.IsNotNull(market, $"Market with id={marketRecord.id} does not exist on mapped message");
                    TestMarketValues(market, marketRecord);
                }
            }
        }

        private void TestMarketValues(IMarketWithOdds entity, oddsChangeMarket record)
        {
            Assert.AreEqual(entity.Id, record.id, "Market id is not correct");
            Assert.IsTrue(CompareSpecifiers(entity.Specifiers, record.specifiers), "The market specifiers do not match");
            Assert.IsTrue(CompareSpecifiers(entity.AdditionalInfo, record.extended_specifiers), "Market additional specifiers do not match");
            Assert.AreEqual(entity.CashoutStatus, record.cashout_statusSpecified ? (CashoutStatus?) MessageMapperHelper.GetEnumValue<CashoutStatus>(record.cashout_status) : null, $"Market id={entity.Id} cashout status do not match");
            Assert.AreEqual(entity.IsFavorite, record.favouriteSpecified && record.favourite == 1, $"Market id={entity.Id} favorite does not match");
            Assert.AreEqual(entity.Status, MessageMapperHelper.GetEnumValue<MarketStatus>(record.status), $"Market id={entity.Id} status does not match");


            if (record.outcome == null || record.outcome.Length == 0)
            {
                Assert.IsNull(entity.OutcomeOdds);
            }
            else
            {
                foreach (var outcomeRecord in record.outcome)
                {
                    var outcome = FindOutcome(entity.OutcomeOdds, outcomeRecord.id);
                    Assert.IsNotNull(outcome, $"Outcome marketId={entity.Id} id={outcomeRecord.id} does not exist");
                    TestOutcomeValues(outcome, outcomeRecord);
                }
            }
        }

        private void TestOutcomeValues(IOutcomeOdds entity, oddsChangeMarketOutcome record)
        {
            Assert.AreEqual(entity.Id, record.id, "Outcome Id does not match");
            Assert.AreEqual(entity.Active, record.activeSpecified ? (bool?)(record.active == 1) : null);
            Assert.AreEqual(entity.Odds, record.odds);
            Assert.AreEqual(entity.Probabilities, record.probabilitiesSpecified ? (double?) record.probabilities : null);

            var entityV1 = entity as IOutcomeOddsV1;
            Assert.IsNotNull(entityV1);
            Assert.AreEqual(entityV1.GetOdds(OddsDisplayType.Decimal), record.odds);
            Assert.IsNotNull(entityV1.GetOdds(OddsDisplayType.American));
            if (entityV1.GetOdds(OddsDisplayType.American).HasValue)
            {
                Assert.IsTrue(Math.Abs(entityV1.GetOdds(OddsDisplayType.American).Value) > 0);
            }
        }
    }
}