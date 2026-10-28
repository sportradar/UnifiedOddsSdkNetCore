// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Extensions;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Authentication
{
    /// <summary>
    /// Ensures each request carries an access token from IAuthenticationCache
    /// </summary>
    internal sealed class AuthenticationDelegatingHandler : DelegatingHandler
    {
        private readonly string _accessToken;
        private readonly IAuthenticationTokenCache _tokenCache;
        private readonly bool _isClientAuthConfigured;

        public AuthenticationDelegatingHandler(IUofConfiguration configuration, IAuthenticationTokenCache tokenCache)
        {
            _tokenCache = tokenCache ?? throw new ArgumentNullException(nameof(tokenCache));
            _accessToken = configuration.AccessToken;
            _isClientAuthConfigured = configuration.Authentication != null;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_isClientAuthConfigured)
            {
                var jwtApiToken = await _tokenCache.GetTokenForApi().ConfigureAwait(false);
                if (string.IsNullOrEmpty(jwtApiToken))
                {
                    throw new CommunicationException("Authentication token is not available. Unable to authenticate the request.", request.RequestUri?.ToString(), HttpStatusCode.Unauthorized, null);
                }
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtApiToken);
            }
            else
            {
                if (!_accessToken.IsNullOrEmpty())
                {
                    request.Headers.Add(UofSdkBootstrap.HttpClientDefaultRequestHeaderForAccessToken, _accessToken);
                }
                else
                {
                    throw new CommunicationException("Access token is not available. Unable to authenticate the request.", request.RequestUri?.ToString(), HttpStatusCode.Unauthorized, null);
                }
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
