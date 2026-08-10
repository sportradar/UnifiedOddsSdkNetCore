// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Authentication;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Tests.Common;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Authentication;

[Collection(NonParallelCollectionFixture.NonParallelTestCollection)]
public class RetryUnauthorizedDelegatingHandlerTests : IAsyncLifetime
{
    private readonly Mock<IAuthenticationClient> _authenticationClientMock = new();

    private WireMockServer _wireMockServer;
    private Uri _wireMockUri;

    public Task InitializeAsync()
    {
        _wireMockServer = WireMockServer.Start();
        _wireMockUri = new Uri(_wireMockServer.Url!);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _wireMockServer?.Stop();
        _wireMockServer?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetDataAsyncWhenApiRespondsUnauthorizedThenRefreshesTokenAndRetries()
    {
        const string endpoint = "/retry-endpoint";
        const string responseBody = "<response>ok-after-retry</response>";

        SetupAuthClientReturningOldTokenThenNewTokenOnRefresh();
        StubApiEndpointUnauthorizedForOldTokenThenOkForNewToken(endpoint, responseBody);

        await using var provider = CreateProviderUsingUofSdkBootstrap();
        var fetcher = provider.GetRequiredService<IDataFetcher>();

        await using var stream = await fetcher.GetDataAsync(new Uri(_wireMockUri, endpoint));
        using var streamReader = new StreamReader(stream);
        var content = await streamReader.ReadToEndAsync();

        content.ShouldBe(responseBody);
        _wireMockServer.LogEntries.Count(log => log.RequestMessage!.Path == endpoint).ShouldBe(2);
    }

    [Fact]
    public async Task GetDataAsyncWhenApiRespondsOkThenNoRefreshIsTriggered()
    {
        const string endpoint = "/no-retry-endpoint";
        const string responseBody = "<response>ok-first-attempt</response>";

        SetupAuthClientReturningSingleToken();
        StubApiEndpointOkForBearerToken(endpoint, "single-token", responseBody);

        await using var provider = CreateProviderUsingUofSdkBootstrap();
        var fetcher = provider.GetRequiredService<IDataFetcher>();

        await using var stream = await fetcher.GetDataAsync(new Uri(_wireMockUri, endpoint));
        using var streamReader = new StreamReader(stream);
        var content = await streamReader.ReadToEndAsync();

        content.ShouldBe(responseBody);
        _wireMockServer.LogEntries.Count(log => log.RequestMessage!.Path == endpoint).ShouldBe(1);
    }

    private void SetupAuthClientReturningOldTokenThenNewTokenOnRefresh()
    {
        _authenticationClientMock
           .SetupSequence(client => client.RequestAccessTokenAsync(It.IsAny<string>(), AuthenticationTokenCache.UofApiTokenAudience, It.IsAny<CancellationToken>()))
           .ReturnsAsync(CreateTokenResponse("old-token"))
           .ReturnsAsync(CreateTokenResponse("new-token"))
           .ThrowsAsync(new InvalidOperationException("Unexpected extra Common IAM token request"));
    }

    private void SetupAuthClientReturningSingleToken()
    {
        _authenticationClientMock
           .SetupSequence(client => client.RequestAccessTokenAsync(It.IsAny<string>(), AuthenticationTokenCache.UofApiTokenAudience, It.IsAny<CancellationToken>()))
           .ReturnsAsync(CreateTokenResponse("single-token"))
           .ThrowsAsync(new InvalidOperationException("Unexpected extra Common IAM token request"));
    }

    private void StubApiEndpointUnauthorizedForOldTokenThenOkForNewToken(string endpoint, string successResponseBody)
    {
        _wireMockServer
            .Given(Request.Create()
                          .WithPath(endpoint)
                          .UsingGet()
                          .WithHeader("Authorization", "Bearer old-token"))
            .RespondWith(Response.Create().WithStatusCode((int)HttpStatusCode.Unauthorized));

        _wireMockServer
            .Given(Request.Create()
                          .WithPath(endpoint)
                          .UsingGet()
                          .WithHeader("Authorization", "Bearer new-token"))
            .RespondWith(Response.Create().WithStatusCode((int)HttpStatusCode.OK).WithBody(successResponseBody));
    }

    private void StubApiEndpointOkForBearerToken(string endpoint, string bearerToken, string responseBody)
    {
        _wireMockServer
            .Given(Request.Create()
                          .WithPath(endpoint)
                          .UsingGet()
                          .WithHeader("Authorization", $"Bearer {bearerToken}"))
            .RespondWith(Response.Create().WithStatusCode((int)HttpStatusCode.OK).WithBody(responseBody));
    }

    private ServiceProvider CreateProviderUsingUofSdkBootstrap()
    {
        var configuration = new TokenSetter(new TestSectionProvider(TestSection.GetDefaultSection()),
                                            _ => new TestBookmakerDetailsProvider(),
                                            _ => new TestProducersProvider())
                           .SetClientAuthentication(TestConsts.AnyPrivateKeyJwt)
                           .SelectCustom()
                           .SetApiHost("localhost")
                           .SetMessagingHost("localhost")
                           .SetClientAuthenticationHost(_wireMockUri.Host)
                           .SetClientAuthenticationPort(_wireMockUri.Port)
                           .SetClientAuthenticationUseSsl(false)
                           .SetInactivitySeconds(30)
                           .SetHttpClientTimeout(5)
                           .SetHttpClientRecoveryTimeout(5)
                           .SetHttpClientFastFailingTimeout(5)
                           .SetDesiredLanguages(TestData.Cultures3)
                           .Build();

        var services = new ServiceCollection();
        services.AddUofSdkServices(configuration);
        services.AddSingleton(_authenticationClientMock.Object);
        return services.BuildServiceProvider();
    }

    private static AuthenticationTokenApiResponse CreateTokenResponse(string accessToken)
    {
        return new AuthenticationTokenApiResponse
        {
            AccessToken = accessToken,
            ExpiresIn = 3600,
            TokenType = "Bearer"
        };
    }
}
