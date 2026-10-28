// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.Config;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Authentication
{
    internal class UsageAuthenticationTokenProvider : IUsageAuthenticationTokenProvider
    {
        private readonly string _accessToken;
        private readonly IAuthenticationTokenCache _tokenCache;
        private readonly bool _isClientAuthConfigured;

        public UsageAuthenticationTokenProvider(IUofConfiguration uofConfiguration, IAuthenticationTokenCache tokenCache)
        {
            if (uofConfiguration == null)
            {
                throw new ArgumentNullException(nameof(uofConfiguration));
            }

            _tokenCache = tokenCache ?? throw new ArgumentNullException(nameof(tokenCache));
            _accessToken = uofConfiguration.AccessToken;
            _isClientAuthConfigured = uofConfiguration.Authentication != null;
        }

        public async Task<string> GetAsync()
        {
            var usageAccessToken = _isClientAuthConfigured
                                       ? await _tokenCache.GetTokenForApi().ConfigureAwait(false)
                                       : _accessToken;

            return string.IsNullOrEmpty(usageAccessToken)
                       ? string.Empty
                       : usageAccessToken;
        }
    }
}
