using System;
using Sportradar.OddsFeed.SDK.API.Internal.Replay;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal.Log;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API
{
    public class HttpDataRestfulTests
    {
        private readonly Uri _badUri = new Uri("http://www.unexisting-url.com");
        private readonly Uri _putUri = new Uri("http://httpbin.org/put");
        private readonly Uri _deleteUri = new Uri("http://httpbin.org/delete");

        private readonly HttpDataRestful _httpDataRestful;

        public HttpDataRestfulTests()
        {
            var config = TestConfigurationInternal.GetConfig();

            object[] args =
            {
                new TestHttpClient(),
                config.AccessToken,
                new Deserializer<response>(),
                5,
                5
            };

            _httpDataRestful = LogInterceptorFactory.Create<HttpDataRestful>(args, null, LoggerType.RestTraffic);
        }

        //TODO requires network
        //[Fact]
        public void PutDataAsyncTest()
        {
            // in logRest file there should be result for this call
            var result = _httpDataRestful.PutDataAsync(_putUri).Result;
            Assert.NotNull(result);
            Assert.True(result.IsSuccessStatusCode);
        }

        //[Fact]
        public void PutDataAsyncTestWithWrongUrl()
        {
            // in logRest file there should be result for this call
            var ex = Assert.Throws<AggregateException>(() => _httpDataRestful.PutDataAsync(_badUri).Result);
            Assert.IsType<AggregateException>(ex);
            if (ex.InnerException != null)
            {
                Assert.IsType<CommunicationException>(ex.InnerException);
            }
        }

        //TODO requires network
        //[Fact]
        public void DeleteDataAsyncTest()
        {
            var result = _httpDataRestful.DeleteDataAsync(_deleteUri).Result;
            Assert.NotNull(result);
            Assert.True(result.IsSuccessStatusCode);
        }

        //[Fact]
        public void DeleteDataAsyncTestWithWrongUrl()
        {
            var ex = Assert.Throws<AggregateException>(() => _httpDataRestful.DeleteDataAsync(_badUri).Result);
            Assert.IsType<AggregateException>(ex);
            if (ex.InnerException != null)
            {
                Assert.IsType<CommunicationException>(ex.InnerException);
            }
        }
    }
}
