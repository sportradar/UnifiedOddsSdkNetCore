// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Authentication
{
    internal static class AuthenticationTokenExtensions
    {
        public static bool IsValid(this AuthenticationTokenApiResponse token)
        {
            return !token.AccessToken.IsNullOrEmpty() && token.ExpiresIn > 0;
        }

        public static bool IsValid(this AuthenticationToken token)
        {
            return !token.AccessToken.IsNullOrEmpty();
        }

        public static bool ShouldBeFetched(this AuthenticationToken token)
        {
            return token.RefetchStartAt < TimeProviderAccessor.Current.Now;
        }

        public static string GetAudienceForLocalToken(this UofClientAuthentication.IPrivateKeyJwt privateKeyJwt)
        {
            if (privateKeyJwt == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(privateKeyJwt.Host))
            {
                throw new ArgumentException("Host is not configured in authentication configuration");
            }

            var httpPrefix = privateKeyJwt.UseSsl ? "https" : "http";

            var portPart = privateKeyJwt.Port > 0 && privateKeyJwt.Port != 80 && privateKeyJwt.Port != 443 ? $":{privateKeyJwt.Port}" : string.Empty;

            return $"{httpPrefix}://{privateKeyJwt.Host}{portPart}/";
        }
    }
}

