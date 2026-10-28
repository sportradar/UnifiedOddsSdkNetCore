// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Authentication
{
    internal sealed class AuthenticationToken
    {
        public string AccessToken { get; set; }

        public int ExpiresIn { get; set; }

        public string TokenType { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public DateTime? RefetchStartAt { get; set; }

        public DateTime? LastFetched { get; set; }

        public DateTime? NextFetchNotBefore { get; set; }

        public AuthenticationToken(AuthenticationTokenApiResponse apiToken)
        {
            AccessToken = apiToken.AccessToken;
            ExpiresIn = apiToken.ExpiresIn;
            TokenType = apiToken.TokenType;

            ExpiresAt = DateTime.Now.AddSeconds(ExpiresIn);
            RefetchStartAt = DateTime.Now.AddSeconds(ExpiresIn * 0.9);
            LastFetched = null;
            NextFetchNotBefore = RefetchStartAt;
        }
    }
}
