/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
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
    public class MapFixtureChangeTests : MapEntityTestBase
    {
        private static readonly IEnumerable<CultureInfo> Cultures = new[] { TestData.Culture };

        /// <inheritdoc />
        public MapFixtureChangeTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void FixtureChangeIsMapped()
        {
            var record = Load<fixture_change>("fixture_change.xml", URN.Parse("sr:sport:1000"), Cultures);
            TestData.FillMessageTimestamp(record);
            var entity = Mapper.MapFixtureChange<ICompetition>(record, Cultures, null);
            Assert.NotNull(entity);
        }

        [Fact]
        public void TestFixtureChangeMapping()
        {
            var record = Load<fixture_change>("fixture_change.xml", URN.Parse("sr:sport:1000"), Cultures);
            TestData.FillMessageTimestamp(record);
            var entity = Mapper.MapFixtureChange<ICompetition>(record, Cultures, null);
            TestEntityValues(entity, record);
        }

        [Fact]
        public void TestFixtureChangeTypeMapping()
        {
            var record = Load<fixture_change>("fixture_change.xml", URN.Parse("sr:sport:1000"), Cultures);
            TestData.FillMessageTimestamp(record);
            record.change_typeSpecified = true;
            record.change_type = 1;
            var typeValidation = ValidateFixtureChangeType(record);
            Assert.Equal(ValidationResult.Success, typeValidation);
            var entity = Mapper.MapFixtureChange<ICompetition>(record, Cultures, null);
            Assert.NotNull(entity);
            Assert.Equal(FixtureChangeType.NEW, entity.ChangeType);
        }

        [Fact]
        public void TestFixtureChangeTypeMappingOther()
        {
            var record = Load<fixture_change>("fixture_change.xml", URN.Parse("sr:sport:1000"), Cultures);
            TestData.FillMessageTimestamp(record);
            record.change_typeSpecified = true;
            record.change_type = 10;
            var typeValidation = ValidateFixtureChangeType(record);
            Assert.Equal(ValidationResult.ProblemsDetected, typeValidation);
            var entity = Mapper.MapFixtureChange<ICompetition>(record, Cultures, null);
            Assert.NotNull(entity);
            Assert.Equal(FixtureChangeType.OTHER, entity.ChangeType);
        }

        [Fact]
        public void TestFixtureChangeTypeMappingNa()
        {
            var record = Load<fixture_change>("fixture_change.xml", URN.Parse("sr:sport:1000"), Cultures);
            TestData.FillMessageTimestamp(record);
            record.change_typeSpecified = false;
            record.change_type = 1;
            var typeValidation = ValidateFixtureChangeType(record);
            Assert.Equal(ValidationResult.Success, typeValidation);
            var entity = Mapper.MapFixtureChange<ICompetition>(record, Cultures, null);
            Assert.NotNull(entity);
            Assert.Equal(FixtureChangeType.NA, entity.ChangeType);
        }

        private void TestEntityValues(IFixtureChange<ICompetition> entity, fixture_change record)
        {
            TestEventMessageProperties(entity, record.timestamp, record.product, record.event_id, record.RequestId);
            Assert.Equal((int)entity.ChangeType, record.change_typeSpecified
                                                                ? (int?)record.change_type
                                                                : (int)FixtureChangeType.OTHER);
            Assert.Equal(entity.NextLiveTime, record.next_live_timeSpecified
                                                                ? (long?)record.next_live_time
                                                                : null);
            Assert.Equal(entity.StartTime, record.start_time);
        }

        private ValidationResult ValidateFixtureChangeType(fixture_change message)
        {
            if (message.change_typeSpecified && !MessageMapperHelper.IsEnumMember<FixtureChangeType>(message.change_type))
            {
                return ValidationResult.ProblemsDetected;
            }
            return ValidationResult.Success;
        }
    }
}
