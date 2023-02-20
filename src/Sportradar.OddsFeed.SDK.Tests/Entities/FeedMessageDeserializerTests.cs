/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities
{
    public class FeedMessageDeserializerTests
    {
        /// <summary>
        /// Class under test
        /// </summary>
        private static readonly IDeserializer<FeedMessage> Deserializer = new Deserializer<FeedMessage>();

        [Fact]
        public void InvalidOperationExceptionIsThrownWhenDeserializationFails()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "odds_change.xml");
            byte[] fileData;
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                fileData = memoryStream.ToArray();
            }
            var list = new List<byte>(fileData.Length);
            list.AddRange(fileData.Take(105));
            list.AddRange(fileData.Skip(300).Take(fileData.Length - 300));

            Action action = () => Deserializer.Deserialize<odds_change>(new MemoryStream(list.ToArray()));
            action.Should().Throw<DeserializationException>();
        }

        [Fact]
        public void AliveIsDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, @"alive.xml");
            var alive = Deserializer.Deserialize<alive>(stream);
            Assert.NotNull(alive);
        }

        [Fact]
        public void BetSettlementIsDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, @"bet_settlement.xml");
            var betSettlement = Deserializer.Deserialize<bet_settlement>(stream);
            Assert.NotNull(betSettlement);
        }

        [Fact]
        public void BetStopIsDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, @"bet_stop.xml");
            var betStop = Deserializer.Deserialize<bet_stop>(stream);
            Assert.NotNull(betStop);
        }

        [Fact]
        public void FixtureChangeIsDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, @"fixture_change.xml");
            var fixtureChange = Deserializer.Deserialize<fixture_change>(stream);
            Assert.NotNull(fixtureChange);
        }

        [Fact]
        public void OddsChangeIsDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, @"odds_change.xml");
            var oddsChange = Deserializer.Deserialize<odds_change>(stream);
            Assert.NotNull(oddsChange);
        }
    }
}
