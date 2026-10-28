// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.DemoProject.Utils;
using Sportradar.OddsFeed.SDK.DemoProject.Utils.CredentialCheck;

namespace Sportradar.OddsFeed.SDK.DemoProject.Example;

/// <summary>
/// Standalone credential troubleshooting utility (FEEDSDK-3213).
/// Checks credentials against Integration and Production without starting the SDK.
/// </summary>
public sealed class CredentialCheckExample(ILogger<CredentialCheckExample> logger) : ExampleBase(logger)
{
    public override void Run()
    {
        RunAsync().GetAwaiter().GetResult();
    }

    private async Task RunAsync()
    {
        Log.LogInformation("Running credential troubleshooting utility");

        Console.WriteLine();
        Console.WriteLine("Select credential type:");
        Console.WriteLine("  1 - SSO access token");
        Console.WriteLine("  2 - CommonIAM (client ID + signing key ID + private key)");
        Console.Write("Enter number: ");

        var selection = Console.ReadLine()?.Trim();
        Console.WriteLine();

        CredentialCheckReport report;
        try
        {
            switch (selection)
            {
                case "1":
                    report = await CheckAccessTokenAsync().ConfigureAwait(false);
                    break;
                case "2":
                    report = await CheckCommonIamAsync().ConfigureAwait(false);
                    break;
                default:
                    Console.WriteLine("Invalid selection.");
                    return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"Credential check failed: {ex.Message}");
            Log.LogError(ex, "Credential check failed");
            return;
        }

        var formatted = CredentialChecker.FormatReport(report);
        Console.WriteLine(formatted);
        Log.LogInformation(formatted);
    }

    private static async Task<CredentialCheckReport> CheckAccessTokenAsync()
    {
        Console.Write("Use access token from App.config? (y|n): ");
        var useConfig = Console.ReadLine()?.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) == true;

        string accessToken;
        if (useConfig)
        {
            accessToken = AppConfigAccessTokenReader.ReadAccessToken();
            Console.WriteLine("Using access token from App.config.");
        }
        else
        {
            accessToken = ConsoleHelper.ReadNonEmpty("Enter access token: ");
        }

        return await CredentialChecker.CheckAccessTokenAsync(accessToken).ConfigureAwait(false);
    }

    private static async Task<CredentialCheckReport> CheckCommonIamAsync()
    {
        var clientId = ConsoleHelper.ReadNonEmpty("Enter Client ID: ");
        var signingKeyId = ConsoleHelper.ReadNonEmpty("Enter Signing Key ID: ");
        var privateKeyPath = ConsoleHelper.ReadExistingPemPath("Enter path to Private Key (.pem): ");
        var privateKey = RsaPkcs8Loader.LoadPkcs8RsaKey(privateKeyPath, signingKeyId);

        var credentials = UofClientAuthentication
                         .PrivateKeyJwt()
                         .SetSigningKeyId(signingKeyId)
                         .SetClientId(clientId)
                         .SetPrivateKey(privateKey)
                         .Build();

        return await CredentialChecker.CheckCommonIamAsync(credentials).ConfigureAwait(false);
    }
}
