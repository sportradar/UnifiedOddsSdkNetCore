/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Log;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class BookmakerDetailsMapperTest
    {
        private const string InputXml = "bookmaker_details.xml";

        private static BookmakerDetailsDTO _entity;

        private BookmakerDetailsFetcher _bookmakerDetailsFetcher;

        [TestInitialize]
        public void Init()
        {
            var deserializer = new Deserializer<bookmaker_details>();
            var dataFetcher = new TestDataFetcher();
            var mapperFactory = new BookmakerDetailsMapperFactory();

            var dataProvider = new DataProvider<bookmaker_details, BookmakerDetailsDTO>(
                TestData.RestXmlPath + InputXml,
                dataFetcher,
                deserializer,
                mapperFactory);
            _entity = dataProvider.GetDataAsync("", TestData.Culture.TwoLetterISOLanguageName).Result;

            object[] args =
            {
                dataProvider
            };
            _bookmakerDetailsFetcher = LogProxyFactory.Create<BookmakerDetailsFetcher>(args, LoggerType.ClientInteraction);
        }

        [TestMethod]
        public void TestInstanceIsNotNull()
        {
            Assert.IsNotNull(_entity, "BookmakerDetailsDTO instance cannot be a null reference");
        }

        [TestMethod]
        public void MappingTest()
        {
            var details = new BookmakerDetails(_entity);
            ValidateBookmakerDetailsFromXml(details);
        }

        [TestMethod]
        public void WhoAmITest()
        {
            var details = _bookmakerDetailsFetcher.WhoAmIAsync().Result;
            ValidateBookmakerDetailsFromXml(details);
        }

        private static void ValidateBookmakerDetailsFromXml(IBookmakerDetails details)
        {
            Assert.AreEqual(TestData.BookmakerId, details.BookmakerId);
            Assert.AreEqual(TestData.VirtualHost, details.VirtualHost);
            Assert.IsNull(details.Message);
            Assert.AreEqual(HttpStatusCode.OK, details.ResponseCode);
            Assert.AreEqual(DateTime.Parse("2016-07-26T17:44:24Z").ToUniversalTime(), details.ExpireAt.ToUniversalTime());
        }
    }
}
