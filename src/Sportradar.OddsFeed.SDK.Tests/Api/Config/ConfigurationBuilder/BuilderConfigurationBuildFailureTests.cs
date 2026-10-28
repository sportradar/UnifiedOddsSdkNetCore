// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Net;
using Shouldly;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Configuration;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;
using Sportradar.OddsFeed.SDK.Tests.Common.Helpers;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

public class BuilderConfigurationBuildFailureTests : ConfigurationBuilderSetup
{
    private const string CommonIamFailureMessage = "Configuration build failed: Common IAM token acquisition failed.";

    [Theory]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.ReplayWithIntegrationCredentials)]
    [InlineData(SdkEnvironment.ReplayWithProductionCredentials)]
    public void BuildWhenWhoAmiFetchFailsWithNonCiamErrorThenRethrowsCommunicationException(SdkEnvironment environment)
    {
        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithBookmakerDetailsFetcher(DataFetcherMockHelper.GetDataFetcherThrowingCommunicationExceptionOnGetData())
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();

        var exception = Should.Throw<CommunicationException>(() => tokenSetter
            .SetAccessToken(TestConsts.AnyAccessToken)
            .SelectEnvironment(environment)
            .SetDefaultLanguage(TestConsts.CultureEn)
            .Build());

        exception.Message.ShouldBe("Fetching bookmaker details failed.");
        exception.InnerException.ShouldNotBeNull();
    }

    [Fact]
    public void BuildWhenWhoAmiFetchFailsForCustomEnvironmentThenRethrowsCommunicationException()
    {
        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithBookmakerDetailsFetcher(DataFetcherMockHelper.GetDataFetcherThrowingCommunicationExceptionOnGetData())
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();

        var exception = Should.Throw<CommunicationException>(() => tokenSetter
            .SetAccessToken(TestConsts.AnyAccessToken)
            .SelectCustom()
            .SetApiHost(TestConsts.AnyApiHost)
            .SetMessagingHost(TestConsts.AnyRabbitHost)
            .SetDefaultLanguage(TestConsts.CultureEn)
            .Build());

        exception.Message.ShouldBe("Fetching bookmaker details failed.");
        exception.InnerException.ShouldNotBeNull();
    }

    [Fact]
    public void BuildWhenProducersFetchFailsWithNonCiamErrorThenRethrowsCommunicationExceptionUnchanged()
    {
        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithStubbedProductionBookmakerDetails(BookmakerDetailsEndpoint.GetValidBookmakerDetails())
                                                   .WithProducersFetcher(DataFetcherMockHelper.GetDataFetcherThrowingCommunicationExceptionOnGetData())
                                                   .BuildTokenSetter();

        var exception = Should.Throw<CommunicationException>(() => tokenSetter
            .SetAccessToken(TestConsts.AnyAccessToken)
            .SelectEnvironment(SdkEnvironment.Production)
            .SetDefaultLanguage(TestConsts.CultureEn)
            .Build());

        exception.Message.ShouldBe("Fetching producers data failed.");
        exception.InnerException.ShouldNotBeNull();
    }

    [Fact]
    public void BuildWhenCommonIamTokenAcquisitionFailsDuringWhoAmiThenThrowsCommunicationExceptionIdentifyingCiam()
    {
        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithBookmakerDetailsFetcher(DataFetcherMockHelper.GetDataFetcherThrowingCommonIamTokenFailureOnGetData())
                                                   .WithOneProducer()
                                                   .BuildTokenSetter();

        var exception = Should.Throw<CommunicationException>(() => tokenSetter
            .SetAccessToken(TestConsts.AnyAccessToken)
            .SelectEnvironment(SdkEnvironment.Production)
            .SetDefaultLanguage(TestConsts.CultureEn)
            .Build());

        exception.Message.ShouldBe(CommonIamFailureMessage);
        exception.ResponseCode.ShouldBe(HttpStatusCode.Unauthorized);
        var innerException = exception.InnerException.ShouldBeOfType<CommunicationException>();
        innerException.ResponseCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public void BuildWhenCommonIamTokenAcquisitionFailsDuringProducersLoadThenThrowsCommunicationExceptionIdentifyingCiam()
    {
        var tokenSetter = ConfigurationUnitBuilders.StubbingOutDataProviders()
                                                   .WithStubbedProductionBookmakerDetails(BookmakerDetailsEndpoint.GetValidBookmakerDetails())
                                                   .WithProducersFetcher(DataFetcherMockHelper.GetDataFetcherThrowingCommonIamTokenFailureOnGetDataForProducers())
                                                   .BuildTokenSetter();

        var exception = Should.Throw<CommunicationException>(() => tokenSetter
            .SetAccessToken(TestConsts.AnyAccessToken)
            .SelectEnvironment(SdkEnvironment.Production)
            .SetDefaultLanguage(TestConsts.CultureEn)
            .Build());

        exception.Message.ShouldBe(CommonIamFailureMessage);
        exception.ResponseCode.ShouldBe(HttpStatusCode.Unauthorized);
        var innerException = exception.InnerException.ShouldBeOfType<CommunicationException>();
        innerException.ResponseCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
