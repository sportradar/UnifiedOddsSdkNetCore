/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.Test
{
    [TestClass]
    public class MapBetStopTest : MapEntityTestBase
    {
        private static readonly IEnumerable<CultureInfo> Cultures = new[] { TestData.Culture };

        [TestMethod]
        public void BetStopIsMapped()
        {
            var record = Load<bet_stop>("bet_stop.xml", URN.Parse("sr:sport:1000"), Cultures);
            TestData.FillMessageTimestamp(record);
            var entity = Mapper.MapBetStop<ICompetition>(record, Cultures, null);
            Assert.IsNotNull(entity, "entity should not be a null reference");
        }

        [TestMethod]
        public void TestBetStopMapping()
        {
            var record = Load<bet_stop>("bet_stop.xml", URN.Parse("sr:sport:1000"), Cultures);
            TestData.FillMessageTimestamp(record);
            var entity = Mapper.MapBetStop<ICompetition>(record, Cultures, null);
            var assertHelper = new AssertHelper(entity);
            TestEntityValues(entity, record, assertHelper);
        }

        private void TestEntityValues(IBetStop<ICompetition> entity, bet_stop record, AssertHelper assertHelper)
        {
            TestEventMessageProperties(assertHelper, entity, record.timestamp, record.product, record.event_id, record.RequestId);

            var recordGroupsCount = record.groups?.Split(new[] {SdkInfo.MarketGroupsDelimiter}, StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
            var entityGroupsCount = entity.Groups?.Count() ?? 0;
            Assert.AreEqual(entityGroupsCount, recordGroupsCount);
            //assertHelper.AreEqual(() => entityGroupsCount, recordGroupsCount);
            assertHelper.AreEqual(() => entity.MarketStatus, MessageMapperHelper.GetEnumValue(record.market_statusSpecified, record.market_status, MarketStatus.SUSPENDED));
        }
    }
}
