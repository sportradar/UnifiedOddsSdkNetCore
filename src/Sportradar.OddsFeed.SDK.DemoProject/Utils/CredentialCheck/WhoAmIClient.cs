// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils.CredentialCheck;

internal static class WhoAmIClient
{
    internal enum AuthMode
    {
        /// <summary>SSO access token via x-access-token header.</summary>
        AccessToken,

        /// <summary>CommonIAM JWT via Authorization: Bearer header.</summary>
        Bearer
    }

    public static async Task<WhoAmIResult> CheckAsync(
        string accessToken,
        CredentialAuthEnvironment apiEnvironment,
        AuthMode authMode = AuthMode.AccessToken,
        CancellationToken cancellationToken = default)
    {
        var apiHost = apiEnvironment == CredentialAuthEnvironment.Integration
                          ? CredentialCheckEndpoints.IntegrationApiHost
                          : CredentialCheckEndpoints.ProductionApiHost;
        var apiBaseUrl = CredentialCheckEndpoints.GetApiBaseUrl(apiEnvironment);
        var whoAmIUrl = $"{apiBaseUrl}{CredentialCheckEndpoints.WhoAmIPath}";

        using var httpClient = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, whoAmIUrl);
        if (authMode == AuthMode.Bearer)
        {
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
        }
        else
        {
            request.Headers.TryAddWithoutValidation("x-access-token", accessToken);
        }

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var statusCode = (int)response.StatusCode;
            var parsed = TryParseWhoAmI(responseBody);

            if (response.IsSuccessStatusCode && parsed?.BookmakerId != null)
            {
                return new WhoAmIResult
                {
                    ApiEnvironment = apiEnvironment,
                    ApiHost = apiHost,
                    Success = true,
                    HttpStatusCode = statusCode,
                    BookmakerId = parsed.BookmakerId,
                    VirtualHost = parsed.VirtualHost,
                    ResponseCode = parsed.ResponseCode
                };
            }

            return new WhoAmIResult
            {
                ApiEnvironment = apiEnvironment,
                ApiHost = apiHost,
                Success = false,
                HttpStatusCode = statusCode,
                BookmakerId = parsed?.BookmakerId,
                VirtualHost = parsed?.VirtualHost,
                ResponseCode = parsed?.ResponseCode,
                ErrorDetail = BuildFailureDetail(statusCode, parsed, responseBody)
            };
        }
        catch (Exception ex)
        {
            return new WhoAmIResult
            {
                ApiEnvironment = apiEnvironment,
                ApiHost = apiHost,
                Success = false,
                ErrorDetail = $"Request to {whoAmIUrl} failed: {ex.Message}"
            };
        }
    }

    private static string BuildFailureDetail(int statusCode, ParsedWhoAmI parsed, string responseBody)
    {
        if (!string.IsNullOrWhiteSpace(parsed?.ResponseCode) && !string.Equals(parsed.ResponseCode, "OK", StringComparison.OrdinalIgnoreCase))
        {
            return $"HTTP {statusCode}, response_code={parsed.ResponseCode}";
        }

        if (!string.IsNullOrWhiteSpace(parsed?.Message))
        {
            return $"HTTP {statusCode}: {parsed.Message}";
        }

        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            var trimmed = responseBody.Trim();
            return trimmed.Length > 300 ? $"HTTP {statusCode}: {trimmed[..300]}..." : $"HTTP {statusCode}: {trimmed}";
        }

        return $"HTTP {statusCode} with empty response body.";
    }

    private static ParsedWhoAmI TryParseWhoAmI(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            var document = XDocument.Parse(responseBody);
            var root = document.Root;
            if (root == null)
            {
                return null;
            }

            return new ParsedWhoAmI
            {
                BookmakerId = TryParseInt(root.Attribute("bookmaker_id")?.Value),
                VirtualHost = root.Attribute("virtual_host")?.Value,
                ResponseCode = root.Attribute("response_code")?.Value,
                Message = root.Value?.Trim()
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static int? TryParseInt(string value)
    {
        return int.TryParse(value, out var parsed) ? parsed : null;
    }

    private sealed class ParsedWhoAmI
    {
        public int? BookmakerId { get; init; }

        public string VirtualHost { get; init; }

        public string ResponseCode { get; init; }

        public string Message { get; init; }
    }
}
