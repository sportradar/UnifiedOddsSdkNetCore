// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Dawn;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Authentication;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess
{
    internal class CredentialProviderCommonIam : ICredentialsProvider
    {
        private readonly IAuthenticationTokenCache _authTokenCache;
        private readonly ILogger _logger;
        private string _password;
        private string _customPassword;

        public string Name
        {
            get;
        }

        public string UserName
        {
            get;
        }

        public string Password
        {
            get
            {
                Refresh();
                return _password;
            }
        }

        public TimeSpan? ValidUntil
        {
            get;
            private set;
        }

        public CredentialProviderCommonIam(IUofConfiguration config, IAuthenticationTokenCache authTokenCache, ILogger logger)
        {
            _ = Guard.Argument(config, nameof(config)).NotNull();
            _ = Guard.Argument(authTokenCache, nameof(authTokenCache)).NotNull();
            _ = Guard.Argument(logger, nameof(logger)).NotNull();

            _authTokenCache = authTokenCache;
            _logger = logger;
            _customPassword = config.Rabbit.Password;

            Name = "CommonIAM";
            UserName = config.Rabbit.Username ?? config.BookmakerDetails.BookmakerId.ToString();
        }

        public void Refresh()
        {
            if (_customPassword != null)
            {
                _password = _customPassword;
                _logger.LogInformation("Using custom password instead of calling CommonIam. Username: {Username}, Password: {Password}", UserName, SdkInfo.ClearSensitiveData(_password));
                return;
            }
            var authToken = _authTokenCache.GetTokenForFeed().GetAwaiter().GetResult();
            if (authToken != null)
            {
                _password = authToken.AccessToken;
                ValidUntil = TimeSpan.FromSeconds(authToken.ExpiresIn);
            }
            _logger.LogInformation("CommonIam credentials refreshed. Username: {Username}, Password: {Password}, ValidUntil: {ValidUntil}s", UserName, SdkInfo.ClearSensitiveData(_password), ValidUntil?.TotalSeconds);
        }
    }
}
