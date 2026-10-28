// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Exceptions;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Authentication
{
    /// <summary>
    /// Ensures each request carries an access token
    /// </summary>
    internal sealed class UsageDelegatingHandler : DelegatingHandler
    {
        private readonly IUsageAuthenticationTokenProvider _usageTokenProvider;

        public UsageDelegatingHandler(IUsageAuthenticationTokenProvider usageTokenProvider)
        {
            _usageTokenProvider = usageTokenProvider ?? throw new ArgumentNullException(nameof(usageTokenProvider));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var usageAccessToken = await _usageTokenProvider.GetAsync().ConfigureAwait(false);

            if (string.IsNullOrEmpty(usageAccessToken))
            {
                throw new CommunicationException("Token is not available. Unable to authenticate the request.", request.RequestUri?.ToString(), HttpStatusCode.Unauthorized, null);
            }

            request.Headers.Add(UofSdkBootstrap.HttpClientDefaultRequestHeaderForAccessToken, usageAccessToken);

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
#if NET
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return SendAsync(request, cancellationToken).GetAwaiter().GetResult();
        }
#endif
    }
}
