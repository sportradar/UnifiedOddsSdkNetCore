/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class SportEventsScheduleMapperTest
    {
        private static readonly IDeserializer<scheduleEndpoint> Deserializer = new Deserializer<scheduleEndpoint>();

        private scheduleEndpoint _record;

        [TestInitialize]
        public void Setup()
        {
            _record = Deserializer.Deserialize(FileHelper.OpenFile(TestData.RestXmlPath, "events.xml"));
        }

        [TestMethod]
        public void MappedScheduleIsNotNull()
        {
            var schedule = new SportEventsScheduleMapperFactory().CreateMapper(_record).Map();
            Assert.IsNotNull(schedule);
            Assert.AreEqual(914, schedule.Items.Count(), "Value schedule.Schedule.Count() is not correct");
        }
    }
}
