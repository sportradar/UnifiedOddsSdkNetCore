/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
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
            Assert.IsNotNull(entity.ChangeType);
        }

        [TestMethod]
        public void TestFixtureChangeTypeMapping()
        {
            var record = Load<fixture_change>("fixture_change.xml", URN.Parse("sr:sport:1000"), Cultures);
            TestData.FillMessageTimestamp(record);
            record.change_typeSpecified = true;
            record.change_type = 1;
            var typeValidation = ValidateFixtureChangeType(record);
            Assert.AreEqual(ValidationResult.SUCCESS, typeValidation);
            var entity = Mapper.MapFixtureChange<ICompetition>(record, Cultures, null);
            Assert.IsNotNull(entity);
            Assert.AreEqual(FixtureChangeType.NEW, entity.ChangeType);
        }

        [TestMethod]
        public void TestFixtureChangeTypeMappingOther()
        {
            var record = Load<fixture_change>("fixture_change.xml", URN.Parse("sr:sport:1000"), Cultures);
            TestData.FillMessageTimestamp(record);
            record.change_typeSpecified = true;
            record.change_type = 10;
            var typeValidation = ValidateFixtureChangeType(record);
            Assert.AreEqual(ValidationResult.PROBLEMS_DETECTED, typeValidation);
            var entity = Mapper.MapFixtureChange<ICompetition>(record, Cultures, null);
            Assert.IsNotNull(entity);
            Assert.AreEqual(FixtureChangeType.OTHER, entity.ChangeType);
        }

        [TestMethod]
        public void TestFixtureChangeTypeMappingNa()
        {
            var record = Load<fixture_change>("fixture_change.xml", URN.Parse("sr:sport:1000"), Cultures);
            TestData.FillMessageTimestamp(record);
            record.change_typeSpecified = false;
            record.change_type = 1;
            var typeValidation = ValidateFixtureChangeType(record);
            Assert.AreEqual(ValidationResult.SUCCESS, typeValidation);
            var entity = Mapper.MapFixtureChange<ICompetition>(record, Cultures, null);
            Assert.IsNotNull(entity);
            Assert.AreEqual(FixtureChangeType.NA, entity.ChangeType);
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

        private ValidationResult ValidateFixtureChangeType(fixture_change message)
        {
            if (message.change_typeSpecified && !MessageMapperHelper.IsEnumMember<FixtureChangeType>(message.change_type))
            {
                return ValidationResult.PROBLEMS_DETECTED;
            }
            return ValidationResult.SUCCESS;
        }
    }
}