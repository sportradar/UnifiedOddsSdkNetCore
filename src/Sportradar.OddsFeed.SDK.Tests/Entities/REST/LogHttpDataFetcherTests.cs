/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class LogHttpDataFetcherTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly TestHttpClient _httpClient;
        private LogHttpDataFetcher _logHttpDataFetcher;
        private readonly Uri _badUri = new Uri("http://www.unexisting-url.com");
        private readonly Uri _getUri = new Uri("http://test.domain.com/get");
        private readonly Uri _postUri = new Uri("http://test.domain.com/post");
        public LogHttpDataFetcherTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _httpClient = new TestHttpClient();
            _logHttpDataFetcher = new LogHttpDataFetcher(_httpClient, new IncrementalSequenceGenerator(), new Deserializer<response>());
        }

        //TODO: requires network
        //[Fact]
        public void GetDataAsyncTest()
        {
            // in logRest file there should be result for this call
            var result = _logHttpDataFetcher.GetDataAsync(_getUri).Result;
            Assert.NotNull(result);
            Assert.True(result.CanRead);
            var s = new StreamReader(result).ReadToEnd();
            Assert.True(!string.IsNullOrEmpty(s));
        }

        [Fact]
        public void GetDataAsyncTestWithWrongUrl()
        {
            // in logRest file there should be result for this call
            _httpClient.DataFetcher.UriReplacements.Add(new Tuple<string, string>(_badUri.ToString(), "-1"));
            Stream result = null;
            var e = Assert.Throws<AggregateException>(() => result = _logHttpDataFetcher.GetDataAsync(_badUri).Result);
            Assert.Null(result);
            Assert.IsType<AggregateException>(e);
            if (e.InnerException != null)
            {
                Assert.IsType<CommunicationException>(e.InnerException);
            }
        }

        //[Fact]
        public void PostDataAsyncTest()
        {
            var result = _logHttpDataFetcher.PostDataAsync(_postUri).Result;
            Assert.NotNull(result);
            Assert.True(result.IsSuccessStatusCode);
        }

        //[Fact]
        public void PostDataAsyncTestWithWrongUrl()
        {
            //TODO: should this be successful?
            _httpClient.DataFetcher.UriReplacements.Add(new Tuple<string, string>(_postUri.ToString(), "-1"));
            var result = new HttpResponseMessage();
            var ex = Assert.Throws<AggregateException>(() => result = _logHttpDataFetcher.PostDataAsync(_badUri).Result);
            Assert.NotNull(result);
            Assert.True(result.IsSuccessStatusCode);
            Assert.IsType<AggregateException>(ex);
            if (ex.InnerException != null)
            {
                Assert.IsType<CommunicationException>(ex.InnerException);
            }
        }

        //[Fact]
        public void PostDataAsyncTestContent()
        {
            var result = _logHttpDataFetcher.PostDataAsync(_postUri, new StringContent("test string")).Result;
            Assert.NotNull(result);
            Assert.True(result.IsSuccessStatusCode);
        }

        //[Fact]
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
                    Assert.NotNull(result);
                    Assert.True(result.IsSuccessStatusCode);
                }
                catch (Exception e)
                {
                    allErrCount++;
                    if (e.InnerException?.Message == "Failed to execute request due to previous failures.")
                    {
                        errCount++;
                    }
                    _outputHelper.WriteLine(e.ToString());
                }
                Assert.Equal(i, allErrCount - 1);
            }
            Assert.Equal(loopCount - 5, errCount);
        }

        //[Fact]
        public void ConsecutivePostAndGetFailureTest()
        {
            const int loopCount = 10;
            var errCount = 0;
            var allPostErrCount = 0;
            var allGetErrCount = 0;
            _httpClient.DataFetcher.UriReplacements.Add(new Tuple<string, string>(_badUri.ToString(), "-1"));
            _httpClient.DataFetcher.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>(_badUri.ToString(), -1, null));

            for (var i = 0; i < loopCount; i++)
            {
                try
                {
                    var result = _logHttpDataFetcher.PostDataAsync(_badUri).Result;
                    Assert.NotNull(result);
                    Assert.False(result.IsSuccessStatusCode);
                }
                catch (Exception e)
                {
                    allPostErrCount++;
                    if (e.InnerException?.Message == "Failed to execute request due to previous failures.")
                    {
                        errCount++;
                    }
                    _outputHelper.WriteLine(e.ToString());
                }
                Assert.Equal(i, allPostErrCount - 1);

                try
                {
                    var result = _logHttpDataFetcher.GetDataAsync(_badUri).Result;
                    Assert.NotNull(result);
                }
                catch (Exception e)
                {
                    allGetErrCount++;
                    if (e.InnerException?.Message == "Failed to execute request due to previous failures.")
                    {
                        errCount++;
                    }
                    _outputHelper.WriteLine(e.ToString());
                }
                Assert.Equal(i, allGetErrCount - 1);
            }
            Assert.Equal((loopCount * 2) - 5, errCount);
        }

        //[Fact]
        public void ExceptionAfterConsecutivePostFailuresTest()
        {
            ConsecutivePostFailureTest();
            try
            {
                var result = new HttpResponseMessage();
                Assert.Throws<CommunicationException>(() => result = _logHttpDataFetcher.PostDataAsync(_getUri).Result);
                Assert.NotNull(result);
                Assert.False(result.IsSuccessStatusCode);
            }
            catch (Exception e)
            {
                if (e.InnerException is CommunicationException)
                {
                    throw e.InnerException;
                }
            }
        }

        //[Fact]
        public void SuccessAfterConsecutiveFailuresResetsTest()
        {
            var httpClient = new TestHttpClient();
            _logHttpDataFetcher = new LogHttpDataFetcher(httpClient, new IncrementalSequenceGenerator(), new Deserializer<response>(), 5, 1);
            ConsecutivePostFailureTest();
            Thread.Sleep(1000);
            var result = _logHttpDataFetcher.GetDataAsync(_getUri).Result;
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }
    }
}
