// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

// ReSharper disable TooManyChainedReferences

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

public class BuilderSimplePropertiesTests : ConfigurationBuilderSetup
{
    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Replay)]
    public void WhenAccessTokenSetForPredefinedEnvironmentThenConfigurationHasAccessToken(SdkEnvironment environment)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureHu)
                    .Build();

        config.AccessToken.ShouldBe(TestConsts.AnyAccessToken);
    }

    [Fact]
    public void WhenAccessTokenSetForCustomEnvironmentThenConfigurationHasAccessToken()
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureHu)
                    .Build();

        config.AccessToken.ShouldBe(TestConsts.AnyAccessToken);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration, -111)]
    [InlineData(SdkEnvironment.Integration, 0)]
    [InlineData(SdkEnvironment.Integration, 111)]
    [InlineData(SdkEnvironment.Production, -111)]
    [InlineData(SdkEnvironment.Production, 0)]
    [InlineData(SdkEnvironment.Production, 111)]
    [InlineData(SdkEnvironment.GlobalIntegration, -111)]
    [InlineData(SdkEnvironment.GlobalIntegration, 0)]
    [InlineData(SdkEnvironment.GlobalIntegration, 111)]
    [InlineData(SdkEnvironment.GlobalProduction, -111)]
    [InlineData(SdkEnvironment.GlobalProduction, 0)]
    [InlineData(SdkEnvironment.GlobalProduction, 111)]
    [InlineData(SdkEnvironment.Replay, -111)]
    [InlineData(SdkEnvironment.Replay, 0)]
    [InlineData(SdkEnvironment.Replay, 111)]
    public void WhenNodeIdSetForPredefinedEnvironmentThenConfigurationHasNodeId(SdkEnvironment environment, int nodeId)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetNodeId(nodeId)
                    .Build();

        config.NodeId.ShouldBe(nodeId);
    }

    [Theory]
    [InlineData(-111)]
    [InlineData(0)]
    [InlineData(111)]
    public void WhenNodeIdSetForCustomEnvironmentThenConfigurationHasNodeId(int nodeId)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetNodeId(nodeId)
                    .Build();

        config.NodeId.ShouldBe(nodeId);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration, ExceptionHandlingStrategy.Throw)]
    [InlineData(SdkEnvironment.Integration, ExceptionHandlingStrategy.Catch)]
    [InlineData(SdkEnvironment.Production, ExceptionHandlingStrategy.Throw)]
    [InlineData(SdkEnvironment.Production, ExceptionHandlingStrategy.Catch)]
    [InlineData(SdkEnvironment.GlobalIntegration, ExceptionHandlingStrategy.Throw)]
    [InlineData(SdkEnvironment.GlobalIntegration, ExceptionHandlingStrategy.Catch)]
    [InlineData(SdkEnvironment.GlobalProduction, ExceptionHandlingStrategy.Throw)]
    [InlineData(SdkEnvironment.GlobalProduction, ExceptionHandlingStrategy.Catch)]
    [InlineData(SdkEnvironment.Replay, ExceptionHandlingStrategy.Throw)]
    [InlineData(SdkEnvironment.Replay, ExceptionHandlingStrategy.Catch)]
    public void WhenExceptionHandlingStrategySetForPredefinedEnvironmentThenConfigurationHasStrategy(SdkEnvironment environment, ExceptionHandlingStrategy strategy)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetExceptionHandlingStrategy(strategy)
                    .Build();

        config.ExceptionHandlingStrategy.ShouldBe(strategy);
    }

    [Theory]
    [InlineData(ExceptionHandlingStrategy.Throw)]
    [InlineData(ExceptionHandlingStrategy.Catch)]
    public void WhenExceptionHandlingStrategySetForCustomEnvironmentThenConfigurationHasStrategy(ExceptionHandlingStrategy strategy)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetExceptionHandlingStrategy(strategy)
                    .Build();

        config.ExceptionHandlingStrategy.ShouldBe(strategy);
    }

    [Fact]
    public void WhenReplaySelectedViaPredefinedEnvironmentAndShortcutThenConfigurationsAreEquivalent()
    {
        var configViaEnv = new TokenSetter(
                                           UofConfigurationSectionProviderMock.Object,
                                           BookmakerDetailsProvider,
                                           ProducersProvider)
                          .SetAccessToken(TestConsts.AnyAccessToken)
                          .SelectEnvironment(SdkEnvironment.Replay)
                          .SetDefaultLanguage(TestConsts.CultureEn)
                          .Build();

        var configViaReplay = new TokenSetter(
                                              UofConfigurationSectionProviderMock.Object,
                                              BookmakerDetailsProvider,
                                              ProducersProvider)
                             .SetAccessToken(TestConsts.AnyAccessToken)
                             .SelectReplay()
                             .SetDefaultLanguage(TestConsts.CultureEn)
                             .Build();

        configViaReplay.ShouldBeEquivalentTo(configViaEnv);
    }
}
