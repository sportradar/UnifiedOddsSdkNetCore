// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Extensions;

namespace Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess
{
    internal class SdkHttpClient : ISdkHttpClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _httpClientName;
        public TimeSpan Timeout { get; }

        /// <inheritdoc />
        public HttpRequestHeaders DefaultRequestHeaders { get; }

        public SdkHttpClient(IHttpClientFactory httpClientFactory, string httpClientName)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpClientName = string.IsNullOrEmpty(httpClientName) ? throw new ArgumentNullException(nameof(httpClientName)) : httpClientName;

            var httpClient = _httpClientFactory.CreateClient(_httpClientName);
            if (httpClient.DefaultRequestHeaders.IsNullOrEmpty())
            {
                throw new InvalidOperationException("Missing DefaultRequestHeaders");
            }
            if (!httpClient.DefaultRequestHeaders.Contains("x-access-token"))
            {
                throw new InvalidOperationException("Missing x-access-token");
            }
            if (!httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                throw new InvalidOperationException("User-Agent");
            }
            Timeout = httpClient.Timeout;
            DefaultRequestHeaders = httpClient.DefaultRequestHeaders;
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> GetAsync(Uri requestUri)
        {
            var httpClient = _httpClientFactory.CreateClient(_httpClientName);
            return await httpClient.GetAsync(requestUri).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request)
        {
            var httpClient = _httpClientFactory.CreateClient(_httpClientName);
            return await httpClient.SendAsync(request).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content)
        {
            var httpClient = _httpClientFactory.CreateClient(_httpClientName);
            return await httpClient.PostAsync(requestUri, content).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> DeleteAsync(Uri requestUri)
        {
            var httpClient = _httpClientFactory.CreateClient(_httpClientName);
            return await httpClient.DeleteAsync(requestUri).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> PutAsync(Uri requestUri, HttpContent content)
        {
            var httpClient = _httpClientFactory.CreateClient(_httpClientName);
            return await httpClient.PutAsync(requestUri, content).ConfigureAwait(false);
        }
    }
}
