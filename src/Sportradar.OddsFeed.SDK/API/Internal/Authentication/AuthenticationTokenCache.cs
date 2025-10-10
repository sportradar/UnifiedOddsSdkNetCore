// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Authentication
{
    internal sealed class AuthenticationTokenCache : IAuthenticationTokenCache
    {
        public const string FusionCacheInstanceName = "AuthFusionCache";
        private const string UofApiTokenAudience = "UF-RestAPI";
        private const string UofFeedTokenAudience = "UF-RabbitMQ";
        private const float EagerRefreshThreshold = 0.9f; // cache refresh in the background when 90% of duration elapsed

        private readonly IFusionCache _cache;
        private readonly IAuthenticationClient _authenticationClient;
        private readonly IJsonWebTokenFactory _jsonWebTokenFactory;
        private readonly ILogger<AuthenticationTokenCache> _logger;

        public AuthenticationTokenCache(IFusionCacheProvider cacheProvider,
                                        IAuthenticationClient authenticationClient,
                                        IJsonWebTokenFactory jsonWebTokenFactory,
                                        ILogger<AuthenticationTokenCache> logger)
        {
            _cache = cacheProvider.GetCache(FusionCacheInstanceName);
            _authenticationClient = authenticationClient;
            _jsonWebTokenFactory = jsonWebTokenFactory;
            _logger = logger;
        }

        public async Task<string> GetTokenForApi()
        {
            var token = await GetAuthTokenAsync(UofApiTokenAudience).ConfigureAwait(false);

            return token?.AccessToken;
        }

        public async Task<string> GetTokenForFeed()
        {
            var token = await GetAuthTokenAsync(UofFeedTokenAudience).ConfigureAwait(false);

            return token?.AccessToken;
        }

        private async Task<AuthenticationToken> GetAuthTokenAsync(string audience)
        {
            try
            {
                return await _cache.GetOrSetAsync<AuthenticationToken>(audience, async (ctx, tokenCt) => await FetchAndCacheTokenAsync(audience, ctx, tokenCt).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Getting new token from Common IAM for {Audience} failed", audience);
            }

            return null;
        }

        private async Task<AuthenticationToken> FetchAndCacheTokenAsync(string audience, FusionCacheFactoryExecutionContext<AuthenticationToken> context, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Fetching new token from Common IAM for {Audience}", audience);
            var tokenResponse = await GetNewAuthenticationTokenFromCommonIam(audience, cancellationToken).ConfigureAwait(false);
            if (tokenResponse.IsValid())
            {
                _logger.LogInformation("Successfully fetched token from Common IAM for {Audience}", audience);
                var newToken = new AuthenticationToken(tokenResponse);

                context.Options.Duration = TimeSpan.FromSeconds(tokenResponse.ExpiresIn);
                context.Options.EagerRefreshThreshold = EagerRefreshThreshold;
                return newToken;
            }
            _logger.LogWarning("Did not get new token from Common IAM for {Audience}", audience);
            return null;
        }

        private Task<AuthenticationTokenApiResponse> GetNewAuthenticationTokenFromCommonIam(string audience, CancellationToken cancellationToken)
        {
            var clientAssertionJwt = _jsonWebTokenFactory.CreateJsonWebToken();

            return _authenticationClient.RequestAccessTokenAsync(clientAssertionJwt, audience, cancellationToken);
        }
    }
}
