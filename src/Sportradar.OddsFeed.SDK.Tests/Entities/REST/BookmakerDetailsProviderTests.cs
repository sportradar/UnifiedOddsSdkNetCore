// /*
// * Copyright (C) Sportradar AG. See LICENSE for full license governing this code
// */

using System;
using System.Net;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class BookmakerDetailsProviderTests
    {
        private const string InputXml = "whoami.xml";

        private static IDataProvider<BookmakerDetailsDTO> BuildProvider(string apiKey)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            var fetcher = string.IsNullOrEmpty(apiKey)
                ? new TestDataFetcher()
                : (IDataFetcher)new HttpDataFetcher(new TestHttpClient(), new Deserializer<response>());
            var url = string.IsNullOrEmpty(apiKey)
                ? TestData.RestXmlPath
                : "https://api.betradar.com/v1/users/";
            var deserializer = new Deserializer<bookmaker_details>();
            var mapperFactory = new BookmakerDetailsMapperFactory();

            return new DataProvider<bookmaker_details, BookmakerDetailsDTO>(
                url + InputXml,
                fetcher,
                deserializer,
                mapperFactory);
        }

        [Fact]
        public void CorrectKeyProducesResponseStatusOk()
        {
            var provider = BuildProvider(null);
            var dto = provider.GetDataAsync(new string[1]).GetAwaiter().GetResult();

            Assert.NotNull(dto);
            Assert.Equal(HttpStatusCode.OK, dto.ResponseCode);
        }

        //TODO requires network
        //[Fact]
        public void IncorrectKeyProducesResponseCodeForbidden()
        {
            var provider = BuildProvider("aaaaaaaaaaa");
            CommunicationException exception = null;
            try
            {
                var dto = provider.GetDataAsync(new string[1]).GetAwaiter().GetResult();
                Assert.NotNull(dto);
            }
            catch (AggregateException ex)
            {
                exception = (CommunicationException)ex.InnerException;
            }

            Assert.NotNull(exception);
            Assert.Equal(HttpStatusCode.Forbidden, exception.ResponseCode);
        }
    }
}
