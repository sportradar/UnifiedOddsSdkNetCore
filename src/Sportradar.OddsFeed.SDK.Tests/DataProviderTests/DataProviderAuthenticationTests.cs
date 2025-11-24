// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Authentication;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;
using static Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Soccer;

namespace Sportradar.OddsFeed.SDK.Tests.DataProviderTests;

public class DataProviderAuthenticationTests : IAsyncLifetime
{
    private static readonly CultureInfo English = TestConsts.CultureEn;
    private WireMockServer _wireMockServer;
    private Uri _serverUri;
    private const string TokenForApi = "token-for-api";
    private const int HttpTimeoutInSeconds = 10;
    private const int MaxConnections = 100;

    public Task InitializeAsync()
    {
        _wireMockServer = WireMockServer.Start();
        _serverUri = new Uri(_wireMockServer.Url!);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _wireMockServer?.Stop();
        _wireMockServer?.Dispose();
        return Task.CompletedTask;
    }

    [Theory]
    [InlineData(UofSdkBootstrap.TimeCriticalDataProviderServiceKey)]
    [InlineData(UofSdkBootstrap.NonTimeCriticalDataProviderServiceKey)]
    public async Task GetDataAsyncWhenClientAuthConfiguredAndTokenAvailableThenSendsRequestWithCommonIamToken(string dataProviderKey)
    {
        var matchSummaryEndpoint = Summary();

        StubWireMockToReturnSummary(matchSummaryEndpoint, English.TwoLetterISOLanguageName, TokenForApi);

        var configuration = GetConfigurationForWiremock(_wireMockServer, HttpTimeoutInSeconds, MaxConnections);
        var authenticationTokenCacheMock = new Mock<IAuthenticationTokenCache>();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddUofSdkServices(configuration);
        serviceCollection.AddSingleton(authenticationTokenCacheMock.Object);

        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        authenticationTokenCacheMock.Setup(auth => auth.GetTokenForApi()).ReturnsAsync(TokenForApi);
        var dataProvider = scope.ServiceProvider.GetRequiredKeyedService<IDataProvider<SportEventSummaryDto>>(dataProviderKey);

        var act = () => dataProvider.GetDataAsync(matchSummaryEndpoint.sport_event.id, English.TwoLetterISOLanguageName);

        await act.ShouldNotThrowAsync();
    }

    [Theory]
    [InlineData(UofSdkBootstrap.TimeCriticalDataProviderServiceKey)]
    [InlineData(UofSdkBootstrap.NonTimeCriticalDataProviderServiceKey)]
    public async Task GetDataAsyncWhenRequestFailsOnceAndRespondsWithCorrectStatusThenRetriesRequest(string dataProviderKey)
    {
        var matchSummaryEndpoint = Summary();
        var path = $"/v1/sports/{English.TwoLetterISOLanguageName}/sport_events/{matchSummaryEndpoint.sport_event.id}/summary.xml";

        const string tokenFor401 = "old-token";
        const string tokenFor200 = "new-token";
        StubApiToReturn401Then200WithSummary(path, matchSummaryEndpoint, tokenFor401, tokenFor200);

        var configuration = GetConfigurationForWiremock(_wireMockServer, HttpTimeoutInSeconds, MaxConnections);
        var authenticationTokenCacheMock = new Mock<IAuthenticationTokenCache>();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddUofSdkServices(configuration);
        serviceCollection.AddSingleton(authenticationTokenCacheMock.Object);

        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        authenticationTokenCacheMock.SetupSequence(auth => auth.GetTokenForApi())
                                    .ReturnsAsync(tokenFor401)
                                    .ReturnsAsync(tokenFor200);
        authenticationTokenCacheMock
           .Setup(auth => auth.RefreshApiTokenAsync(It.IsAny<string>()))
           .Returns(Task.CompletedTask);

        var dataProvider = scope.ServiceProvider.GetRequiredKeyedService<IDataProvider<SportEventSummaryDto>>(dataProviderKey);

        await Should.NotThrowAsync(async () => await dataProvider.GetDataAsync(matchSummaryEndpoint.sport_event.id, English.TwoLetterISOLanguageName));

        var requests = _wireMockServer.LogEntries;
        requests.ShouldBeOfSize(2);
        requests[0].RequestMessage.Headers.ShouldNotBeNull();
        requests[1].RequestMessage.Headers.ShouldNotBeNull();
        requests[0].RequestMessage.Headers["Authorization"].First().ShouldBe($"Bearer {tokenFor401}");
        requests[1].RequestMessage.Headers["Authorization"].First().ShouldBe($"Bearer {tokenFor200}");
    }

    [Theory]
    [InlineData(UofSdkBootstrap.TimeCriticalDataProviderServiceKey)]
    [InlineData(UofSdkBootstrap.NonTimeCriticalDataProviderServiceKey)]
    public async Task GetDataAsyncWhenRequestFailsMultipleTimesThenFailsAfterOneRetry(string dataProviderKey)
    {
        var matchSummaryEndpoint = Summary();
        var path = $"/v1/sports/{English.TwoLetterISOLanguageName}/sport_events/{matchSummaryEndpoint.sport_event.id}/summary.xml";

        const string tokenFor401 = "token-for-401";
        StubWireMockToReturn401(path, tokenFor401);

        var configuration = GetConfigurationForWiremock(_wireMockServer, HttpTimeoutInSeconds, MaxConnections);
        var authenticationTokenCacheMock = new Mock<IAuthenticationTokenCache>();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddUofSdkServices(configuration);
        serviceCollection.AddSingleton(authenticationTokenCacheMock.Object);

        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        authenticationTokenCacheMock.SetupSequence(auth => auth.GetTokenForApi())
                                    .ReturnsAsync(tokenFor401)
                                    .ReturnsAsync(tokenFor401);
        authenticationTokenCacheMock
           .Setup(auth => auth.RefreshApiTokenAsync(It.IsAny<string>()))
           .Returns(Task.CompletedTask);

        var dataProvider = scope.ServiceProvider.GetRequiredKeyedService<IDataProvider<SportEventSummaryDto>>(dataProviderKey);

        await Should.ThrowAsync<CommunicationException>(async () => await dataProvider.GetDataAsync(matchSummaryEndpoint.sport_event.id, English.TwoLetterISOLanguageName));

        var requests = _wireMockServer.LogEntries;
        requests.ShouldBeOfSize(2);
        requests.ShouldAllBe(log => log.RequestMessage.Headers != null && log.RequestMessage.Headers["Authorization"].First() == $"Bearer {tokenFor401}");
    }

    [Theory]
    [InlineData(UofSdkBootstrap.TimeCriticalDataProviderServiceKey)]
    [InlineData(UofSdkBootstrap.NonTimeCriticalDataProviderServiceKey)]
    public async Task GetDataAsyncWhenClientAuthenticationIsNotConfiguredAndApiReturns401ThenDoesNotTriggerRetry(string dataProviderKey)
    {
        const string xAccessToken = "token";
        var matchSummaryEndpoint = Summary();
        var sportSummaryPath = $"/v1/sports/{English.TwoLetterISOLanguageName}/sport_events/{matchSummaryEndpoint.sport_event.id}/summary.xml";

        StubWireMockToReturn401Then200WithSummary(sportSummaryPath, matchSummaryEndpoint, xAccessToken);

        var configuration = GetValidConfigurationWithoutCiamAuthenticationFor(_serverUri, xAccessToken, HttpTimeoutInSeconds, MaxConnections);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddUofSdkServices(configuration);
        var authenticationTokenCacheMock = new Mock<IAuthenticationTokenCache>();
        serviceCollection.AddSingleton(authenticationTokenCacheMock.Object);

        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var dataProvider = scope.ServiceProvider.GetRequiredKeyedService<IDataProvider<SportEventSummaryDto>>(dataProviderKey);

        await Should.ThrowAsync<CommunicationException>(async () => await dataProvider.GetDataAsync(matchSummaryEndpoint.sport_event.id, English.TwoLetterISOLanguageName));
    }

    private void StubApiToReturn401Then200WithSummary(string path, matchSummaryEndpoint matchSummaryEndpoint, string tokenFor401, string tokenFor200)
    {
        _wireMockServer.Given(Request.Create()
                                     .UsingGet()
                                     .WithPath(path)
                       .WithHeader("Authorization", $"Bearer {tokenFor401}"))
                       .InScenario("RetryOn401")
                       .WillSetStateTo("SecondAttempt")
                       .RespondWith(Response.Create()
                                            .WithStatusCode(401));

        _wireMockServer.Given(Request.Create()
                                     .UsingGet()
                                     .WithPath(path)
                       .WithHeader("Authorization", $"Bearer {tokenFor200}"))
                       .InScenario("RetryOn401")
                       .WhenStateIs("SecondAttempt")
                       .RespondWith(Response.Create()
                                            .WithStatusCode(200)
                                            .WithHeader("Content-Type", "application/xml")
                                            .WithBody(MsgSerializer.SerializeToXml(matchSummaryEndpoint)));
    }

    private void StubWireMockToReturn401(string path, string tokenFor401)
    {
        _wireMockServer.Given(Request.Create()
                                     .UsingGet()
                                     .WithPath(path)
                                     .WithHeader("Authorization", $"Bearer {tokenFor401}"))
                       .RespondWith(Response.Create()
                                            .WithStatusCode(401));
    }

    private void StubWireMockToReturn401Then200WithSummary(string path, matchSummaryEndpoint matchSummaryEndpoint, string xAccessToken)
    {
        _wireMockServer.Given(Request.Create()
                                     .UsingGet()
                                     .WithPath(path)
                                     .WithHeader("x-access-token", xAccessToken))
                       .InScenario("RetryOn401")
                       .WillSetStateTo("SecondAttempt")
                       .RespondWith(Response.Create()
                                            .WithStatusCode(401));

        _wireMockServer.Given(Request.Create()
                                     .UsingGet()
                                     .WithPath(path)
                                     .WithHeader("x-access-token", xAccessToken))
                       .InScenario("RetryOn401")
                       .WhenStateIs("SecondAttempt")
                       .RespondWith(Response.Create()
                                            .WithStatusCode(200)
                                            .WithHeader("Content-Type", "application/xml")
                                            .WithBody(MsgSerializer.SerializeToXml(matchSummaryEndpoint)));
    }

    private void StubWireMockToReturnSummary(matchSummaryEndpoint matchSummary, string language, string commonIamToken)
    {
        _wireMockServer.Given(Request.Create()
                                     .UsingGet()
                                     .WithPath($"/v1/sports/{language}/sport_events/{matchSummary.sport_event.id}/summary.xml")
                                     .WithHeader("Authorization", $"Bearer {commonIamToken}"))
                       .RespondWith(Response.Create()
                                            .WithStatusCode(200)
                                            .WithHeader("Content-Type", "application/xml")
                                            .WithBody(MsgSerializer.SerializeToXml(matchSummary)));
    }

    private static IUofConfiguration GetConfigurationForWiremock(WireMockServer wireMockServer, int httpTimeoutInSeconds, int maxConnections)
    {
        var mockConfiguration = new Mock<IUofConfiguration>();
        var mockApiConfiguration = new Mock<IUofApiConfiguration>();

        var tenSeconds = TimeSpan.FromSeconds(httpTimeoutInSeconds);
        mockApiConfiguration.SetupGet(x => x.Host).Returns(new Uri(wireMockServer.Url!).Host);
        mockApiConfiguration.SetupGet(x => x.BaseUrl).Returns(wireMockServer.Url);
        mockApiConfiguration.SetupGet(x => x.UseSsl).Returns(false);
        mockApiConfiguration.SetupGet(x => x.HttpClientTimeout).Returns(tenSeconds);
        mockApiConfiguration.SetupGet(x => x.HttpClientRecoveryTimeout).Returns(tenSeconds);
        mockApiConfiguration.SetupGet(x => x.HttpClientFastFailingTimeout).Returns(tenSeconds);
        mockApiConfiguration.SetupGet(x => x.MaxConnectionsPerServer).Returns(maxConnections);

        var cache = new Mock<IUofCacheConfiguration>();
        var auth = new Mock<UofClientAuthentication.IPrivateKeyJwt>();

        mockConfiguration.SetupGet(x => x.Api).Returns(mockApiConfiguration.Object);
        mockConfiguration.SetupGet(x => x.Cache).Returns(cache.Object);
        mockConfiguration.SetupGet(x => x.Authentication).Returns(auth.Object);

        return mockConfiguration.Object;
    }

    private static IUofConfiguration GetValidConfigurationWithoutCiamAuthenticationFor(Uri serverUri, string xAccessToken, int httpTimeoutInSeconds, int maxConnections)
    {
        var mockConfiguration = new Mock<IUofConfiguration>();
        var mockApiConfiguration = new Mock<IUofApiConfiguration>();

        var timeout = TimeSpan.FromSeconds(httpTimeoutInSeconds);
        mockApiConfiguration.SetupGet(x => x.Host).Returns($"{serverUri.Host}:{serverUri.Port}");
        mockApiConfiguration.SetupGet(x => x.BaseUrl).Returns(serverUri.ToString());
        mockApiConfiguration.SetupGet(x => x.UseSsl).Returns(false);
        mockApiConfiguration.SetupGet(x => x.HttpClientTimeout).Returns(timeout);
        mockApiConfiguration.SetupGet(x => x.HttpClientFastFailingTimeout).Returns(timeout);
        mockApiConfiguration.SetupGet(x => x.MaxConnectionsPerServer).Returns(maxConnections);

        var cache = new Mock<IUofCacheConfiguration>();
        var auth = new Mock<UofClientAuthentication.IPrivateKeyJwt>();

        mockConfiguration.SetupGet(x => x.Api).Returns(mockApiConfiguration.Object);
        mockConfiguration.SetupGet(x => x.Cache).Returns(cache.Object);
        mockConfiguration.SetupGet(x => x.Authentication).Returns((UofClientAuthentication.IPrivateKeyJwt)null);
        mockConfiguration.SetupGet(x => x.AccessToken).Returns(xAccessToken);

        return mockConfiguration.Object;
    }
}
