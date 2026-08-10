// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Configuration;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

public class BuilderBookmakerDetailsTests : ConfigurationBuilderSetup
{
    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.ReplayWithIntegrationCredentials)]
    [InlineData(SdkEnvironment.ReplayWithProductionCredentials)]
    public void ConfigurationIsBuiltForPredefinedEnvironmentWhenTokenSetterHasBookmakerDetailsThenBookmakerConfigurationIsNotEmpty(SdkEnvironment environment)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object,
                                     _ => BookmakerDetailsProvider,
                                     _ => ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureHu)
                    .Build();

        config.BookmakerDetails.ShouldNotBeEmpty();
    }

    [Fact]
    public void ConfigurationIsBuiltForCustomEnvironmentWhenTokenSetterHasBookmakerDetailsThenBookmakerConfigurationIsNotEmpty()
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object,
                                     _ => BookmakerDetailsProvider,
                                     _ => ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureHu)
                    .Build();

        config.BookmakerDetails.ShouldNotBeEmpty();
    }

    [Fact]
    public void RetrievesProductionBookmakerId()
    {
        var bookmakerDetails = BookmakerDetailsEndpoint.GetValidBookmakerDetails();

        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                        .WithStubbedProductionBookmakerDetails(bookmakerDetails)
                                                        .WithOneProducer()
                                                        .BuildTokenSetter();
        var config = tokenSetter
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(SdkEnvironment.Production)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.BookmakerDetails.BookmakerId.ShouldBe(bookmakerDetails.bookmaker_id);
    }

    [Fact]
    public void ReplayWithIntegrationCredentialsRetrievesIntegrationBookmakerIdWhenIntegrationSucceeds()
    {
        var integrationBookmakerDetails = BookmakerDetailsEndpoint.GetValidBookmakerDetails();

        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithStubbedIntegrationBookmakerDetails(integrationBookmakerDetails)
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();
        var config = tokenSetter
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(SdkEnvironment.ReplayWithIntegrationCredentials)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.BookmakerDetails.BookmakerId.ShouldBe(integrationBookmakerDetails.bookmaker_id);
    }

    [Fact]
    public void ReplayWithProductionCredentialsRetrievesProductionBookmakerIdWhenProductionSucceeds()
    {
        var productionBookmakerDetails = BookmakerDetailsEndpoint.GetValidBookmakerDetails();

        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithStubbedProductionBookmakerDetails(productionBookmakerDetails)
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();
        var config = tokenSetter
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(SdkEnvironment.ReplayWithProductionCredentials)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.BookmakerDetails.BookmakerId.ShouldBe(productionBookmakerDetails.bookmaker_id);
    }

    [Theory]
    [InlineData(SdkEnvironment.ReplayWithIntegrationCredentials)]
    [InlineData(SdkEnvironment.ReplayWithProductionCredentials)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    public void ReplayWithAccessTokenThrowsWhenConfiguredEnvironmentFailsAndStrategyIsThrow(SdkEnvironment environment)
    {
        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithFailingBookmakerDetailsFor(environment)
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();

        Should.Throw<CommunicationException>(() => tokenSetter
                                                .SetAccessToken(TestConsts.AnyAccessToken)
                                                .SelectEnvironment(environment)
                                                .SetDefaultLanguage(TestConsts.CultureEn)
                                                .SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Throw)
                                                .Build());
    }

    [Theory]
    [InlineData(SdkEnvironment.ReplayWithIntegrationCredentials)]
    [InlineData(SdkEnvironment.ReplayWithProductionCredentials)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    public void EnvironmentWithClientAuthenticationThrowsWhenConfiguredEnvironmentFailsAndStrategyIsThrow(SdkEnvironment environment)
    {
        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithFailingBookmakerDetailsFor(environment)
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();

        Should.Throw<CommunicationException>(() => tokenSetter
                                                  .SetClientAuthentication(TestConsts.AnyPrivateKeyJwt)
                                                  .SelectEnvironment(environment)
                                                  .SetDefaultLanguage(TestConsts.CultureEn)
                                                  .SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Throw)
                                                  .Build());
    }

    [Theory]
    [InlineData(SdkEnvironment.ReplayWithIntegrationCredentials)]
    [InlineData(SdkEnvironment.ReplayWithProductionCredentials)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    public void ReplayWithAccessTokenThrowsWhenConfiguredEnvironmentFailsAndStrategyIsCatch(SdkEnvironment environment)
    {
        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithFailingBookmakerDetailsFor(environment)
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();

        Should.Throw<CommunicationException>(() => tokenSetter
                                                  .SetAccessToken(TestConsts.AnyAccessToken)
                                                  .SelectEnvironment(environment)
                                                  .SetDefaultLanguage(TestConsts.CultureEn)
                                                  .SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Catch)
                                                  .Build());
    }

    [Theory]
    [InlineData(SdkEnvironment.ReplayWithIntegrationCredentials)]
    [InlineData(SdkEnvironment.ReplayWithProductionCredentials)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    public void EnvironmentWithClientAuthenticationThrowsWhenConfiguredEnvironmentFailsAndStrategyIsCatch(SdkEnvironment environment)
    {
        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithFailingBookmakerDetailsFor(environment)
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();

        Should.Throw<CommunicationException>(() => tokenSetter
                                                  .SetClientAuthentication(TestConsts.AnyPrivateKeyJwt)
                                                  .SelectEnvironment(environment)
                                                  .SetDefaultLanguage(TestConsts.CultureEn)
                                                  .SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Catch)
                                                  .Build());
    }

    [Fact]
    public void ReplayWithIntegrationCredentialsUsesIntegrationApiHost()
    {
        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithStubbedIntegrationBookmakerDetails(BookmakerDetailsEndpoint.GetValidBookmakerDetails())
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();
        var config = tokenSetter
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(SdkEnvironment.ReplayWithIntegrationCredentials)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Api.Host.ShouldBe(EnvironmentManager.GetApiHost(SdkEnvironment.Integration));
    }

    [Fact]
    public void ReplayWithIntegrationCredentialsUsesReplayRabbitMqHost()
    {
        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithStubbedIntegrationBookmakerDetails(BookmakerDetailsEndpoint.GetValidBookmakerDetails())
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();
        var config = tokenSetter
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(SdkEnvironment.ReplayWithIntegrationCredentials)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Rabbit.Host.ShouldBe(EnvironmentManager.GetMqHost(SdkEnvironment.ReplayWithIntegrationCredentials));
    }

    [Fact]
    public void ReplayWithProductionCredentialsUsesProductionApiHost()
    {
        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithStubbedProductionBookmakerDetails(BookmakerDetailsEndpoint.GetValidBookmakerDetails())
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();
        var config = tokenSetter
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(SdkEnvironment.ReplayWithProductionCredentials)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Api.Host.ShouldBe(EnvironmentManager.GetApiHost(SdkEnvironment.Production));
    }

    [Fact]
    public void ReplayWithProductionCredentialsUsesReplayRabbitMqHost()
    {
        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithStubbedProductionBookmakerDetails(BookmakerDetailsEndpoint.GetValidBookmakerDetails())
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();
        var config = tokenSetter
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(SdkEnvironment.ReplayWithProductionCredentials)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Rabbit.Host.ShouldBe(EnvironmentManager.GetMqHost(SdkEnvironment.ReplayWithProductionCredentials));
    }
}
