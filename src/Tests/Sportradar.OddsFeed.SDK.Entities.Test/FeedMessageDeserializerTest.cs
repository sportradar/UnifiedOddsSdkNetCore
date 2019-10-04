/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.Test
{
    [TestClass]
    public class FeedMessageDeserializerTest
    {
        /// <summary>
        /// Class under test
        /// </summary>
        private static readonly IDeserializer<FeedMessage> Deserializer = new Deserializer<FeedMessage>();

        [TestMethod]
        [ExpectedException(typeof(DeserializationException))]
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
            Deserializer.Deserialize<odds_change>(new MemoryStream(list.ToArray()));
        }

        [TestMethod]
        public void AliveIsDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, @"alive.xml");
            var alive = Deserializer.Deserialize<alive>(stream);
            Assert.IsNotNull(alive, $"Failed to deserialize {typeof(alive).Name} instance");
        }

        [TestMethod]
        public void BetSettlementIsDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath,@"bet_settlement.xml");
            var betSettlement = Deserializer.Deserialize<bet_settlement>(stream);
            Assert.IsNotNull(betSettlement, $"Failed to deserialize {typeof(bet_settlement).Name} instance");
        }

        [TestMethod]
        public void BetStopIsDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, @"bet_stop.xml");
            var betStop = Deserializer.Deserialize<bet_stop>(stream);
            Assert.IsNotNull(betStop, $"Failed to deserialize {typeof(bet_stop).Name} instance");
        }

        [TestMethod]
        public void FixtureChangeIsDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, @"fixture_change.xml");
            var fixtureChange = Deserializer.Deserialize<fixture_change>(stream);
            Assert.IsNotNull(fixtureChange, $"Failed to deserialize {typeof(fixture_change).Name} instance");
        }

        [TestMethod]
        public void OddsChangeIsDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, @"odds_change.xml");
            var oddsChange = Deserializer.Deserialize<odds_change>(stream);
            Assert.IsNotNull(oddsChange, $"Failed to deserialize {typeof(odds_change).Name} instance");
        }
    }
}
