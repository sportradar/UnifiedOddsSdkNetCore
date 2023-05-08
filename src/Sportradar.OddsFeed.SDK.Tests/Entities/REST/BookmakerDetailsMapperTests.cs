/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Net;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal.Log;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class BookmakerDetailsMapperTests
    {
        private const string InputXml = "bookmaker_details.xml";
        private readonly BookmakerDetailsDTO _entity;
        private readonly BookmakerDetailsFetcher _bookmakerDetailsFetcher;

        public BookmakerDetailsMapperTests()
        {
            var deserializer = new Deserializer<bookmaker_details>();
            var dataFetcher = new TestDataFetcher();
            var mapperFactory = new BookmakerDetailsMapperFactory();

            var dataProvider = new DataProvider<bookmaker_details, BookmakerDetailsDTO>(
                TestData.RestXmlPath + InputXml,
                dataFetcher,
                deserializer,
                mapperFactory);
            _entity = dataProvider.GetDataAsync("", TestData.Culture.TwoLetterISOLanguageName).GetAwaiter().GetResult();

            object[] args =
            {
                dataProvider
            };
            _bookmakerDetailsFetcher = LogInterceptorFactory.Create<BookmakerDetailsFetcher>(args, null, LoggerType.ClientInteraction);
        }

        [Fact]
        public void TestInstanceIsNotNull()
        {
            Assert.NotNull(_entity);
        }

        [Fact]
        public void Mapping()
        {
            var details = new BookmakerDetails(_entity);
            ValidateBookmakerDetailsFromXml(details);
        }

        [Fact]
        public void WhoAmI()
        {
            var details = _bookmakerDetailsFetcher.WhoAmIAsync().GetAwaiter().GetResult();
            ValidateBookmakerDetailsFromXml(details);
        }

        private static void ValidateBookmakerDetailsFromXml(IBookmakerDetails details)
        {
            Assert.Equal(TestData.BookmakerId, details.BookmakerId);
            Assert.Equal(TestData.VirtualHost, details.VirtualHost);
            Assert.Null(details.Message);
            Assert.Equal(HttpStatusCode.OK, details.ResponseCode);
            Assert.Equal(DateTime.Parse("2016-07-26T17:44:24Z").ToUniversalTime(), details.ExpireAt.ToUniversalTime());
        }
    }
}
