/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities
{
    public class MapBetStopTests : MapEntityTestBase
    {
        private static readonly IEnumerable<CultureInfo> Cultures = new[] { TestData.Culture };

        /// <inheritdoc />
        public MapBetStopTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void BetStopIsMapped()
        {
            var record = Load<bet_stop>("bet_stop.xml", URN.Parse("sr:sport:1000"), Cultures);
            TestData.FillMessageTimestamp(record);
            var entity = Mapper.MapBetStop<ICompetition>(record, Cultures, null);
            Assert.NotNull(entity);
        }

        [Fact]
        public void TestBetStopMapping()
        {
            var record = Load<bet_stop>("bet_stop.xml", URN.Parse("sr:sport:1000"), Cultures);
            TestData.FillMessageTimestamp(record);
            var entity = Mapper.MapBetStop<ICompetition>(record, Cultures, null);
            TestEntityValues(entity, record);
        }

        private void TestEntityValues(IBetStop<ICompetition> entity, bet_stop record)
        {
            TestEventMessageProperties(entity, record.timestamp, record.product, record.event_id, record.RequestId);

            var recordGroupsCount = record.groups?.Split(new[] { SdkInfo.MarketGroupsDelimiter }, StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
            var entityGroupsCount = entity.Groups?.Count() ?? 0;
            Assert.Equal(entityGroupsCount, recordGroupsCount);
            Assert.Equal(entity.MarketStatus, MessageMapperHelper.GetEnumValue(record.market_statusSpecified, record.market_status, MarketStatus.SUSPENDED));
        }
    }
}
