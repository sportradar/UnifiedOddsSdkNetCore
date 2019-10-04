/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Net;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class BookmakerDetailsProviderTest
    {
        private const string InputXml = "whoami.xml";

        public static IDataProvider<BookmakerDetailsDTO> BuildProvider(string apiKey)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            var fetcher = string.IsNullOrEmpty(apiKey)
                ? new TestDataFetcher()
                : (IDataFetcher) new HttpDataFetcher(new HttpClient(), apiKey, new Deserializer<response>());
            var url = string.IsNullOrEmpty(apiKey)
                ? TestData.RestXmlPath
                : "https://api.betradar.com/v1/users/";
            var deserializer = new Deserializer<bookmaker_details>();
            var mapperFactory = new BookmakerDetailsMapperFactory();

            return  new DataProvider<bookmaker_details, BookmakerDetailsDTO>(
                url + InputXml,
                fetcher,
                deserializer,
                mapperFactory);
        }

        [TestMethod]
        public void CorrectKeyProducesResponseStatusOk()
        {
            var provider = BuildProvider(null);
            var dto = provider.GetDataAsync(new string[1]).Result;

            Assert.IsNotNull(dto, "returned object should not be null");
            Assert.AreEqual(HttpStatusCode.OK, dto.ResponseCode, "Response code should be OK");
        }

        [TestMethod]
        public void IncorrectKeyProducesResponseCodeForbidden()
        {
            var provider = BuildProvider("aaaaaaaaaaa");
            CommunicationException exception = null;
            try
            {
                var dto = provider.GetDataAsync(new string[1]).Result;
                Assert.IsNotNull(dto);
            }
            catch (AggregateException ex)
            {
                exception = (CommunicationException) ex.InnerException;
            }

            Assert.IsNotNull(exception, "returned object should not be null");
            Assert.AreEqual(HttpStatusCode.Forbidden, exception.ResponseCode, "Response code of the exception should be forbidden");
        }
    }
}
