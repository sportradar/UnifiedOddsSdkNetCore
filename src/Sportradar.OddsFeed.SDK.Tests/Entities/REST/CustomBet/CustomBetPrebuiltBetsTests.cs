// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.CustomBet;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions.Assertions;
using Xunit;
using static Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.CustomBet.CustomBetEndpoint;
using static Sportradar.OddsFeed.SDK.Tests.Common.Helpers.DataFetcherMockHelper;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.CustomBet;

public class CustomBetPrebuiltBetsTests
{
    [Fact]
    public async Task GetPrebuiltBetsThrowErrorWhenFetcherFails()
    {
        var dataFetcher = GetDataFetcherThrowingCommunicationExceptionAsync();
        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var getPrebuiltBets = () => customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        await getPrebuiltBets.Should().ThrowAsync<CommunicationException>();
    }

    [Fact]
    public async Task GetPrebuiltBetsThrowErrorWhenFetcherThrowsGenericException()
    {
        var dataFetcher = GetDataFetcherThrowingExceptionAsync();
        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var getPrebuiltBets = () => customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        await getPrebuiltBets.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetPrebuiltBetsReturnsNullWhenFetcherFailsAndExceptionHandlingStrategyIsCatch()
    {
        var dataFetcher = GetDataFetcherThrowingCommunicationExceptionAsync();
        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyCatch()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.Should().BeNull();
    }

    [Fact]
    public async Task GetPrebuiltBetsReturnsNullWhenFetcherThrowsGenericExceptionAndExceptionHandlingStrategyIsCatch()
    {
        var dataFetcher = GetDataFetcherThrowingExceptionAsync();
        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyCatch()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.Should().BeNull();
    }

    [Fact]
    public async Task GetPrebuiltBetsCallsEndpointWithoutProvidingEventUrn()
    {
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForQuery(preBuiltBetsApiResponse, query => !query.ContainsKey("event_urn"));
        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBetsRequest.EventId.ShouldBeNull();
        prebuiltBets.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPrebuiltBetsCallsEndpointWithCountFromRequest()
    {
        const int count = 4;
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForQuery(preBuiltBetsApiResponse, query => query["count"] == count.ToString());

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.WithCount(count).Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPrebuiltBetsCallsEndpointWithoutCountWhenCountIsNotProvided()
    {
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForQuery(preBuiltBetsApiResponse, query => !query.ContainsKey("count"));

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBetsRequest.Count.ShouldBeNull();
        prebuiltBets.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPrebuiltBetsCallsEndpointWithEventIdFromRequest()
    {
        const string sportEventId = "sr:match:1";
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForQuery(preBuiltBetsApiResponse, query => query["event_urn"] == sportEventId);

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.WithEvent(sportEventId.ToUrn()).Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPrebuiltBetsCallsEndpointWithLengthFromRequest()
    {
        const int length = 4;
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForQuery(preBuiltBetsApiResponse, query => query["length"] == length.ToString());

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.WithLength(length).Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPrebuiltBetsCallsEndpointWithoutLengthWhenLengthIsNotProvided()
    {
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForQuery(preBuiltBetsApiResponse, query => !query.ContainsKey("length"));

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBetsRequest.Length.ShouldBeNull();
        prebuiltBets.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPrebuiltBetsCallsEndpointWithUserFromRequest()
    {
        const string user = "usr123";
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForQuery(preBuiltBetsApiResponse, query => query["user"] == user);

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.WithUser(user).Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPrebuiltBetsCallsEndpointWithoutUserWhenUserIsNotProvided()
    {
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForQuery(preBuiltBetsApiResponse, query => !query.ContainsKey("user"));

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBetsRequest.User.ShouldBeNull();
        prebuiltBets.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPrebuiltBetsCallsEndpointWithSubBookmakerHeaderFromRequest()
    {
        const int subBookmakerId = 1;
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForHeaders(preBuiltBetsApiResponse, headers => headers["x-sub-bookmaker"] == subBookmakerId.ToString());

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.WithSubBookmakerId(subBookmakerId).Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPrebuiltBetsTracksLatencyUnderDataRouterManagerHistogramWithCustomBetPrebuiltEndpointTag()
    {
        const string telemetryEndpointTag = "endpoint";
        const string telemetryEndpointTagValue = "CustomBetPrebuilt";

        var exportedMetrics = new List<Metric>();
        using var meterProvider = Sdk.CreateMeterProviderBuilder()
                                     .AddMeter(UofSdkTelemetry.ServiceName)
                                     .AddInMemoryExporter(exportedMetrics)
                                     .Build();

        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForAnyRequest(preBuiltBetsApiResponse);

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        meterProvider.ForceFlush().ShouldBeTrue();

        var dataRouterManagerMetric = GetCustombetPrebuiltBetsMetricFrom(exportedMetrics);

        dataRouterManagerMetric.ShouldNotBeNull();
        dataRouterManagerMetric.ShouldHaveTag(telemetryEndpointTag, telemetryEndpointTagValue);
    }

    [Fact]
    public async Task GetPrebuiltBetsReturnsRequestedRecommendations()
    {
        const int requestedRecommendations = 5;
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        preBuiltBetsApiResponse.requested_recommendations = requestedRecommendations;

        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForAnyRequest(preBuiltBetsApiResponse);

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.RequestedRecommendations.Should().Be(preBuiltBetsApiResponse.requested_recommendations);
    }

    [Fact]
    public async Task GetPrebuiltBetsReturnsGeneratedAt()
    {
        const string generatedAt = "2026-03-12T10:00:00Z";
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        preBuiltBetsApiResponse.generated_at = generatedAt;

        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForAnyRequest(preBuiltBetsApiResponse);

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.GeneratedAt.Should().Be(SdkInfo.ParseDate(generatedAt));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetPrebuiltBetsReturnsNullGeneratedAtWhenMissing(string generatedAt)
    {
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        preBuiltBetsApiResponse.generated_at = generatedAt;

        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForAnyRequest(preBuiltBetsApiResponse);

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.GeneratedAt.Should().Be(null);
    }

    [Fact]
    public async Task GetPrebuiltBetsReturnsStaticNonsenseGeneratedAtWhenValueInvalid()
    {
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        preBuiltBetsApiResponse.generated_at = "invalid date";

        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForAnyRequest(preBuiltBetsApiResponse);

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.GeneratedAt.Should().Be(DateTime.Parse("01/01/0001 00:00:00"));
    }

    [Fact]
    public async Task GetPrebuiltBetsReturnsEmptyEventsWhenSourceEventsAreNull()
    {
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        preBuiltBetsApiResponse.@event = null;

        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForAnyRequest(preBuiltBetsApiResponse);

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.Events.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetPrebuiltBetsReturnsEmptyRecommendationsWhenSourceRecommendationsAreNull()
    {
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        preBuiltBetsApiResponse.@event[0].recommendations = null;

        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForAnyRequest(preBuiltBetsApiResponse);

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.Events.ShouldBeOfSize(preBuiltBetsApiResponse.@event.Length);
        prebuiltBets.Events[0].Recommendations.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetPrebuiltBetsMapsEventId()
    {
        const string sourceEventId = "sr:match:2000";
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        preBuiltBetsApiResponse.@event[0].id = sourceEventId;

        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForAnyRequest(preBuiltBetsApiResponse);

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.Events.ShouldBeOfSize(preBuiltBetsApiResponse.@event.Length);
        prebuiltBets.Events[0].EventId.Should().Be(sourceEventId.ToUrn());
    }

    [Fact]
    public async Task GetPrebuiltBetsMapsProvidedRecommendations()
    {
        const int sourceProvidedRecommendations = 3;
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        preBuiltBetsApiResponse.@event[0].provided_recommendation = sourceProvidedRecommendations;

        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForAnyRequest(preBuiltBetsApiResponse);

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.Events.ShouldBeOfSize(preBuiltBetsApiResponse.@event.Length);
        prebuiltBets.Events[0].ProvidedRecommendations.Should().Be(sourceProvidedRecommendations);
    }

    [Fact]
    public async Task GetPrebuiltBetsMapsSource()
    {
        const string sourceValue = "test-source";
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        preBuiltBetsApiResponse.@event[0].source = sourceValue;

        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForAnyRequest(preBuiltBetsApiResponse);

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.Events.ShouldBeOfSize(preBuiltBetsApiResponse.@event.Length);
        prebuiltBets.Events[0].Source.Should().Be(sourceValue);
    }

    [Fact]
    public async Task GetPrebuiltBetsMapsRecommendationOdds()
    {
        const double firstOdds = 2.35;
        const double secondOdds = 3.35;
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        preBuiltBetsApiResponse.@event[0].recommendations[0].odds = firstOdds;
        preBuiltBetsApiResponse.@event[0].recommendations[1].odds = secondOdds;

        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForAnyRequest(preBuiltBetsApiResponse);

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.Events.ShouldBeOfSize(preBuiltBetsApiResponse.@event.Length);
        prebuiltBets.Events[0].Recommendations.ShouldBeOfSize(preBuiltBetsApiResponse.@event[0].recommendations.Length);
        prebuiltBets.Events[0].Recommendations[0].Odds.Should().Be(firstOdds);
        prebuiltBets.Events[0].Recommendations[1].Odds.Should().Be(secondOdds);
    }

    [Fact]
    public async Task GetPrebuiltBetsMapsRecommendationProbability()
    {
        const double firstSourceProbability = 0.25;
        const double secondSourceProbability = 0.35;
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        preBuiltBetsApiResponse.@event[0].recommendations[0].probability = firstSourceProbability;
        preBuiltBetsApiResponse.@event[0].recommendations[1].probability = secondSourceProbability;

        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForAnyRequest(preBuiltBetsApiResponse);

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.Events.ShouldBeOfSize(preBuiltBetsApiResponse.@event.Length);
        prebuiltBets.Events[0].Recommendations.ShouldBeOfSize(preBuiltBetsApiResponse.@event[0].recommendations.Length);
        prebuiltBets.Events[0].Recommendations[0].Probability.Should().Be(firstSourceProbability);
        prebuiltBets.Events[0].Recommendations[1].Probability.Should().Be(secondSourceProbability);
    }

    [Fact]
    public async Task GetPrebuiltBetsReturnsEmptySelectionsWhenSourceSelectionsAreNull()
    {
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        preBuiltBetsApiResponse.@event[0].recommendations[0].selection = null;

        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForAnyRequest(preBuiltBetsApiResponse);

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.Events.ShouldBeOfSize(preBuiltBetsApiResponse.@event.Length);
        prebuiltBets.Events[0].Recommendations[0].Selections.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetPrebuiltBetsMapsSelectionMarketId()
    {
        const int firstSourceMarketId = 201;
        const int secondSourceMarketId = 202;
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        preBuiltBetsApiResponse.@event[0].recommendations[0].selection[0].market_id = firstSourceMarketId;
        preBuiltBetsApiResponse.@event[0].recommendations[0].selection[1].market_id = secondSourceMarketId;

        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForAnyRequest(preBuiltBetsApiResponse);

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.Events.ShouldBeOfSize(preBuiltBetsApiResponse.@event.Length);
        prebuiltBets.Events[0].Recommendations[0].Selections[0].MarketId.Should().Be(firstSourceMarketId);
        prebuiltBets.Events[0].Recommendations[0].Selections[1].MarketId.Should().Be(secondSourceMarketId);
    }

    [Fact]
    public async Task GetPrebuiltBetsMapsSelectionOutcomeId()
    {
        const string firstSourceOutcomeId = "12";
        const string secondSourceOutcomeId = "13";
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        preBuiltBetsApiResponse.@event[0].recommendations[0].selection[0].outcome_id = firstSourceOutcomeId;
        preBuiltBetsApiResponse.@event[0].recommendations[0].selection[1].outcome_id = secondSourceOutcomeId;

        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForAnyRequest(preBuiltBetsApiResponse);

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.Events.ShouldBeOfSize(preBuiltBetsApiResponse.@event.Length);
        prebuiltBets.Events[0].Recommendations[0].Selections[0].OutcomeId.Should().Be(firstSourceOutcomeId);
        prebuiltBets.Events[0].Recommendations[0].Selections[1].OutcomeId.Should().Be(secondSourceOutcomeId);
    }

    [Fact]
    public async Task GetPrebuiltBetsMapsSelectionSpecifiers()
    {
        const string firstSourceSpecifiers = "variant=sr:exact_goals:bestof:3";
        const string secondSourceSpecifiers = "variant=sr:exact_goals:bestof:5";
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        preBuiltBetsApiResponse.@event[0].recommendations[0].selection[0].specifiers = firstSourceSpecifiers;
        preBuiltBetsApiResponse.@event[0].recommendations[0].selection[1].specifiers = secondSourceSpecifiers;

        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForAnyRequest(preBuiltBetsApiResponse);

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .WithDataFetcher(dataFetcher)
                                                      .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.Events.ShouldBeOfSize(preBuiltBetsApiResponse.@event.Length);
        prebuiltBets.Events[0].Recommendations[0].Selections[0].Specifiers.Should().Be(firstSourceSpecifiers);
        prebuiltBets.Events[0].Recommendations[0].Selections[1].Specifiers.Should().Be(secondSourceSpecifiers);
    }

    [Fact]
    public void PrebuiltBetsRequestCannotBeConfiguredWithNullEventId()
    {
        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                      .AddDefaultDependencies()
                                                      .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                      .Build();

        var buildRequestAction = () => customBetManager.PrebuiltBetsRequestBuilder.WithEvent(null);

        buildRequestAction.Should().Throw<ArgumentNullException>()
                          .Which.Message.Contains("eventId", StringComparison.Ordinal);
    }

    [Fact]
    public async Task PrebuiltBetsRequestDefaultSubBookmakerIsBookmakerId()
    {
        const int bookmakerId = 1;
        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForHeaders(
            preBuiltBetsApiResponse,
            headers => headers["x-sub-bookmaker"] == bookmakerId.ToString());

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
            .AddDefaultDependencies()
            .WithConfigurationProviding(bookmakerId, ExceptionHandlingStrategy.Throw)
            .WithDataFetcher(dataFetcher)
            .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.Should().NotBeNull();
    }

    [Fact]
    public async Task PrebuiltBetsRequestSubBookmakerCanBeOverriden()
    {
        const int bookmakerIdToBeOverwritten = 1;
        const int subBookmakerId = 123;

        var preBuiltBetsApiResponse = GetFullyPopulatedPrebuiltBetsType();
        var dataFetcher = GetDataFetcherProvidingPrebuiltBetsForHeaders(
            preBuiltBetsApiResponse,
            headers => headers["x-sub-bookmaker"] == subBookmakerId.ToString());

        var customBetManager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
            .AddDefaultDependencies()
            .WithConfigurationProviding(bookmakerIdToBeOverwritten, ExceptionHandlingStrategy.Throw)
            .WithDataFetcher(dataFetcher)
            .Build();

        var prebuiltBetsRequest = customBetManager.PrebuiltBetsRequestBuilder.WithSubBookmakerId(subBookmakerId).Build();

        var prebuiltBets = await customBetManager.GetPrebuiltBets(prebuiltBetsRequest);

        prebuiltBets.Should().NotBeNull();
    }

    private static Metric GetCustombetPrebuiltBetsMetricFrom(List<Metric> exportedMetrics)
    {
        return exportedMetrics.SingleOrDefault(x => x.Name == UofSdkTelemetry.DataRouterManager.Name && x.MetricType == MetricType.Histogram);
    }
}
