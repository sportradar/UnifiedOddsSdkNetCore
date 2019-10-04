/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class TournamentTest
    {
        private const string TourInfoXml = "tournament_info.xml";

        private static TournamentInfoDTO _entity;

        [TestInitialize]
        public void Init()
        {
            var deserializer = new Deserializer<tournamentInfoEndpoint>();
            var dataFetcher = new TestDataFetcher();
            var mapperFactory = new TournamentInfoMapperFactory();

            var dataProvider = new DataProvider<tournamentInfoEndpoint, TournamentInfoDTO>(
                TestData.RestXmlPath + TourInfoXml,
                dataFetcher,
                deserializer,
                mapperFactory);
            _entity = dataProvider.GetDataAsync("", "en").Result;
        }

        [TestMethod]
        public void TestsAreInSportEntityFactoryTest()
        {
            Assert.IsNotNull(_entity);
        }
    }
}
