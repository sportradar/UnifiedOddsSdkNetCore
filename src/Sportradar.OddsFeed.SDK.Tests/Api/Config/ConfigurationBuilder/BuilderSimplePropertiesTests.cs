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
    [InlineData(SdkEnvironment.ReplayWithIntegrationCredentials)]
    [InlineData(SdkEnvironment.ReplayWithProductionCredentials)]
    public void WhenAccessTokenSetForPredefinedEnvironmentThenConfigurationHasAccessToken(SdkEnvironment environment)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, _ => BookmakerDetailsProvider, _ => ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureHu)
                    .Build();

        config.AccessToken.ShouldBe(TestConsts.AnyAccessToken);
    }

    [Fact]
    public void WhenAccessTokenSetForCustomEnvironmentThenConfigurationHasAccessToken()
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, _ => BookmakerDetailsProvider, _ => ProducersProvider)
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
    [InlineData(SdkEnvironment.ReplayWithIntegrationCredentials, -111)]
    [InlineData(SdkEnvironment.ReplayWithProductionCredentials, -111)]
    [InlineData(SdkEnvironment.ReplayWithIntegrationCredentials, 0)]
    [InlineData(SdkEnvironment.ReplayWithProductionCredentials, 0)]
    [InlineData(SdkEnvironment.ReplayWithIntegrationCredentials, 111)]
    [InlineData(SdkEnvironment.ReplayWithProductionCredentials, 111)]
    public void WhenNodeIdSetForPredefinedEnvironmentThenConfigurationHasNodeId(SdkEnvironment environment, int nodeId)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, _ => BookmakerDetailsProvider, _ => ProducersProvider)
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
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, _ => BookmakerDetailsProvider, _ => ProducersProvider)
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
    [InlineData(SdkEnvironment.ReplayWithIntegrationCredentials, ExceptionHandlingStrategy.Throw)]
    [InlineData(SdkEnvironment.ReplayWithProductionCredentials, ExceptionHandlingStrategy.Throw)]
    [InlineData(SdkEnvironment.ReplayWithIntegrationCredentials, ExceptionHandlingStrategy.Catch)]
    [InlineData(SdkEnvironment.ReplayWithProductionCredentials, ExceptionHandlingStrategy.Catch)]
    public void WhenExceptionHandlingStrategySetForPredefinedEnvironmentThenConfigurationHasStrategy(SdkEnvironment environment, ExceptionHandlingStrategy strategy)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, _ => BookmakerDetailsProvider, _ => ProducersProvider)
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
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, _ => BookmakerDetailsProvider, _ => ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetExceptionHandlingStrategy(strategy)
                    .Build();

        config.ExceptionHandlingStrategy.ShouldBe(strategy);
    }
}
