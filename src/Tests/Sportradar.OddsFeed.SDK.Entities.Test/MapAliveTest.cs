/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.Test
{
    [TestClass]
    public class MapAliveTest : MapEntityTestBase
    {
        [TestMethod]
        public void AliveIsMapped()
        {
            var record = Load<alive>("alive.xml", null, null);
            TestData.FillMessageTimestamp(record);
            var entity = Mapper.MapAlive(record);
            Assert.IsNotNull(entity, "entity should not be a null reference");
        }

        [TestMethod]
        public void TestAliveMapping()
        {
            var record = Load<alive>("alive.xml", null, null);
            TestData.FillMessageTimestamp(record);
            var entity = Mapper.MapAlive(record);
            var assertHelper = new AssertHelper(entity);
            TestEntityValues(entity, record, assertHelper);
        }

        private void TestEntityValues(IAlive entity, alive message, AssertHelper assertHelper)
        {
            TestMessageProperties(assertHelper, entity, message.timestamp, message.product);
            assertHelper.AreEqual(() => entity.Subscribed, message.subscribed == 1);
        }
    }
}
