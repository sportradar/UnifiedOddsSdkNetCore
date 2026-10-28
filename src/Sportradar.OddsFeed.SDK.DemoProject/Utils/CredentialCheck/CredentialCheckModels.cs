// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils.CredentialCheck;

internal enum CredentialAuthEnvironment
{
    Integration,
    Production
}

internal sealed class CommonIamTokenResult
{
    public CredentialAuthEnvironment AuthEnvironment { get; init; }

    public bool Success { get; init; }

    public int? HttpStatusCode { get; init; }

    public string ErrorDetail { get; init; }

    public string JwtToken { get; init; }
}

internal sealed class WhoAmIResult
{
    public CredentialAuthEnvironment ApiEnvironment { get; init; }

    public string ApiHost { get; init; }

    public bool Success { get; init; }

    public int? HttpStatusCode { get; init; }

    public int? BookmakerId { get; init; }

    public string VirtualHost { get; init; }

    public string ResponseCode { get; init; }

    public string ErrorDetail { get; init; }
}

internal sealed class CredentialCheckReport
{
    public string CredentialType { get; init; }

    public CommonIamTokenResult IntegrationCommonIamTokenResult { get; init; }

    public CommonIamTokenResult ProductionCommonIamTokenResult { get; init; }

    public WhoAmIResult IntegrationWhoAmI { get; init; }

    public WhoAmIResult ProductionWhoAmI { get; init; }

    public bool WhoAmIChecksSkipped =>
        (IntegrationCommonIamTokenResult != null || ProductionCommonIamTokenResult != null)
        && IntegrationCommonIamTokenResult?.Success != true
        && ProductionCommonIamTokenResult?.Success != true;
}

internal static class CredentialCheckEndpoints
{
    public const string IntegrationApiHost = "stgapi.betradar.com";
    public const string ProductionApiHost = "api.betradar.com";
    public const string IntegrationAuthHost = "stg-auth.sportradar.com";
    public const string ProductionAuthHost = "auth.sportradar.com";
    public const string ApiTokenAudience = "https://api.betradar.com";
    public const string WhoAmIPath = "/v1/users/whoami.xml";

    public static string GetApiBaseUrl(CredentialAuthEnvironment environment)
    {
        return environment == CredentialAuthEnvironment.Integration
                   ? $"https://{IntegrationApiHost}"
                   : $"https://{ProductionApiHost}";
    }

    public static string GetAuthBaseUrl(CredentialAuthEnvironment environment)
    {
        return environment == CredentialAuthEnvironment.Integration
                   ? $"https://{IntegrationAuthHost}"
                   : $"https://{ProductionAuthHost}";
    }

    public static string GetEnvironmentLabel(CredentialAuthEnvironment environment)
    {
        return environment == CredentialAuthEnvironment.Integration ? "Integration" : "Production";
    }
}
