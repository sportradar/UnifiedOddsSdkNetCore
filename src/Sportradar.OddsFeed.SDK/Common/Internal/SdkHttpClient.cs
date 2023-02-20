/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    internal class SdkHttpClient : ISdkHttpClient
    {
        private readonly HttpClient _httpClient;

        /// <inheritdoc />
        public HttpRequestHeaders DefaultRequestHeaders => _httpClient.DefaultRequestHeaders;

        public SdkHttpClient(string accessToken, HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            httpClient.DefaultRequestHeaders.Add("x-access-token", accessToken); //
            httpClient.DefaultRequestHeaders.Add("User-Agent", $"UfSdk-{SdkInfo.SdkType}/{SdkInfo.GetVersion()} (NET: {Environment.Version}, OS: {Environment.OSVersion}, Init: {SdkInfo.Created:yyyyMMddHHmm})");
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
