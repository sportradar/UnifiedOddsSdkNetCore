// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Configuration;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;
using Sportradar.OddsFeed.SDK.Tests.Common.Helpers;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;

internal static class ConfigurationUnitBuilderExtensions
{
    public static ConfigurationUnitBuilder WithStubbedProductionBookmakerDetails(this ConfigurationUnitBuilder builder, bookmaker_details bookmakerDetails)
    {
        var apiHost = EnvironmentManager.GetApiHost(SdkEnvironment.Production);
        return GetFetcherProvidingBookmakerDetailsOnlyIfApiHostMatches(builder, bookmakerDetails, apiHost);
    }

    public static ConfigurationUnitBuilder WithStubbedIntegrationBookmakerDetails(this ConfigurationUnitBuilder builder, bookmaker_details bookmakerDetails)
    {
        var apiHost = EnvironmentManager.GetApiHost(SdkEnvironment.Integration);
        return GetFetcherProvidingBookmakerDetailsOnlyIfApiHostMatches(builder, bookmakerDetails, apiHost);
    }

    public static ConfigurationUnitBuilder WithFailingBookmakerDetailsFor(this ConfigurationUnitBuilder builder, SdkEnvironment environment)
    {
        var apiHost = EnvironmentManager.GetApiHost(environment);
        return GetFailingFetcher(builder, apiHost);
    }

    public static ConfigurationUnitBuilder WithFailingFirstThenProvidingBookmakerDetails(this ConfigurationUnitBuilder builder, bookmaker_details bookmakerDetails)
    {
        return GetFetcherFailingOnceThenProvidingValue(builder, bookmakerDetails);
    }

    public static ConfigurationUnitBuilder WithOneProducer(this ConfigurationUnitBuilder builder)
    {
        return SetOneProducerFetcher(builder);
    }

    public static ConfigurationUnitBuilder WithProducerFailingWIthTimeoutException(this ConfigurationUnitBuilder builder)
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

    private static ConfigurationUnitBuilder GetFailingFetcher(ConfigurationUnitBuilder builder, string apiHost)
    {
        var dataFetcher = DataFetcherMockHelper.GetDataFetcherThrowingCommunicationExceptionIfHostMatches(apiHost);
        return builder.WithBookmakerDetailsFetcher(dataFetcher);
    }

    private static ConfigurationUnitBuilder GetFetcherFailingOnceThenProvidingValue(ConfigurationUnitBuilder builder, bookmaker_details bookmakerDetails)
    {
        var dataFetcher = DataFetcherMockHelper.GetDataFetcherThrowingExceptionThenProvidingData(bookmakerDetails);
        return builder.WithBookmakerDetailsFetcher(dataFetcher);
    }
}
