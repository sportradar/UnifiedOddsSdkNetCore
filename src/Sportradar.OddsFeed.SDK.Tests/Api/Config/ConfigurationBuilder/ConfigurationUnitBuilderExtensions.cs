// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;
using Sportradar.OddsFeed.SDK.Tests.Common.Helpers;

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

internal static class ConfigurationUnitBuilderExtensions
{
    public static ConfigurationUnitBuilder WithStubbedProductionBookmakerDetails(this ConfigurationUnitBuilder builder, bookmaker_details bookmakerDetails)
    {
        return builder.WithStubbedProductionBookmakerDetailsAndIntegrationFailure(bookmakerDetails);
    }

    public static ConfigurationUnitBuilder WithStubbedIntegrationBookmakerDetailsAndProductionFailure(this ConfigurationUnitBuilder builder, bookmaker_details bookmakerDetails)
    {
        var apiHost = EnvironmentManager.GetApiHost(SdkEnvironment.Integration);
        return GetFetcherProvidingBookmakerDetailsOnlyIfApiHostMatches(builder, bookmakerDetails, apiHost);
    }

    public static ConfigurationUnitBuilder WithStubbedProductionBookmakerDetailsAndIntegrationFailure(this ConfigurationUnitBuilder builder, bookmaker_details bookmakerDetails)
    {
        var apiHost = EnvironmentManager.GetApiHost(SdkEnvironment.Production);
        return GetFetcherProvidingBookmakerDetailsOnlyIfApiHostMatches(builder, bookmakerDetails, apiHost);
    }

    public static ConfigurationUnitBuilder WithStubbedIntegrationAndProductionBookmakerDetails(
        this ConfigurationUnitBuilder builder,
        bookmaker_details integrationBookmakerDetails,
        bookmaker_details productionBookmakerDetails)
    {
        var integrationHost = EnvironmentManager.GetApiHost(SdkEnvironment.Integration);
        var productionHost = EnvironmentManager.GetApiHost(SdkEnvironment.Production);
        var dataFetcher = DataFetcherMockHelper.GetDataFetcherProvidingBookmakerDetailsForBothHosts(
            integrationBookmakerDetails, integrationHost,
            productionBookmakerDetails, productionHost);

        return builder.WithBookmakerDetailsFetcher(dataFetcher);
    }

    public static ConfigurationUnitBuilder WithOneProducer(this ConfigurationUnitBuilder builder)
    {
        return SetOneProducerFetcher(builder);
    }

    public static ConfigurationUnitBuilder WithOneProducerAndAnyBookmakerDetails(this ConfigurationUnitBuilder builder)
    {
        return SetOneProducerFetcher(SetAnyBookmakerDetailsFetcher(builder));
    }

    private static ConfigurationUnitBuilder SetAnyBookmakerDetailsFetcher(ConfigurationUnitBuilder builder)
    {
        return builder.WithBookmakerDetailsFetcher(
            DataFetcherMockHelper.GetDataFetcherProvidingBookmakerDetails(BookmakerDetailsEndpoint.GetValidBookmakerDetails()));
    }

    private static ConfigurationUnitBuilder SetOneProducerFetcher(ConfigurationUnitBuilder builder)
    {
        return builder.WithProducersFetcher(
            DataFetcherMockHelper.GetDataFetcherProvidingProducers(ProducersEndpoint.GetOneProducer()));
    }

    private static ConfigurationUnitBuilder GetFetcherProvidingBookmakerDetailsOnlyIfApiHostMatches(ConfigurationUnitBuilder builder, bookmaker_details bookmakerDetails, string apiHost)
    {
        var dataFetcher = DataFetcherMockHelper.GetDataFetcherProvidingBookmakerDetailsIfHostMatches(bookmakerDetails, apiHost);
        return builder.WithBookmakerDetailsFetcher(dataFetcher);
    }
}
