// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;
using static Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Soccer;

namespace Sportradar.OddsFeed.SDK.Tests.DataProviderTests;

public class DataProviderReplayTests : IAsyncLifetime
{
    private const string Language = "en";

    private WireMockServer _wireMockServer;

    public Task InitializeAsync()
    {
        _wireMockServer = WireMockServer.Start();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _wireMockServer?.Stop();
        _wireMockServer?.Dispose();
        return Task.CompletedTask;
    }

    public static IEnumerable<object[]> ReplayEnvironments()
    {
        yield return [SdkEnvironment.ReplayWithIntegrationCredentials];
        yield return [SdkEnvironment.ReplayWithProductionCredentials];
    }

    public static IEnumerable<object[]> NonReplayEnvironments()
    {
        yield return [SdkEnvironment.Integration];
        yield return [SdkEnvironment.Production];
        yield return [SdkEnvironment.GlobalIntegration];
        yield return [SdkEnvironment.GlobalProduction];
    }

    [Theory]
    [MemberData(nameof(ReplayEnvironments))]
    public async Task FixtureDataReturnedFromReplayServerWhenEnvironmentSetToReplay(SdkEnvironment environment)
    {
        var fixture = Fixture();
        var uofConfiguration = BuildConfigurationFor(environment);
        StubApiToReturnFixtureForReplay(uofConfiguration.Api.ReplayBaseUrl, fixture);
        await using var serviceProvider = BuildServiceProviderWith(uofConfiguration);
        var dataProvider = serviceProvider.GetRequiredService<IDataProvider<FixtureDto>>();

        var result = await dataProvider.GetDataAsync(fixture.fixture.id, Language);

        result.ShouldNotBeNull();
    }

    [Theory]
    [MemberData(nameof(NonReplayEnvironments))]
    public async Task FixtureDataProviderCallsUrlWithoutReplayWhenEnvironmentIsNotReplay(SdkEnvironment environment)
    {
        var fixture = Fixture();
        var uofConfiguration = BuildConfigurationFor(environment);
        await using var serviceProvider = BuildServiceProviderWith(uofConfiguration);
        var dataProvider = serviceProvider.GetRequiredService<IDataProvider<FixtureDto>>();
        StubApiToReturnFixtureForNonReplay(uofConfiguration.Api.ReplayBaseUrl, fixture);

        var result = await dataProvider.GetDataAsync(fixture.fixture.id, Language);

        result.ShouldNotBeNull();
    }

    [Theory]
    [MemberData(nameof(ReplayEnvironments))]
    public async Task FixtureChangeFixtureUsesReplayFixtureEndpointWhenEnvironmentIsReplay(SdkEnvironment environment)
    {
        var fixture = Fixture();
        var uofConfiguration = BuildConfigurationFor(environment);
        await using var serviceProvider = BuildServiceProviderWith(uofConfiguration);
        var dataProvider = serviceProvider.GetRequiredService<IDataProviderNamed<FixtureDto>>();
        StubApiToReturnFixtureChangeForReplay(uofConfiguration.Api.ReplayBaseUrl, fixture);

        var result = await dataProvider.GetDataAsync(fixture.fixture.id, Language);

        result.ShouldNotBeNull();
    }

    [Theory]
    [MemberData(nameof(NonReplayEnvironments))]
    public async Task FixtureChangeFixtureDataProviderCallsExpectedUrlWhenEnvironmentIsNotReplay(SdkEnvironment environment)
    {
        var fixture = Fixture();
        var uofConfiguration = BuildConfigurationFor(environment);
        await using var serviceProvider = BuildServiceProviderWith(uofConfiguration);
        var dataProvider = serviceProvider.GetRequiredService<IDataProviderNamed<FixtureDto>>();
        StubApiToReturnFixtureChangeForNonReplay(uofConfiguration.Api.ReplayBaseUrl, fixture);

        var result = await dataProvider.GetDataAsync(fixture.fixture.id, Language);

        result.ShouldNotBeNull();
    }

    [Theory]
    [MemberData(nameof(ReplayEnvironments))]
    public async Task SportEventSummaryDataProviderCallsExpectedUrlWhenEnvironmentIsReplay(SdkEnvironment environment)
    {
        var summary = Summary();
        var uofConfiguration = BuildConfigurationFor(environment);
        await using var serviceProvider = BuildServiceProviderWith(uofConfiguration);
        var dataProvider = serviceProvider.GetRequiredService<IDataProvider<SportEventSummaryDto>>();
        StubApiToReturnSummaryForReplay(uofConfiguration.Api.ReplayBaseUrl, summary);

        var result = await dataProvider.GetDataAsync(summary.sport_event.id, Language);

        result.ShouldNotBeNull();
    }

    [Theory]
    [MemberData(nameof(NonReplayEnvironments))]
    public async Task SportEventSummaryDataProviderCallsExpectedUrlWhenEnvironmentIsNotReplay(SdkEnvironment environment)
    {
        var summary = Summary();
        var uofConfiguration = BuildConfigurationFor(environment);
        await using var serviceProvider = BuildServiceProviderWith(uofConfiguration);
        var dataProvider = serviceProvider.GetRequiredService<IDataProvider<SportEventSummaryDto>>();
        StubApiToReturnSummaryForNonReplay(uofConfiguration.Api.ReplayBaseUrl, summary);

        var result = await dataProvider.GetDataAsync(summary.sport_event.id, Language);

        result.ShouldNotBeNull();
    }

    [Theory]
    [MemberData(nameof(ReplayEnvironments))]
    public async Task MatchTimelineDataProviderCallsExpectedUrlWhenEnvironmentIsReplay(SdkEnvironment environment)
    {
        var timeline = MatchTimelineEndpoints.LiverpoolVsPsg2025MatchTimeline();
        var uofConfiguration = BuildConfigurationFor(environment);
        await using var serviceProvider = BuildServiceProviderWith(uofConfiguration);
        var dataProvider = serviceProvider.GetRequiredService<IDataProvider<MatchTimelineDto>>();
        StubApiToReturnTimelineForReplay(uofConfiguration.Api.ReplayBaseUrl, timeline);

        var result = await dataProvider.GetDataAsync(timeline.sport_event.id, Language);

        result.ShouldNotBeNull();
    }

    [Theory]
    [MemberData(nameof(NonReplayEnvironments))]
    public async Task MatchTimelineDataProviderCallsExpectedUrlWhenEnvironmentIsNotReplay(SdkEnvironment environment)
    {
        var timeline = MatchTimelineEndpoints.LiverpoolVsPsg2025MatchTimeline();
        var uofConfiguration = BuildConfigurationFor(environment);
        await using var serviceProvider = BuildServiceProviderWith(uofConfiguration);
        var dataProvider = serviceProvider.GetRequiredService<IDataProvider<MatchTimelineDto>>();
        StubApiToReturnTimelineForNonReplay(uofConfiguration.Api.ReplayBaseUrl, timeline);

        var result = await dataProvider.GetDataAsync(timeline.sport_event.id, Language);

        result.ShouldNotBeNull();
    }

    private ServiceProvider BuildServiceProviderWith(IUofConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddUofSdkServices(configuration);
        return services.BuildServiceProvider(true);
    }

    private IUofConfiguration BuildConfigurationFor(SdkEnvironment environment)
    {
        var wireMockHost = GetWireMockHost();
        var baseUrl = $"http://{wireMockHost}";

        return new UofConfigurationBuilder()
            .WithAccessToken(TestConsts.AnyAccessToken)
            .WithEnvironment(environment)
            .WithDefaultLanguage(TestConsts.CultureEn)
            .WithExceptionStrategy(ExceptionHandlingStrategy.Throw)
            .WithApiConfiguration(api => api
                .WithHost(wireMockHost)
                .WithBaseUrl(baseUrl)
                .WithReplayBaseUrl($"{baseUrl}/v1/replay")
                .WithUseSsl(false)
                .WithHttpClientTimeout(TimeSpan.FromSeconds(30))
                .WithHttpClientRecoveryTimeout(TimeSpan.FromSeconds(10))
                .WithHttpClientFastFailingTimeout(TimeSpan.FromSeconds(5))
                .WithMaxConnectionsPerServer(20))
            .Build();
    }

    private string GetWireMockHost()
    {
        var wireMockUri = new Uri(_wireMockServer.Url!);
        return $"{wireMockUri.Host}:{wireMockUri.Port}";
    }

    private void StubApiToReturnFixtureForReplay(string replayBaseUrl, fixturesEndpoint fixture)
    {
        StubApiToReturnResponseForUrlMatching(url => url.Contains(replayBaseUrl) && url.Contains("/fixture.xml", StringComparison.OrdinalIgnoreCase), MsgSerializer.SerializeToXml(fixture));
    }

    private void StubApiToReturnFixtureForNonReplay(string replayBaseUrl, fixturesEndpoint fixture)
    {
        StubApiToReturnResponseForUrlMatching(url => !url.Contains(replayBaseUrl) && url.Contains("/fixture.xml", StringComparison.OrdinalIgnoreCase), MsgSerializer.SerializeToXml(fixture));
    }

    private void StubApiToReturnFixtureChangeForReplay(string replayBaseUrl, fixturesEndpoint fixture)
    {
        // no typo, fixture change used fixture URL for replay environment
        StubApiToReturnResponseForUrlMatching(url => url.Contains(replayBaseUrl) && url.Contains("/fixture.xml", StringComparison.OrdinalIgnoreCase), MsgSerializer.SerializeToXml(fixture));
    }

    private void StubApiToReturnFixtureChangeForNonReplay(string replayBaseUrl, fixturesEndpoint fixture)
    {
        StubApiToReturnResponseForUrlMatching(url => !url.Contains(replayBaseUrl) && url.Contains("/fixture_change_fixture.xml", StringComparison.OrdinalIgnoreCase), MsgSerializer.SerializeToXml(fixture));
    }

    private void StubApiToReturnSummaryForReplay(string replayBaseUrl, matchSummaryEndpoint summary)
    {
        StubApiToReturnResponseForUrlMatching(url => url.Contains(replayBaseUrl) && url.Contains("/summary.xml", StringComparison.OrdinalIgnoreCase), MsgSerializer.SerializeToXml(summary));
    }

    private void StubApiToReturnSummaryForNonReplay(string replayBaseUrl, matchSummaryEndpoint summary)
    {
        StubApiToReturnResponseForUrlMatching(url => !url.Contains(replayBaseUrl) && url.Contains("/summary.xml", StringComparison.OrdinalIgnoreCase), MsgSerializer.SerializeToXml(summary));
    }

    private void StubApiToReturnTimelineForReplay(string replayBaseUrl, matchTimelineEndpoint timeline)
    {
        StubApiToReturnResponseForUrlMatching(url => url.Contains(replayBaseUrl) && url.Contains("/timeline.xml", StringComparison.OrdinalIgnoreCase), MsgSerializer.SerializeToXml(timeline));
    }

    private void StubApiToReturnTimelineForNonReplay(string replayBaseUrl, matchTimelineEndpoint timeline)
    {
        StubApiToReturnResponseForUrlMatching(url => !url.Contains(replayBaseUrl) && url.Contains("/timeline.xml", StringComparison.OrdinalIgnoreCase), MsgSerializer.SerializeToXml(timeline));
    }

    private void StubApiToReturnResponseForUrlMatching(Func<string, bool> urlMatcher, string responseBody)
    {
        _wireMockServer.Given(Request.Create()
                                     .UsingGet()
                                     .WithUrl(urlMatcher))
                       .RespondWith(Response.Create()
                                            .WithStatusCode(StatusCodes.Status200OK)
                                            .WithHeader("Content-Type", "application/xml")
                                            .WithBody(responseBody));
    }
}
