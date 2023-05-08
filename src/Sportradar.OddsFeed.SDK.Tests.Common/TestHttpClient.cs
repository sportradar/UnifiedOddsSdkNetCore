using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    internal class TestHttpClient : ISdkHttpClient
    {
        /// <inheritdoc />
        public HttpRequestHeaders DefaultRequestHeaders { get; }

        public TestDataFetcher DataFetcher { get; }

        public TestHttpClient()
        {
            DataFetcher = new TestDataFetcher();
            DefaultRequestHeaders = new HttpClient().DefaultRequestHeaders;
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
}
