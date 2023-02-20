/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    internal class SdkHttpClientPool : ISdkHttpClient
    {
        private readonly IList<HttpClient> _httpClientPool;
        private int _latest;
        private readonly object _lock = new object();

        /// <inheritdoc />
        public HttpRequestHeaders DefaultRequestHeaders { get; }

        public SdkHttpClientPool(string accessToken, int poolSize, int timeoutSecond)
        : this(accessToken, poolSize, TimeSpan.FromSeconds(timeoutSecond))
        {
        }

        public SdkHttpClientPool(string accessToken, int poolSize, TimeSpan timeout)
        {
            if (poolSize < 1)
            {
                poolSize = 1;
            }
            _httpClientPool = new List<HttpClient>(poolSize);
            for (var i = 0; i < poolSize; i++)
            {
                var httpClient = new HttpClient { Timeout = timeout };
                httpClient.DefaultRequestHeaders.Add("x-access-token", accessToken); //
                httpClient.DefaultRequestHeaders.Add("User-Agent", $"UfSdk-{SdkInfo.SdkType}/{SdkInfo.GetVersion()} (NET: {Environment.Version}, OS: {Environment.OSVersion}, Init: {SdkInfo.Created:yyyyMMddHHmm})");
                _httpClientPool.Add(httpClient);
            }

            SdkLoggerFactory.GetLoggerForExecution(typeof(SdkHttpClientPool)).LogDebug($"SdkHttpClientPool with size {poolSize} and timeout {timeout.TotalSeconds}s created.");
            DefaultRequestHeaders = _httpClientPool.First().DefaultRequestHeaders;
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> GetAsync(Uri requestUri)
        {
            var httpClient = GetHttpClient();
            var result = await httpClient.GetAsync(requestUri).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content)
        {
            var httpClient = GetHttpClient();
            return await httpClient.PostAsync(requestUri, content).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> DeleteAsync(Uri requestUri)
        {
            var httpClient = GetHttpClient();
            return await httpClient.DeleteAsync(requestUri).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> PutAsync(Uri requestUri, HttpContent content)
        {
            var httpClient = GetHttpClient();
            return await httpClient.PutAsync(requestUri, content).ConfigureAwait(false);
        }

        private HttpClient GetHttpClient()
        {
            if (_httpClientPool.Count == 1)
            {
                return _httpClientPool[0];
            }

            lock (_lock)
            {
                _latest = _latest == _httpClientPool.Count - 1 ? 0 : _latest + 1;
                return _httpClientPool[_latest];
            }
        }
    }
}
