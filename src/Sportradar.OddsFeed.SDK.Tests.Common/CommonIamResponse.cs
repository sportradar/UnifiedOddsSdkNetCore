// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Text.Json.Serialization;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

internal sealed class CommonIamResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "bearer";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
