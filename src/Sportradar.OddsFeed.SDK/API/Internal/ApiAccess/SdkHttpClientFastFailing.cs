// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Net.Http;

namespace Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess
{
    internal class SdkHttpClientFastFailing : SdkHttpClient, ISdkHttpClientFastFailing
    {
        public SdkHttpClientFastFailing(HttpClient httpClient)
        : base(httpClient)
        {
        }
    }
}
