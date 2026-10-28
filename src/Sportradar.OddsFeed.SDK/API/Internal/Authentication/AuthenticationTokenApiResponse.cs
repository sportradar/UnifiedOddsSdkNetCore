// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Text.Json.Serialization;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Authentication
{
    internal sealed class AuthenticationTokenApiResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        public string ToSanitizedString()
        {
            return $"AccessToken={SdkInfo.ClearSensitiveData(AccessToken)}, ExpiresIn={ExpiresIn}, TokenType={TokenType}";
        }
    }
}
