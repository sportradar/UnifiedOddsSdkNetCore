// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.Config;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils.CredentialCheck;

internal static class CredentialChecker
{
    public static async Task<CredentialCheckReport> CheckAccessTokenAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new ArgumentException("Access token cannot be empty.", nameof(accessToken));
        }

        var integration = await WhoAmIClient.CheckAsync(accessToken, CredentialAuthEnvironment.Integration, WhoAmIClient.AuthMode.AccessToken, cancellationToken).ConfigureAwait(false);
        var production = await WhoAmIClient.CheckAsync(accessToken, CredentialAuthEnvironment.Production, WhoAmIClient.AuthMode.AccessToken, cancellationToken).ConfigureAwait(false);

        return new CredentialCheckReport
        {
            CredentialType = "SSO access token",
            IntegrationWhoAmI = integration,
            ProductionWhoAmI = production
        };
    }

    public static async Task<CredentialCheckReport> CheckCommonIamAsync(
        UofClientAuthentication.IPrivateKeyJwtData credentials,
        CancellationToken cancellationToken = default)
    {
        var integrationToken = await CommonIamTokenClient.RequestTokenAsync(credentials, CredentialAuthEnvironment.Integration, cancellationToken).ConfigureAwait(false);
        var productionToken = await CommonIamTokenClient.RequestTokenAsync(credentials, CredentialAuthEnvironment.Production, cancellationToken).ConfigureAwait(false);

        WhoAmIResult integrationWhoAmI = null;
        WhoAmIResult productionWhoAmI = null;
        if (integrationToken.Success)
        {
            integrationWhoAmI = await WhoAmIClient.CheckAsync(integrationToken.JwtToken,
                                                             CredentialAuthEnvironment.Integration,
                                                             WhoAmIClient.AuthMode.Bearer,
                                                             cancellationToken).ConfigureAwait(false);
        }

        if (productionToken.Success)
        {
            productionWhoAmI = await WhoAmIClient.CheckAsync(productionToken.JwtToken,
                                                            CredentialAuthEnvironment.Production,
                                                            WhoAmIClient.AuthMode.Bearer,
                                                            cancellationToken).ConfigureAwait(false);
        }

        return new CredentialCheckReport
        {
            CredentialType = "CommonIAM (client authentication)",
            IntegrationCommonIamTokenResult = integrationToken,
            ProductionCommonIamTokenResult = productionToken,
            IntegrationWhoAmI = integrationWhoAmI,
            ProductionWhoAmI = productionWhoAmI
        };
    }

    public static string FormatReport(CredentialCheckReport report)
    {
        var output = new StringBuilder();
        output.AppendLine();
        output.AppendLine($"Credential type: {report.CredentialType}");

        if (report.IntegrationCommonIamTokenResult != null)
        {
            output.AppendLine();
            output.Append(FormatCommonIamResult(report.IntegrationCommonIamTokenResult));
        }

        if (report.ProductionCommonIamTokenResult != null)
        {
            output.AppendLine();
            output.Append(FormatCommonIamResult(report.ProductionCommonIamTokenResult));
        }

        if (report.WhoAmIChecksSkipped)
        {
            output.AppendLine();
            output.AppendLine("WhoAmI checks: skipped  (no JWT token available)");
        }
        else
        {
            output.AppendLine();
            output.Append(FormatWhoAmIResult("Integration", report.IntegrationWhoAmI));
            output.AppendLine();
            output.AppendLine();
            output.Append(FormatWhoAmIResult("Production", report.ProductionWhoAmI));
        }

        output.AppendLine();
        output.AppendLine();
        output.Append(FormatDiagnosis(report));
        return output.ToString();
    }

    private static string FormatCommonIamResult(CommonIamTokenResult result)
    {
        var authLabel = CredentialCheckEndpoints.GetEnvironmentLabel(result.AuthEnvironment);
        var authHost = result.AuthEnvironment == CredentialAuthEnvironment.Integration
                           ? CredentialCheckEndpoints.IntegrationAuthHost
                           : CredentialCheckEndpoints.ProductionAuthHost;

        if (result.Success)
        {
            return $"CommonIAM token ({authLabel}, {authHost}): SUCCESS (HTTP {result.HttpStatusCode})";
        }

        return $"CommonIAM token ({authLabel}, {authHost}): FAILED"
               + (result.HttpStatusCode.HasValue ? $" (HTTP {result.HttpStatusCode})" : string.Empty)
               + Environment.NewLine
               + $"  Detail: {result.ErrorDetail}";
    }

    private static string FormatWhoAmIResult(string label, WhoAmIResult result)
    {
        if (result == null)
        {
            return $"{label} WhoAmI: not checked";
        }

        if (result.Success)
        {
            return $"{label} WhoAmI ({result.ApiHost}): SUCCESS (HTTP {result.HttpStatusCode}, bookmaker_id={result.BookmakerId}, virtual_host={result.VirtualHost})";
        }

        return $"{label} WhoAmI ({result.ApiHost}): FAILED"
               + (result.HttpStatusCode.HasValue ? $" (HTTP {result.HttpStatusCode})" : string.Empty)
               + Environment.NewLine
               + $"  Detail: {result.ErrorDetail}";
    }

    private static string FormatDiagnosis(CredentialCheckReport report)
    {
        if (report.WhoAmIChecksSkipped)
        {
            return "Diagnosis: CommonIAM rejected the credentials on both Integration and Production auth servers. Verify client ID, signing key ID, and private key.";
        }

        var integrationAuthOk = report.IntegrationCommonIamTokenResult?.Success == true;
        var productionAuthOk = report.ProductionCommonIamTokenResult?.Success == true;
        if (integrationAuthOk && !productionAuthOk)
        {
            return "Diagnosis: CommonIAM credentials are registered on Integration auth only (stg-auth.sportradar.com). Use Integration/GlobalIntegration SDK environment.";
        }

        if (productionAuthOk && !integrationAuthOk)
        {
            return "Diagnosis: CommonIAM credentials are registered on Production auth only (auth.sportradar.com). Use Production/GlobalProduction SDK environment.";
        }

        var integrationOk = report.IntegrationWhoAmI?.Success == true;
        var productionOk = report.ProductionWhoAmI?.Success == true;

        if (integrationOk && !productionOk)
        {
            return "Diagnosis: Credentials are valid for Integration only. Use Integration/GlobalIntegration SDK environment, or obtain Production credentials if Production access is required.";
        }

        if (productionOk && !integrationOk)
        {
            return "Diagnosis: Credentials are valid for Production only. Use Production/GlobalProduction SDK environment, or obtain Integration credentials if Integration access is required.";
        }

        if (integrationOk && productionOk)
        {
            return "Diagnosis: Credentials are accepted on both Integration and Production API hosts.";
        }

        return "Diagnosis: Credentials were not accepted on either Integration or Production API hosts. Verify the token is current, belongs to the expected bookmaker, and matches the intended environment.";
    }
}
