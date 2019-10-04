/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.API.Internal.Replay;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal.Log;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.API.Test
{
    [TestClass]
    public class HttpDataRestfulTest
    {
        private readonly Uri _badUri = new Uri("http://www.unexisting-url.com");
        private readonly Uri _putUri = new Uri("http://httpbin.org/put");
        private readonly Uri _deleteUri = new Uri("http://httpbin.org/delete");

        private HttpDataRestful _httpDataRestful;

        [TestInitialize]
        public void Init()
        {
            var config = TestConfigurationInternal.GetConfig();

            object[] args =
            {
                new HttpClient(),
                config.AccessToken,
                new Deserializer<response>(),
                5,
                5
            };

            _httpDataRestful = LogProxyFactory.Create<HttpDataRestful>(args, LoggerType.RestTraffic);
        }

        [TestMethod]
        public void PutDataAsyncTest()
        {
            // in logRest file there should be result for this call
            var result = _httpDataRestful.PutDataAsync(_putUri).Result;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessStatusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(CommunicationException))]
        public void PutDataAsyncTestWithWrongUrl()
        {
            // in logRest file there should be result for this call
            try
            {
                var result = _httpDataRestful.PutDataAsync(_badUri).Result;
                Assert.IsNotNull(result);
                Assert.IsFalse(result.IsSuccessStatusCode);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }
            }
        }

        [TestMethod]
        public void DeleteDataAsyncTest()
        {
            var result = _httpDataRestful.DeleteDataAsync(_deleteUri).Result;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessStatusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(CommunicationException))]
        public void DeleteDataAsyncTestWithWrongUrl()
        {
            try
            {
                var result = _httpDataRestful.PostDataAsync(_badUri).Result;
                Assert.IsNotNull(result);
                Assert.IsFalse(result.IsSuccessStatusCode);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }
            }
        }
    }
}
