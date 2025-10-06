// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

public class BuilderApiConfigurationTests : ConfigurationBuilderSetup
{
    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Replay)]
    public void WhenBuiltForPredefinedEnvironmentThenApiConfigurationHasDefaultValues(SdkEnvironment environment)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Api.ShouldMatchApiDefaultsForNonCustomEnvironment(environment);
    }

    [Fact]
    public void WhenBuiltForCustomEnvironmentThenApiConfigurationHasDefaultValues()
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Api.ShouldMatchApiDefaultsForCustomEnvironment();
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.HttpClientTimeoutMin)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.HttpClientTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.HttpClientTimeoutMax)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.HttpClientTimeoutMin)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.HttpClientTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.HttpClientTimeoutMax)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.HttpClientTimeoutMin)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.HttpClientTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.HttpClientTimeoutMax)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.HttpClientTimeoutMin)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.HttpClientTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.HttpClientTimeoutMax)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.HttpClientTimeoutMin)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.HttpClientTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.HttpClientTimeoutMax)]
    public void WhenHttpClientTimeoutSetForPredefinedEnvironmentThenApiConfigurationHasExpectedTimeout(SdkEnvironment environment, int timeoutSeconds)
    {
        var expected = TimeSpan.FromSeconds(timeoutSeconds);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetHttpClientTimeout(timeoutSeconds)
                    .Build();

        config.Api.HttpClientTimeout.ShouldBe(expected);
    }

    [Theory]
    [InlineData(ConfigLimit.HttpClientTimeoutMin)]
    [InlineData(ConfigLimit.HttpClientTimeoutDefault + 1)]
    [InlineData(ConfigLimit.HttpClientTimeoutMax)]
    public void WhenHttpClientTimeoutSetForCustomEnvironmentThenApiConfigurationHasExpectedTimeout(int timeoutSeconds)
    {
        var expected = TimeSpan.FromSeconds(timeoutSeconds);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetHttpClientTimeout(timeoutSeconds)
                    .Build();

        config.Api.HttpClientTimeout.ShouldBe(expected);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.HttpClientRecoveryTimeoutMin)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.HttpClientRecoveryTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.HttpClientRecoveryTimeoutMax)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.HttpClientRecoveryTimeoutMin)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.HttpClientRecoveryTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.HttpClientRecoveryTimeoutMax)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.HttpClientRecoveryTimeoutMin)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.HttpClientRecoveryTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.HttpClientRecoveryTimeoutMax)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.HttpClientRecoveryTimeoutMin)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.HttpClientRecoveryTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.HttpClientRecoveryTimeoutMax)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.HttpClientRecoveryTimeoutMin)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.HttpClientRecoveryTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.HttpClientRecoveryTimeoutMax)]
    public void WhenHttpClientRecoveryTimeoutSetForPredefinedEnvironmentThenApiConfigurationHasExpectedRecoveryTimeout(SdkEnvironment environment, int timeoutSeconds)
    {
        var expected = TimeSpan.FromSeconds(timeoutSeconds);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetHttpClientRecoveryTimeout(timeoutSeconds)
                    .Build();

        config.Api.HttpClientRecoveryTimeout.ShouldBe(expected);
    }

    [Theory]
    [InlineData(ConfigLimit.HttpClientRecoveryTimeoutMin)]
    [InlineData(ConfigLimit.HttpClientRecoveryTimeoutDefault + 1)]
    [InlineData(ConfigLimit.HttpClientRecoveryTimeoutMax)]
    public void WhenHttpClientRecoveryTimeoutSetForCustomEnvironmentThenApiConfigurationHasExpectedRecoveryTimeout(int timeoutSeconds)
    {
        var expected = TimeSpan.FromSeconds(timeoutSeconds);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetHttpClientRecoveryTimeout(timeoutSeconds)
                    .Build();

        config.Api.HttpClientRecoveryTimeout.ShouldBe(expected);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.HttpClientFastFailingTimeoutMin)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.HttpClientFastFailingTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.HttpClientFastFailingTimeoutMax)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.HttpClientFastFailingTimeoutMin)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.HttpClientFastFailingTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.HttpClientFastFailingTimeoutMax)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.HttpClientFastFailingTimeoutMin)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.HttpClientFastFailingTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.HttpClientFastFailingTimeoutMax)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.HttpClientFastFailingTimeoutMin)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.HttpClientFastFailingTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.HttpClientFastFailingTimeoutMax)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.HttpClientFastFailingTimeoutMin)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.HttpClientFastFailingTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.HttpClientFastFailingTimeoutMax)]
    public void WhenHttpClientFastFailingTimeoutSetForPredefinedEnvironmentThenApiConfigurationHasExpectedFastFailingTimeout(SdkEnvironment environment, int timeoutSeconds)
    {
        var expected = TimeSpan.FromSeconds(timeoutSeconds);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetHttpClientFastFailingTimeout(timeoutSeconds)
                    .Build();

        config.Api.HttpClientFastFailingTimeout.ShouldBe(expected);
    }

    [Theory]
    [InlineData(ConfigLimit.HttpClientFastFailingTimeoutMin)]
    [InlineData(ConfigLimit.HttpClientFastFailingTimeoutDefault + 1)]
    [InlineData(ConfigLimit.HttpClientFastFailingTimeoutMax)]
    public void WhenHttpClientFastFailingTimeoutSetForCustomEnvironmentThenApiConfigurationHasExpectedFastFailingTimeout(int timeoutSeconds)
    {
        var expected = TimeSpan.FromSeconds(timeoutSeconds);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetHttpClientFastFailingTimeout(timeoutSeconds)
                    .Build();

        config.Api.HttpClientFastFailingTimeout.ShouldBe(expected);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration, 123)]
    [InlineData(SdkEnvironment.Production, 123)]
    [InlineData(SdkEnvironment.GlobalIntegration, 123)]
    [InlineData(SdkEnvironment.GlobalProduction, 123)]
    [InlineData(SdkEnvironment.Replay, 123)]
    public void WhenMaxConnectionsPerServerSetForPredefinedEnvironmentThenApiConfigurationHasExpectedMaxConnectionsPerServer(SdkEnvironment environment, int maxConnectionsPerServer)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetMaxConnectionsPerServer(maxConnectionsPerServer)
                    .Build();

        config.Api.MaxConnectionsPerServer.ShouldBe(maxConnectionsPerServer);
    }

    [Fact]
    public void WhenMaxConnectionsPerServerSetForCustomEnvironmentThenApiConfigurationHasExpectedMaxConnectionsPerServer()
    {
        const int maxConnectionsPerServer = 5;
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetMaxConnectionsPerServer(maxConnectionsPerServer)
                    .Build();

        config.Api.MaxConnectionsPerServer.ShouldBe(maxConnectionsPerServer);
    }
}
