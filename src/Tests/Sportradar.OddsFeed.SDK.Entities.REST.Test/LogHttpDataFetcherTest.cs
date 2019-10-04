/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class LogHttpDataFetcherTest
    {
        private LogHttpDataFetcher _logHttpDataFetcher; // = new LogHttpDataFetcher();
        private readonly Uri _badUri = new Uri("http://www.unexisting-url.com");
        private readonly Uri _getUri = new Uri("http://httpbin.org/get");
        private readonly Uri _postUri = new Uri("http://httpbin.org/post");

        [TestInitialize]
        public void Init()
        {
            var httpClient = new HttpClient();
            _logHttpDataFetcher = new LogHttpDataFetcher(httpClient, TestData.AccessToken, new IncrementalSequenceGenerator(), new Deserializer<response>());
        }

        [TestMethod]
        public void GetDataAsyncTest()
        {
            // in logRest file there should be result for this call
            var result = _logHttpDataFetcher.GetDataAsync(_getUri).Result;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.CanRead);
            var s = new StreamReader(result).ReadToEnd();
            Assert.IsTrue(!string.IsNullOrEmpty(s));
        }

        [TestMethod]
        [ExpectedException(typeof(CommunicationException))]
        public void GetDataAsyncTestWithWrongUrl()
        {
            // in logRest file there should be result for this call
            try
            {
                var stream = _logHttpDataFetcher.GetDataAsync(_badUri).Result;
                Assert.IsNotNull(stream);
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
        public void PostDataAsyncTest()
        {
            var result = _logHttpDataFetcher.PostDataAsync(_postUri).Result;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessStatusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(CommunicationException))]
        public void PostDataAsyncTestWithWrongUrl()
        {
            try
            {
                var result = _logHttpDataFetcher.PostDataAsync(_badUri).Result;
                Assert.IsNotNull(result);
                Assert.IsTrue(result.IsSuccessStatusCode);
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
        public void PostDataAsyncTestContent()
        {
            var result = _logHttpDataFetcher.PostDataAsync(_postUri, new StringContent("test string")).Result;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessStatusCode);
        }

        [TestMethod]
        public void LoggingTestContent()
        {
            var result = _logHttpDataFetcher.PostDataAsync(_postUri, new StringContent("test string")).Result;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessStatusCode);
        }

        [TestMethod]
        public void ConsecutivePostFailureTest()
        {
            const int loopCount = 10;
            var errCount = 0;
            var allErrCount = 0;
            for (var i = 0; i < loopCount; i++)
            {
                try
                {
                    var result = _logHttpDataFetcher.PostDataAsync(_badUri).Result;
                    Assert.IsNotNull(result);
                    Assert.IsTrue(result.IsSuccessStatusCode);
                }
                catch (Exception e)
                {
                    allErrCount++;
                    if (e.InnerException?.Message == "Failed to execute request due to previous failures.")
                    {
                        errCount++;
                    }
                    Console.WriteLine(e);
                }
                Assert.AreEqual(i, allErrCount-1);
            }
            Assert.AreEqual(loopCount - 5, errCount);
        }

        [TestMethod]
        public void ConsecutivePostAndGetFailureTest()
        {
            const int loopCount = 10;
            var errCount = 0;
            var allPostErrCount = 0;
            var allGetErrCount = 0;
            for (var i = 0; i < loopCount; i++)
            {
                try
                {
                    var result = _logHttpDataFetcher.PostDataAsync(_badUri).Result;
                    Assert.IsNotNull(result);
                    Assert.IsTrue(result.IsSuccessStatusCode);
                }
                catch (Exception e)
                {
                    allPostErrCount++;
                    if (e.InnerException?.Message == "Failed to execute request due to previous failures.")
                    {
                        errCount++;
                    }
                    Console.WriteLine(e);
                }
                Assert.AreEqual(i, allPostErrCount - 1);

                try
                {
                    var result = _logHttpDataFetcher.GetDataAsync(_badUri).Result;
                    Assert.IsNotNull(result);
                }
                catch (Exception e)
                {
                    allGetErrCount++;
                    if (e.InnerException?.Message == "Failed to execute request due to previous failures.")
                    {
                        errCount++;
                    }
                    Console.WriteLine(e);
                }
                Assert.AreEqual(i, allGetErrCount - 1);
            }
            Assert.AreEqual(loopCount*2-5, errCount);
        }

        [TestMethod]
        [ExpectedException(typeof(CommunicationException))]
        public void ExceptionAfterConsecutivePostFailuresTest()
        {
            ConsecutivePostFailureTest();
            try
            {
                var result = _logHttpDataFetcher.PostDataAsync(_getUri).Result;
                Assert.IsNotNull(result);
                Assert.IsFalse(result.IsSuccessStatusCode);
            }
            catch (Exception e)
            {
                if (e.InnerException is CommunicationException)
                {
                    throw e.InnerException;
                }
            }
        }

        [TestMethod]
        public void SuccessAfterConsecutiveFailuresResetsTest()
        {
            var httpClient = new HttpClient();
            _logHttpDataFetcher = new LogHttpDataFetcher(httpClient, TestData.AccessToken, new IncrementalSequenceGenerator(), new Deserializer<response>(), 5, 1);
            ConsecutivePostFailureTest();
            Thread.Sleep(1000);
            var result = _logHttpDataFetcher.GetDataAsync(_getUri).Result;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }
    }
}
