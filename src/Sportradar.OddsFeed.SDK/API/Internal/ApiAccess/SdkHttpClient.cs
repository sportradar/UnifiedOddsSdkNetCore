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
        private readonly HttpClient _httpClient;

        public TimeSpan Timeout => _httpClient.Timeout;

        /// <inheritdoc />
        public HttpRequestHeaders DefaultRequestHeaders => _httpClient.DefaultRequestHeaders;

        public SdkHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            if (_httpClient.DefaultRequestHeaders.IsNullOrEmpty())
            {
                throw new InvalidOperationException("Missing DefaultRequestHeaders");
            }
            if (!_httpClient.DefaultRequestHeaders.Contains("x-access-token"))
            {
                throw new InvalidOperationException("Missing x-access-token");
            }
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                throw new InvalidOperationException("User-Agent");
            }
            //_httpClient.DefaultRequestHeaders.Add("x-access-token", accessToken); //
            //_httpClient.DefaultRequestHeaders.Add("User-Agent", $"UfSdk-{SdkInfo.SdkType}/{SdkInfo.GetVersion()} (OS: {Environment.OSVersion}, NET: {Environment.Version}, Init: {SdkInfo.Created:yyyyMMddHHmm})");
            //Timeout = TimeSpan.FromSeconds(_httpClient.Timeout.TotalSeconds);
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> GetAsync(Uri requestUri)
        {
            return await _httpClient.GetAsync(requestUri).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content)
        {
            return await _httpClient.PostAsync(requestUri, content).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> DeleteAsync(Uri requestUri)
        {
            return await _httpClient.DeleteAsync(requestUri).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> PutAsync(Uri requestUri, HttpContent content)
        {
            return await _httpClient.PutAsync(requestUri, content).ConfigureAwait(false);
        }
    }
}
