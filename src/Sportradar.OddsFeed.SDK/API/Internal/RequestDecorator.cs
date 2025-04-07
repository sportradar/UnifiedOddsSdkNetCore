// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Net.Http;

namespace Sportradar.OddsFeed.SDK.Api.Internal
{
    /// <summary>
    /// Request decorator used to decorate the <see cref="HttpRequestMessage"/> before it is sent
    /// </summary>
    public class RequestDecorator : IRequestDecorator
    {
        /// <summary>
        /// Decorates the provided <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="request">Http request</param>
        public void Decorate(HttpRequestMessage request)
        {
            var traceId = Guid.NewGuid().ToString();
            request.Headers.Add(HttpApiConstants.TraceIdHeaderName, traceId);
        }
    }
}
