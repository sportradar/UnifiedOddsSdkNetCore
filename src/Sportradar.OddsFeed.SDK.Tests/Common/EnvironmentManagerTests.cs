// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public class EnvironmentManagerTests
{
    [Fact]
    public void GlobalAndNonGlobalApiSitUnderSameIpsButReplayShouldPointToNonGlobalAsItIsLongTermStrategy()
    {
        const string nonGlobalApiHost = "stgapi.betradar.com";
        Assert.Equal(nonGlobalApiHost, FindReplayEnvironmentSetting().ApiHost);
        Assert.Equal(nonGlobalApiHost, EnvironmentManager.GetSetting(SdkEnvironment.Replay).ApiHost);
        Assert.Equal(nonGlobalApiHost, EnvironmentManager.GetApiHost(SdkEnvironment.Replay));
    }

    [Fact]
    public void ReplayShouldPointToNonGlobalMessagingEndpointAsItIsLongTermStrategy()
    {
        const string nonGlobalMessagingHost = "replaymq.betradar.com";
        Assert.Equal(nonGlobalMessagingHost, FindReplayEnvironmentSetting().MqHost);
        Assert.Equal(nonGlobalMessagingHost, EnvironmentManager.GetSetting(SdkEnvironment.Replay).MqHost);
        Assert.Equal(nonGlobalMessagingHost, EnvironmentManager.GetMqHost(SdkEnvironment.Replay));
    }

    [Fact]
    public void ReplayShouldSupportSslOnlyConnections()
    {
        Assert.True(FindReplayEnvironmentSetting().OnlySsl);
        Assert.True(EnvironmentManager.GetSetting(SdkEnvironment.Replay).OnlySsl);
    }

    [Fact]
    public void RetryListShouldBeDeprecatedAsItIsNotOnlyNeverUsedInReplayButItIsExposedThroughStaticContextToCustomerSusceptibleToBreakingChange()
    {
        Assert.NotNull(FindReplayEnvironmentSetting().EnvironmentRetryList);
        Assert.NotNull(EnvironmentManager.GetSetting(SdkEnvironment.Replay).EnvironmentRetryList);
    }

    [Theory]
    [InlineData(SdkEnvironment.Production, "auth.sportradar.com")]
    [InlineData(SdkEnvironment.GlobalProduction, "auth.sportradar.com")]
    [InlineData(SdkEnvironment.Integration, "stg-auth.sportradar.com")]
    [InlineData(SdkEnvironment.GlobalIntegration, "stg-auth.sportradar.com")]
    public void GetAuthenticationHostShouldReturnCorrectHostForValidEnvironments(SdkEnvironment environment, string expectedHost)
    {
        var result = EnvironmentManager.GetAuthenticationHost(environment);
        Assert.Equal(expectedHost, result);
    }

    [Theory]
    [InlineData(SdkEnvironment.Custom)]
    [InlineData(SdkEnvironment.Replay)]
    public void GetAuthenticationHostShouldThrowArgumentOutOfRangeExceptionForUnsupportedEnvironments(SdkEnvironment environment)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EnvironmentManager.GetAuthenticationHost(environment));
    }

    private EnvironmentSetting FindReplayEnvironmentSetting()
    {
        var replaySettings = EnvironmentManager.EnvironmentSettings.Find(e => e.Environment == SdkEnvironment.Replay);
        Assert.NotNull(replaySettings);
        return replaySettings;
    }
}
