// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
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
    [InlineData(SdkEnvironment.Replay)]
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
    public void ReplayEnvironmentRetrievesIntegrationBookmakerIdWhenIntegrationSucceedsAndProductionFails()
    {
        var integrationBookmakerDetails = BookmakerDetailsEndpoint.GetValidBookmakerDetails();

        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithStubbedIntegrationBookmakerDetailsAndProductionFailure(integrationBookmakerDetails)
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();
        var config = tokenSetter
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectReplay()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.BookmakerDetails.BookmakerId.ShouldBe(integrationBookmakerDetails.bookmaker_id);
    }

    [Fact]
    public void ReplayEnvironmentFallsBackToProductionBookmakerDetailsWhenIntegrationFailsAndProductionSucceeds()
    {
        var productionBookmakerDetails = BookmakerDetailsEndpoint.GetValidBookmakerDetails();

        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithStubbedProductionBookmakerDetailsAndIntegrationFailure(productionBookmakerDetails)
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();
        var config = tokenSetter
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectReplay()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.BookmakerDetails.BookmakerId.ShouldBe(productionBookmakerDetails.bookmaker_id);
    }

    [Fact]
    public void ReplayEnvironmentReturnsIntegrationBookmakerIdWhenBothEnvironmentsSucceed()
    {
        var integrationBookmakerDetails = BookmakerDetailsEndpoint.GetValidBookmakerDetails();
        integrationBookmakerDetails.bookmaker_id = 100;
        var productionBookmakerDetails = BookmakerDetailsEndpoint.GetValidBookmakerDetails();
        productionBookmakerDetails.bookmaker_id = 200;

        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithStubbedIntegrationAndProductionBookmakerDetails(integrationBookmakerDetails, productionBookmakerDetails)
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();
        var config = tokenSetter
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectReplay()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.BookmakerDetails.BookmakerId.ShouldBe(integrationBookmakerDetails.bookmaker_id);
    }

    [Theory]
    [InlineData(ExceptionHandlingStrategy.Catch)]
    [InlineData(ExceptionHandlingStrategy.Throw)]
    public void ReplayEnvironmentReturnsNullBookmakerDetailsWhenBothEnvironmentsFail(ExceptionHandlingStrategy exceptionHandlingStrategy)
    {
        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();
        var config = tokenSetter
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectReplay()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetExceptionHandlingStrategy(exceptionHandlingStrategy)
                    .Build();

        config.BookmakerDetails.ShouldBeNull();
    }

    [Fact]
    public void ReplayEnvironmentSetsApiHostToIntegrationHostWhenIntegrationSucceeds()
    {
        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithStubbedIntegrationBookmakerDetailsAndProductionFailure(BookmakerDetailsEndpoint.GetValidBookmakerDetails())
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();
        var config = tokenSetter
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectReplay()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Api.Host.ShouldBe(EnvironmentManager.GetApiHost(SdkEnvironment.Integration));
    }

    [Fact]
    public void ReplayEnvironmentSetsRabbitHostToReplayMqHostWhenIntegrationSucceeds()
    {
        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithStubbedIntegrationBookmakerDetailsAndProductionFailure(BookmakerDetailsEndpoint.GetValidBookmakerDetails())
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();
        var config = tokenSetter
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectReplay()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Rabbit.Host.ShouldBe(EnvironmentManager.GetMqHost(SdkEnvironment.Replay));
    }

    [Fact]
    public void ReplayEnvironmentSetsApiHostToProductionHostWhenOnlyProductionSucceeds()
    {
        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithStubbedProductionBookmakerDetailsAndIntegrationFailure(BookmakerDetailsEndpoint.GetValidBookmakerDetails())
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();
        var config = tokenSetter
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectReplay()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Api.Host.ShouldBe(EnvironmentManager.GetApiHost(SdkEnvironment.Production));
    }

    [Fact]
    public void ReplayEnvironmentSetsRabbitHostToReplayMqHostWhenOnlyProductionSucceeds()
    {
        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithStubbedProductionBookmakerDetailsAndIntegrationFailure(BookmakerDetailsEndpoint.GetValidBookmakerDetails())
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();
        var config = tokenSetter
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectReplay()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Rabbit.Host.ShouldBe(EnvironmentManager.GetMqHost(SdkEnvironment.Replay));
    }

    [Fact]
    public void ReplayEnvironmentSetsApiHostToIntegrationHostWhenBothEnvironmentsSucceed()
    {
        var integrationBookmakerDetails = BookmakerDetailsEndpoint.GetValidBookmakerDetails();
        integrationBookmakerDetails.bookmaker_id = 100;
        var productionBookmakerDetails = BookmakerDetailsEndpoint.GetValidBookmakerDetails();
        productionBookmakerDetails.bookmaker_id = 200;

        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithStubbedIntegrationAndProductionBookmakerDetails(integrationBookmakerDetails, productionBookmakerDetails)
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();
        var config = tokenSetter
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectReplay()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Api.Host.ShouldBe(EnvironmentManager.GetApiHost(SdkEnvironment.Integration));
    }

    [Fact]
    public void ReplayEnvironmentSetsRabbitHostToReplayMqHostWhenBothEnvironmentsSucceed()
    {
        var integrationBookmakerDetails = BookmakerDetailsEndpoint.GetValidBookmakerDetails();
        integrationBookmakerDetails.bookmaker_id = 100;
        var productionBookmakerDetails = BookmakerDetailsEndpoint.GetValidBookmakerDetails();
        productionBookmakerDetails.bookmaker_id = 200;

        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithStubbedIntegrationAndProductionBookmakerDetails(integrationBookmakerDetails, productionBookmakerDetails)
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();
        var config = tokenSetter
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectReplay()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Rabbit.Host.ShouldBe(EnvironmentManager.GetMqHost(SdkEnvironment.Replay));
    }
}
