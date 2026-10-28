// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Sportradar.OddsFeed.SDK.Api.Config;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils.CredentialCheck;

internal static class CommonIamTokenClient
{
    private const int ClientAssertionExpirySeconds = 60;

    public static async Task<CommonIamTokenResult> RequestTokenAsync(
        UofClientAuthentication.IPrivateKeyJwtData credentials,
        CredentialAuthEnvironment authEnvironment,
        CancellationToken cancellationToken = default)
    {
        var authBaseUrl = CredentialCheckEndpoints.GetAuthBaseUrl(authEnvironment);
        var clientAssertion = CreateClientAssertion(credentials, authBaseUrl);

        using var httpClient = new HttpClient { BaseAddress = new Uri(authBaseUrl) };
        using var request = new HttpRequestMessage(HttpMethod.Post, "oauth/token")
        {
            Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"),
                new KeyValuePair<string, string>("client_assertion", clientAssertion),
                new KeyValuePair<string, string>("audience", CredentialCheckEndpoints.ApiTokenAudience)
            })
        };

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var statusCode = (int)response.StatusCode;

            if (!response.IsSuccessStatusCode)
            {
                return new CommonIamTokenResult
                {
                    AuthEnvironment = authEnvironment,
                    Success = false,
                    HttpStatusCode = statusCode,
                    ErrorDetail = FormatOAuthError(responseBody, statusCode)
                };
            }

            var tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(responseBody, JsonSerializerOptions);
            if (string.IsNullOrWhiteSpace(tokenResponse?.AccessToken))
            {
                return new CommonIamTokenResult
                {
                    AuthEnvironment = authEnvironment,
                    Success = false,
                    HttpStatusCode = statusCode,
                    ErrorDetail = $"Token endpoint returned HTTP {statusCode} but no access_token was present."
                };
            }

            return new CommonIamTokenResult
            {
                AuthEnvironment = authEnvironment,
                Success = true,
                HttpStatusCode = statusCode,
                JwtToken = tokenResponse.AccessToken
            };
        }
        catch (Exception ex)
        {
            return new CommonIamTokenResult
            {
                AuthEnvironment = authEnvironment,
                Success = false,
                ErrorDetail = $"Request to {authBaseUrl}/oauth/token failed: {ex.Message}"
            };
        }
    }

    private static string CreateClientAssertion(UofClientAuthentication.IPrivateKeyJwtData credentials, string authBaseUrl)
    {
        var signingKey = credentials.PrivateKey;
        if (signingKey is RsaSecurityKey rsaKey && string.IsNullOrWhiteSpace(rsaKey.KeyId))
        {
            rsaKey.KeyId = credentials.SigningKeyId;
        }

        var issuedAt = DateTimeOffset.UtcNow;
        var expiresAt = issuedAt.AddSeconds(ClientAssertionExpirySeconds);
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, credentials.ClientId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, issuedAt.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var jwt = new JwtSecurityToken(
            credentials.ClientId,
            $"{authBaseUrl.TrimEnd('/')}/",
            claims,
            issuedAt.UtcDateTime,
            expiresAt.UtcDateTime,
            signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    private static string FormatOAuthError(string responseBody, int statusCode)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return $"HTTP {statusCode} with empty response body.";
        }

        try
        {
            var errorResponse = JsonSerializer.Deserialize<OAuthErrorResponse>(responseBody, JsonSerializerOptions);
            if (!string.IsNullOrWhiteSpace(errorResponse?.Error))
            {
                return string.IsNullOrWhiteSpace(errorResponse.ErrorDescription)
                           ? $"HTTP {statusCode}: {errorResponse.Error}"
                           : $"HTTP {statusCode}: {errorResponse.Error} - {errorResponse.ErrorDescription}";
            }
        }
        catch (JsonException)
        {
            // fall through to raw body
        }

        return $"HTTP {statusCode}: {responseBody.Trim()}";
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed class OAuthTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
    }

    private sealed class OAuthErrorResponse
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("error_description")]
        public string ErrorDescription { get; set; }
    }
}
