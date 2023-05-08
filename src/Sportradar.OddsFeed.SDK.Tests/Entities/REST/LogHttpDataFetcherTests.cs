/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
        private LogHttpDataFetcher _logHttpDataFetcherPool;
        private readonly Uri _badUri = new Uri("http://www.unexisting-url.com");
        private readonly Uri _getUri = new Uri("http://test.domain.com/get");
        private readonly Uri _postUri = new Uri("http://test.domain.com/post");

        public LogHttpDataFetcherTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            var httpMessageHandler = new TestMessageHandler(_outputHelper, 200, 0);
            var httpClient = new HttpClient(httpMessageHandler);
            var sdkHttpClient = new SdkHttpClient("aaa", httpClient);
            var sdkHttpClientPool = new SdkHttpClientPool("aaa", 20, TimeSpan.FromSeconds(5), httpMessageHandler);

            _logHttpDataFetcher = new LogHttpDataFetcher(sdkHttpClient, new IncrementalSequenceGenerator(), new Deserializer<response>());
            _logHttpDataFetcherPool = new LogHttpDataFetcher(sdkHttpClientPool, new IncrementalSequenceGenerator(), new Deserializer<response>());
        }

        //[Fact]
        public async Task PerformanceOf100SequentialRequests()
        {
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < 100; i++)
            {
                var result = await _logHttpDataFetcher.GetDataAsync(_getUri).ConfigureAwait(false);
                Assert.NotNull(result);
                Assert.True(result.CanRead);
            }
            _outputHelper.WriteLine($"Elapsed {stopwatch.ElapsedMilliseconds} ms");
        }

        [Fact]
        public async Task PerformanceOfManyParallelRequests()
        {
            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task>();
            for (var i = 0; i < 1000; i++)
            {
                var task = _logHttpDataFetcher.GetDataAsync(GetRequestUri(false));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
            Assert.True(tasks.TrueForAll(a => a.IsCompletedSuccessfully));
            _outputHelper.WriteLine($"Elapsed {stopwatch.ElapsedMilliseconds} ms");
        }

        [Fact]
        public async Task PerformancePoolOfManyParallelRequests()
        {
            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task>();
            for (var i = 0; i < 1000; i++)
            {
                var task = _logHttpDataFetcherPool.GetDataAsync(GetRequestUri(false));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
            Assert.True(tasks.TrueForAll(a => a.IsCompletedSuccessfully));
            _outputHelper.WriteLine($"Elapsed {stopwatch.ElapsedMilliseconds} ms");
        }

        [Fact]
        public async Task PerformanceOfManyUniqueParallelRequests()
        {
            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task>();
            for (var i = 0; i < 1000; i++)
            {
                var task = _logHttpDataFetcher.GetDataAsync(GetRequestUri(true));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
            Assert.True(tasks.TrueForAll(a => a.IsCompletedSuccessfully));
            _outputHelper.WriteLine($"Elapsed {stopwatch.ElapsedMilliseconds} ms");
        }

        [Fact]
        public async Task PerformancePoolOfManyUniqueParallelRequests()
        {
            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task>();
            for (var i = 0; i < 1000; i++)
            {
                var task = _logHttpDataFetcherPool.GetDataAsync(GetRequestUri(true));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
            Assert.True(tasks.TrueForAll(a => a.IsCompletedSuccessfully));
            _outputHelper.WriteLine($"Elapsed {stopwatch.ElapsedMilliseconds} ms");
        }

        [Fact]
        public async Task PerformanceOfManyUniqueUriParallelRequests()
        {
            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task>();
            for (var i = 0; i < 1000; i++)
            {
                var task = _logHttpDataFetcher.GetDataAsync(GetRandomUri(true));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
            Assert.True(tasks.TrueForAll(a => a.IsCompletedSuccessfully));
            _outputHelper.WriteLine($"Elapsed {stopwatch.ElapsedMilliseconds} ms");
        }

        [Fact]
        public async Task PerformancePoolOfManyUniqueUriParallelRequests()
        {
            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task>();
            for (var i = 0; i < 1000; i++)
            {
                var task = _logHttpDataFetcherPool.GetDataAsync(GetRandomUri(true));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
            Assert.True(tasks.TrueForAll(a => a.IsCompletedSuccessfully));
            _outputHelper.WriteLine($"Elapsed {stopwatch.ElapsedMilliseconds} ms");
        }

        //[Fact]
        public async Task BingPerformanceOfManyUniqueUriParallelRequests()
        {
            var sdkHttpClient = new SdkHttpClient("aaa", new HttpClient());
            var logHttpDataFetcher = new LogHttpDataFetcher(sdkHttpClient, new IncrementalSequenceGenerator(), new Deserializer<response>());

            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task>();
            for (var i = 0; i < 100; i++)
            {
                var task = logHttpDataFetcher.GetDataAsync(GetBingSearchUri(true));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
            Assert.True(tasks.TrueForAll(a => a.IsCompletedSuccessfully));
            _outputHelper.WriteLine($"Elapsed {stopwatch.ElapsedMilliseconds} ms");
        }

        //[Fact]
        public async Task BingPerformancePoolOfManyUniqueUriParallelRequests()
        {
            var sdkHttpClientPool = new SdkHttpClientPool("aaa", 20, 50, 10);
            var logHttpDataFetcher = new LogHttpDataFetcher(sdkHttpClientPool, new IncrementalSequenceGenerator(), new Deserializer<response>());

            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task>();
            for (var i = 0; i < 100; i++)
            {
                var task = logHttpDataFetcher.GetDataAsync(GetBingSearchUri(true));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
            Assert.True(tasks.TrueForAll(a => a.IsCompletedSuccessfully));
            _outputHelper.WriteLine($"Elapsed {stopwatch.ElapsedMilliseconds} ms");
        }

        //TODO: requires network
        //[Fact]
        public void GetDataAsync()
        {
            // in logRest file there should be result for this call
            var result = _logHttpDataFetcher.GetDataAsync(_getUri).GetAwaiter().GetResult();
            Assert.NotNull(result);
            Assert.True(result.CanRead);
            var s = new StreamReader(result).ReadToEnd();
            Assert.True(!string.IsNullOrEmpty(s));
        }

        //[Fact]
        public void GetDataAsyncTestWithWrongUrl()
        {
            // in logRest file there should be result for this call
            _httpClient.DataFetcher.UriReplacements.Add(new Tuple<string, string>(_badUri.ToString(), "-1"));
            Stream result = null;
            var e = Assert.Throws<CommunicationException>(() => result = _logHttpDataFetcher.GetDataAsync(_badUri).GetAwaiter().GetResult());
            Assert.Null(result);
            Assert.IsType<CommunicationException>(e);
            if (e.InnerException != null)
            {
                Assert.IsType<CommunicationException>(e.InnerException);
            }
        }

        //[Fact]
        public void PostDataAsync()
        {
            var result = _logHttpDataFetcher.PostDataAsync(_postUri).GetAwaiter().GetResult();
            Assert.NotNull(result);
            Assert.True(result.IsSuccessStatusCode);
        }

        //[Fact]
        public void PostDataAsyncTestWithWrongUrl()
        {
            //TODO: should this be successful?
            _httpClient.DataFetcher.UriReplacements.Add(new Tuple<string, string>(_postUri.ToString(), "-1"));
            var result = new HttpResponseMessage();
            var ex = Assert.Throws<AggregateException>(() => result = _logHttpDataFetcher.PostDataAsync(_badUri).GetAwaiter().GetResult());
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
            var result = _logHttpDataFetcher.PostDataAsync(_postUri, new StringContent("test string")).GetAwaiter().GetResult();
            Assert.NotNull(result);
            Assert.True(result.IsSuccessStatusCode);
        }

        //[Fact]
        public void ConsecutivePostFailure()
        {
            const int loopCount = 10;
            var errCount = 0;
            var allErrCount = 0;
            for (var i = 0; i < loopCount; i++)
            {
                try
                {
                    var result = _logHttpDataFetcher.PostDataAsync(_badUri).GetAwaiter().GetResult();
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
        public void ConsecutivePostAndGetFailure()
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
                    var result = _logHttpDataFetcher.PostDataAsync(_badUri).GetAwaiter().GetResult();
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
                    var result = _logHttpDataFetcher.GetDataAsync(_badUri).GetAwaiter().GetResult();
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
        public void ExceptionAfterConsecutivePostFailures()
        {
            ConsecutivePostFailure();
            try
            {
                var result = new HttpResponseMessage();
                Assert.Throws<CommunicationException>(() => result = _logHttpDataFetcher.PostDataAsync(_getUri).GetAwaiter().GetResult());
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
        public void SuccessAfterConsecutiveFailuresResets()
        {
            var httpClient = new TestHttpClient();
            _logHttpDataFetcher = new LogHttpDataFetcher(httpClient, new IncrementalSequenceGenerator(), new Deserializer<response>(), 5, 1);
            ConsecutivePostFailure();
            Task.Delay(1000).GetAwaiter().GetResult();
            var result = _logHttpDataFetcher.GetDataAsync(_getUri).GetAwaiter().GetResult();
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        private Uri GetRequestUri(bool isRandom)
        {
            var id = isRandom ? SdkInfo.GetRandom() : 1;
            return new Uri($"http://test.domain.com/api/v1/sr:match:{id}/summary.xml");
        }

        private Uri GetRandomUri(bool isRandom)
        {
            var id = isRandom ? SdkInfo.GetRandom() : 1;
            return new Uri($"http://test.domain.com/api/v1/sr:match:{id}.xml");
        }

        private Uri GetBingSearchUri(bool isRandom)
        {
            var id = isRandom ? SdkInfo.GetGuid(8) : "1";
            return new Uri($"https://www.bing.com/search?q={id}");
        }

        private class TestMessageHandler : HttpMessageHandler
        {
            private readonly int _timeoutMs;
            private readonly int _timeoutVariablePercent;
            private readonly ITestOutputHelper _outputHelper;

            public TestMessageHandler(ITestOutputHelper outputHelper, int timeoutMs, int timeoutVariablePercent = 0)
            {
                _timeoutMs = timeoutMs;
                _timeoutVariablePercent = timeoutVariablePercent;
                _outputHelper = outputHelper;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var stopWatch = Stopwatch.StartNew();
                var timeout = _timeoutVariablePercent < 1 ? _timeoutMs : SdkInfo.GetVariableNumber(_timeoutMs, _timeoutVariablePercent);

                await Task.Delay(timeout, cancellationToken).ConfigureAwait(false);
                //_outputHelper.WriteLine($"Request to {request.RequestUri} took {stopWatch.ElapsedMilliseconds} ms");
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Accepted,
                    ReasonPhrase = HttpStatusCode.Accepted.ToString(),
                    RequestMessage = request,
                    Content = new StringContent("some text")
                };
            }
        }
    }
}
