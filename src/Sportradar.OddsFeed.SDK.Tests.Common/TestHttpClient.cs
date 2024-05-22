// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

internal class TestHttpClient : ISdkHttpClient
{
    public TimeSpan Timeout { get; }

    /// <inheritdoc />
    public HttpRequestHeaders DefaultRequestHeaders { get; }

    public TestDataFetcher DataFetcher { get; }

    public TestHttpClient()
    {
        DataFetcher = new TestDataFetcher();
        var httpClient = new HttpClient();
        DefaultRequestHeaders = httpClient.DefaultRequestHeaders;
        Timeout = TimeSpan.FromSeconds(httpClient.Timeout.TotalSeconds);
    }

    public Task<HttpResponseMessage> GetAsync(Uri requestUri)
    {
        return Task.FromResult(DataFetcher.GetAsync(requestUri).GetAwaiter().GetResult());
    }

    public async Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content)
    {
        return await DataFetcher.PostDataAsync(requestUri, content).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResponseMessage> DeleteAsync(Uri requestUri)
    {
        return await DataFetcher.DeleteDataAsync(requestUri).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResponseMessage> PutAsync(Uri requestUri, HttpContent content)
    {
        return await DataFetcher.PutDataAsync(requestUri, content).ConfigureAwait(false);
    }
}
