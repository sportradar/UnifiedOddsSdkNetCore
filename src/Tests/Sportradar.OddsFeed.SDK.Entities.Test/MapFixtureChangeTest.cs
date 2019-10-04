/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.Test
{
    [TestClass]
    public class MapFixtureChangeTest : MapEntityTestBase
    {
        private static readonly IEnumerable<CultureInfo> Cultures = new[] { TestData.Culture };

        [TestMethod]
        public void FixtureChangeIsMapped()
        {
            var record = Load<fixture_change>("fixture_change.xml", URN.Parse("sr:sport:1000"), Cultures);
            TestData.FillMessageTimestamp(record);
            var entity = Mapper.MapFixtureChange<ICompetition>(record, Cultures, null);
            Assert.IsNotNull(entity, "entity should not be a null reference");
        }

        [TestMethod]
        public void TestFixtureChangeMapping()
        {
            var record = Load<fixture_change>("fixture_change.xml", URN.Parse("sr:sport:1000"), Cultures);
            TestData.FillMessageTimestamp(record);
            var entity = Mapper.MapFixtureChange<ICompetition>(record, Cultures, null);
            var assertHelper = new AssertHelper(entity);
            TestEntityValues(entity, record, assertHelper);
        }

        private void TestEntityValues(IFixtureChange<ICompetition> entity, fixture_change record, AssertHelper assertHelper)
        {
            TestEventMessageProperties(assertHelper, entity, record.timestamp, record.product, record.event_id, record.RequestId);
            assertHelper.AreEqual(() => (int)entity.ChangeType, record.change_typeSpecified
                                                                ? (int?)record.change_type
                                                                : (int)FixtureChangeType.OTHER);
            assertHelper.AreEqual(() => entity.NextLiveTime, record.next_live_timeSpecified
                                                                ? (long?)record.next_live_time
                                                                : null);
            assertHelper.AreEqual(() => entity.StartTime, record.start_time);
        }
    }
}