// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public class EnvironmentManagerTests
{
    [Theory]
    [InlineData(SdkEnvironment.ReplayWithIntegrationCredentials, "stgapi.betradar.com")]
    [InlineData(SdkEnvironment.ReplayWithProductionCredentials, "api.betradar.com")]
    public void ReplayShouldPointToCorrectApiHost(SdkEnvironment replayEnvironment, string expectedApiHost)
    {
        Assert.Equal(expectedApiHost, GetReplayEnvironmentSetting(replayEnvironment).ApiHost);
        Assert.Equal(expectedApiHost, EnvironmentManager.GetSetting(replayEnvironment).ApiHost);
        Assert.Equal(expectedApiHost, EnvironmentManager.GetApiHost(replayEnvironment));
    }

    [Theory]
    [InlineData(SdkEnvironment.ReplayWithIntegrationCredentials)]
    [InlineData(SdkEnvironment.ReplayWithProductionCredentials)]
    public void ReplayEnvironmentSettingsShouldUseReplayMessagingEndpoint(SdkEnvironment replayEnvironment)
    {
        const string nonGlobalMessagingHost = "replaymq.betradar.com";
        Assert.Equal(nonGlobalMessagingHost, GetReplayEnvironmentSetting(replayEnvironment).MqHost);
        Assert.Equal(nonGlobalMessagingHost, EnvironmentManager.GetSetting(replayEnvironment).MqHost);
        Assert.Equal(nonGlobalMessagingHost, EnvironmentManager.GetMqHost(replayEnvironment));
    }

    [Theory]
    [InlineData(SdkEnvironment.ReplayWithIntegrationCredentials)]
    [InlineData(SdkEnvironment.ReplayWithProductionCredentials)]
    public void ReplayShouldSupportSslOnlyConnections(SdkEnvironment replayEnvironment)
    {
        Assert.True(GetReplayEnvironmentSetting(replayEnvironment).OnlySsl);
        Assert.True(EnvironmentManager.GetSetting(replayEnvironment).OnlySsl);
    }

    [Theory]
    [InlineData(SdkEnvironment.Production, "auth.sportradar.com")]
    [InlineData(SdkEnvironment.GlobalProduction, "auth.sportradar.com")]
    [InlineData(SdkEnvironment.ReplayWithProductionCredentials, "auth.sportradar.com")]
    [InlineData(SdkEnvironment.Integration, "stg-auth.sportradar.com")]
    [InlineData(SdkEnvironment.GlobalIntegration, "stg-auth.sportradar.com")]
    [InlineData(SdkEnvironment.ReplayWithIntegrationCredentials, "stg-auth.sportradar.com")]
    public void GetAuthenticationHostShouldReturnCorrectHostForValidEnvironments(SdkEnvironment environment, string expectedHost)
    {
        var result = EnvironmentManager.GetAuthenticationHost(environment);
        Assert.Equal(expectedHost, result);
    }

    [Theory]
    [InlineData(SdkEnvironment.Custom)]
    public void GetAuthenticationHostShouldThrowArgumentOutOfRangeExceptionForUnsupportedEnvironments(SdkEnvironment environment)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EnvironmentManager.GetAuthenticationHost(environment));
    }

    private static EnvironmentSetting GetReplayEnvironmentSetting(SdkEnvironment replayEnvironment)
    {
        var replaySettings = EnvironmentManager.EnvironmentSettings.Find(e => e.Environment == replayEnvironment);
        Assert.NotNull(replaySettings);
        return replaySettings;
    }
}
