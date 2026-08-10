// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils.CredentialCheck;

/// <summary>
/// Reads the access token from App.config without building the full SDK configuration.
/// </summary>
internal static class AppConfigAccessTokenReader
{
    private const string SectionName = "uofSdkSection";
    private const string PlaceholderToken = "BETRADAR_ACCESS_TOKEN";

    public static string ReadAccessToken()
    {
        var configFile = $"{Assembly.GetExecutingAssembly().Location}.config";
        if (!File.Exists(configFile))
        {
            throw new InvalidOperationException($"Could not find App.config at '{configFile}'.");
        }

        var document = XDocument.Load(configFile);
        var accessToken = document.Root?.Element(SectionName)?.Attribute("accessToken")?.Value;
        if (string.IsNullOrWhiteSpace(accessToken) || accessToken.Equals(PlaceholderToken, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("App.config does not contain a valid accessToken value.");
        }

        return accessToken;
    }
}
