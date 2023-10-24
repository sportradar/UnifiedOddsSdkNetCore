/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Net.Http;

namespace Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess
{
    internal class SdkHttpClientRecovery : SdkHttpClient, ISdkHttpClientRecovery
    {
        public SdkHttpClientRecovery(HttpClient httpClient)
        : base(httpClient)
        {
        }
    }
}
