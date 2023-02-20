/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities
{
    public class MapOddsChangeTests : MapEntityTestBase
    {
        private static readonly IEnumerable<CultureInfo> Cultures = new[] { TestData.Culture };

        private static readonly IFeedMessageValidator Validator = new TestFeedMessageValidator();

        /// <inheritdoc />
        public MapOddsChangeTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void OddsChangeWithoutOddsIsMapped()
        {
            var record = Load<odds_change>("odds_change.xml", URN.Parse("sr:sport:1000"), Cultures);
            TestData.FillMessageTimestamp(record);
            record.odds.market = null;
            var entity = Mapper.MapOddsChange<ICompetition>(record, Cultures, null);
            Assert.NotNull(entity);
        }

        [Fact]
        public void TestOddsChangeMapping()
        {
            var record = Load<odds_change>("odds_change.xml", URN.Parse("sr:sport:1000"), Cultures);
            TestData.FillMessageTimestamp(record);
            Validator.Validate(record);
            var entity = Mapper.MapOddsChange<ICompetition>(record, Cultures, null);
            TestEntityValues(entity, record);
        }

        private void TestEntityValues(IOddsChange<ICompetition> entity, odds_change record)
        {
            TestEventMessageProperties(entity, record.timestamp, record.product, record.event_id, record.RequestId);
            Assert.Equal(entity.ChangeReason, MessageMapperHelper.GetEnumValue<OddsChangeReason>(record.odds_change_reason));

            if (record.odds.betstop_reasonSpecified)
            {
                Assert.Equal(entity.BetStopReason.Id, record.odds.betstop_reason);
            }
            else
            {
                Assert.Null(entity.BetStopReason);
            }

            if (record.odds.betting_statusSpecified)
            {
                Assert.Equal(entity.BettingStatus.Id, record.odds.betting_status);
            }
            else
            {
                Assert.Null(entity.BettingStatus);
            }

            if (record.odds?.market == null || record.odds.market.Length == 0)
            {
                Assert.Null(entity.Markets);
            }
            else
            {
                foreach (var marketRecord in record.odds.market)
                {
                    var market = FindMarket(entity.Markets, marketRecord.id, marketRecord.specifiers);
                    Assert.NotNull(market);
                    TestMarketValues(market, marketRecord);
                }
            }
        }

        private void TestMarketValues(IMarketWithOdds entity, oddsChangeMarket record)
        {
            Assert.Equal(entity.Id, record.id);
            Assert.True(CompareSpecifiers(entity.Specifiers, record.specifiers), "The market specifiers do not match");
            Assert.True(CompareSpecifiers(entity.AdditionalInfo, record.extended_specifiers), "Market additional specifiers do not match");
            Assert.Equal(entity.CashoutStatus, record.cashout_statusSpecified ? (CashoutStatus?)MessageMapperHelper.GetEnumValue<CashoutStatus>(record.cashout_status) : null);
            Assert.Equal(entity.IsFavorite, record.favouriteSpecified && record.favourite == 1);
            Assert.Equal(entity.Status, MessageMapperHelper.GetEnumValue<MarketStatus>(record.status));

            if (record.outcome == null || record.outcome.Length == 0)
            {
                Assert.Null(entity.OutcomeOdds);
            }
            else
            {
                foreach (var outcomeRecord in record.outcome)
                {
                    var outcome = FindOutcome(entity.OutcomeOdds, outcomeRecord.id);
                    Assert.NotNull(outcome);
                    TestOutcomeValues(outcome, outcomeRecord);
                }
            }
        }

        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        private void TestOutcomeValues(IOutcomeOdds entity, oddsChangeMarketOutcome record)
        {
            Assert.NotNull(entity);
            Assert.Equal(entity.Id, record.id);
            Assert.Equal(entity.Active, record.activeSpecified ? (bool?)(record.active == 1) : null);
            Assert.Equal(entity.Probabilities, record.probabilitiesSpecified ? (double?)record.probabilities : null);
            Assert.Equal(entity.GetOdds(), record.odds);
            Assert.NotNull(entity.GetOdds(OddsDisplayType.American));
            if (entity.GetOdds(OddsDisplayType.American).HasValue)
            {
                Assert.True(Math.Abs(entity.GetOdds(OddsDisplayType.American).Value) > 0);
            }
        }
    }
}
