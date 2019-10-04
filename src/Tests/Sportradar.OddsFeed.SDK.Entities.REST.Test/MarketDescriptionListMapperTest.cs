/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class MarketDescriptionListMapperTest
    {
        private const string InputXml = "invariant_market_descriptions.en.xml";

        private static EntityList<MarketDescriptionDTO> _entity;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            var deserializer = new Deserializer<market_descriptions>();
            var dataFetcher = new TestDataFetcher();
            var mapperFactory = new MarketDescriptionsMapperFactory();

            var dataProvider = new DataProvider<market_descriptions, EntityList<MarketDescriptionDTO>>(
                TestData.RestXmlPath + InputXml,
                dataFetcher,
                deserializer,
                mapperFactory);
            _entity = dataProvider.GetDataAsync("", "en").Result;
        }

        [TestMethod]
        public void TestInstanceIsNotNull()
        {
            Assert.IsNotNull(_entity, "IMarketDescriptionList instance cannot be a null reference");
            Assert.IsNotNull(_entity.Items, "Market descriptions cannot be a null reference");
            Assert.AreEqual(750, _entity.Items.Count(), "The number of market description in fetched entity is not correct");
        }
    }
}
