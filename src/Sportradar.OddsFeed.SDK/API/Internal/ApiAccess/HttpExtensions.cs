// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Linq;
using System.Net.Http;

namespace Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess
{
    internal static class HttpExtensions
    {
        public static string GetTraceId(this HttpRequestMessage requestMessage)
        {
            var traceId = string.Empty;
            var requestHeaders = requestMessage?.Headers;
            if (requestHeaders != null && requestHeaders.TryGetValues(HttpApiConstants.TraceIdHeaderName, out var headers))
            {
                traceId = headers.FirstOrDefault();
            }

            return traceId;
        }
    }
}
