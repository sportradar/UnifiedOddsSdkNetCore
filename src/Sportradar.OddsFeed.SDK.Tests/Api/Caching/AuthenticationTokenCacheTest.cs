// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Authentication;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Api;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;
using Xunit.Abstractions;
using ZiggyCreatures.Caching.Fusion;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Caching;

public class AuthenticationTokenCacheTest : IAsyncLifetime
{
    private const int HttpTimeoutInSeconds = 10;
    private const int MaxConnections = 10;
    private WireMockServer _wireMockServer;
    private readonly XunitLoggerFactory _xUnitLogFactory;

    public AuthenticationTokenCacheTest(ITestOutputHelper testOutputHelper)
    {
        _xUnitLogFactory = new XunitLoggerFactory(testOutputHelper);
    }

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

    [Fact]
    public async Task GetTokenForApiWhenCalledThenReturnsAccessToken()
    {
        var uofConfiguration = GetConfigurationWithAuthenticationFor(_wireMockServer);
        var authenticationTokenCache = CreateAuthenticationTokenCacheFor(uofConfiguration);

        const string testJwtToken = "test-jwt-token";
        StubWireMockToReturnAuthenticationToken(testJwtToken);

        var tokenForApi = await authenticationTokenCache.GetTokenForApi();

        tokenForApi.ShouldBe(testJwtToken);
    }

    [Fact]
    public async Task RefreshTokenFromSingleThreadRefreshesTokenOnlyOnce()
    {
        var uofConfiguration = GetConfigurationWithAuthenticationFor(_wireMockServer);
        var authenticationTokenCache = CreateAuthenticationTokenCacheFor(uofConfiguration);

        const string oldJwtToken = "old-jwt-token";
        StubWireMockToReturnAuthenticationToken(oldJwtToken);

        var tokenToBeRefreshed = await authenticationTokenCache.GetTokenForApi();

        const string newJwtToken = "new-jwt-token";
        StubWireMockToReturnAuthenticationToken(newJwtToken);

        await authenticationTokenCache.RefreshApiTokenAsync(tokenToBeRefreshed);

        var currentToken = await authenticationTokenCache.GetTokenForApi();

        currentToken.ShouldBe(newJwtToken);
    }

    [Fact]
    public async Task RefreshTokenWhenCiamFailsTwiceThenAttemptsToRefreshToken()
    {
        var uofConfiguration = GetConfigurationWithAuthenticationFor(_wireMockServer);
        var authenticationTokenCache = CreateAuthenticationTokenCacheFor(uofConfiguration);

        const string jwtToken = "jwt-token";
        StubAuthenticationApiToFailTwiceThenReturnAuthenticationToken(jwtToken);

        await authenticationTokenCache.RefreshApiTokenAsync(jwtToken);
        var currentToken = await authenticationTokenCache.GetTokenForApi();

        currentToken.ShouldBe(jwtToken);
    }

    [Fact]
    public async Task RefreshTokenFromMultipleThreadsRefreshesTokenOnlyOnce()
    {
        var uofConfiguration = GetConfigurationWithAuthenticationFor(_wireMockServer);
        var authenticationTokenCache = CreateAuthenticationTokenCacheFor(uofConfiguration);

        const string testJwtToken = "old-jwt-token";
        StubWireMockToReturnAuthenticationToken(testJwtToken);

        var tokenToBeRefreshed = await authenticationTokenCache.GetTokenForApi();
        ResetWireMockLog();

        const string newJwtToken = "new-jwt-token";
        StubWireMockToReturnAuthenticationToken(newJwtToken);

        await Run10000TimesInParallel(() => authenticationTokenCache.RefreshApiTokenAsync(tokenToBeRefreshed));

        var newToken = await authenticationTokenCache.GetTokenForApi();

        newToken.ShouldBe(newJwtToken);
        _wireMockServer.RequestsMatching(url => url.EndsWith("oauth/token", StringComparison.OrdinalIgnoreCase)).ShouldHaveSingleItem();
    }

    private static async Task Run10000TimesInParallel(Func<Task> action)
    {
        var getTokenTasks = Enumerable.Range(0, 10000).ToArray().Select(_ => action());
        await Task.WhenAll(getTokenTasks);
    }

    private void ResetWireMockLog()
    {
        _wireMockServer.ResetLogEntries();
    }

    private AuthenticationTokenCache CreateAuthenticationTokenCacheFor(IUofConfiguration config)
    {
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                             .Returns(new HttpClient()
                             {
                                 BaseAddress = new Uri(config.Api.BaseUrl)
                             });

        return new AuthenticationTokenCache(CreateFusionCacheProviderReturningFusionCache().Object,
                                            new AuthenticationClient(httpClientFactoryMock.Object, "AuthHttpClient", _xUnitLogFactory),
                                            new JsonWebTokenFactory(config),
                                            _xUnitLogFactory.CreateLogger<AuthenticationTokenCache>());
    }

    private void StubWireMockToReturnAuthenticationToken(string jwtToken)
    {
        var commonIamTokenObject = new CommonIamResponse
        {
            AccessToken = jwtToken,
            ExpiresIn = 30000,
            TokenType = "Bearer"
        };

        _wireMockServer.Given(Request.Create()
                                     .UsingPost()
                                     .WithPath("/oauth/token"))
                       .RespondWith(Response.Create()
                                            .WithStatusCode(200)
                                            .WithBody(JsonSerializer.Serialize(commonIamTokenObject)));
    }

    private void StubAuthenticationApiToFailTwiceThenReturnAuthenticationToken(string jwtToken)
    {
        const string scenarioName = "FailTwiceThenSucceed";
        var commonIamTokenObject = new CommonIamResponse
        {
            AccessToken = jwtToken,
            ExpiresIn = 30000,
            TokenType = "Bearer"
        };

        _wireMockServer.Given(Request.Create()
                                     .UsingPost()
                                     .WithPath("/oauth/token"))
                       .InScenario(scenarioName)
                       .WillSetStateTo("FirstFailure")
                       .RespondWith(Response.Create()
                                            .WithStatusCode(500)
                                            .WithBody("{\"error\":\"server_error\"}"));

        _wireMockServer.Given(Request.Create()
                                     .UsingPost()
                                     .WithPath("/oauth/token"))
                       .InScenario(scenarioName)
                       .WhenStateIs("FirstFailure")
                       .WillSetStateTo("SecondFailure")
                       .RespondWith(Response.Create()
                                            .WithStatusCode(500)
                                            .WithBody("{\"error\":\"server_error\"}"));

        _wireMockServer.Given(Request.Create()
                                     .UsingPost()
                                     .WithPath("/oauth/token"))
                       .InScenario(scenarioName)
                       .WhenStateIs("SecondFailure")
                       .RespondWith(Response.Create()
                                            .WithStatusCode(200)
                                            .WithBody(JsonSerializer.Serialize(commonIamTokenObject)));
    }

    private static IUofConfiguration GetConfigurationWithAuthenticationFor(WireMockServer wireMockServer)
    {
        var mockConfiguration = new Mock<IUofConfiguration>();
        var mockApiConfiguration = new Mock<IUofApiConfiguration>();

        var tenSeconds = TimeSpan.FromSeconds(HttpTimeoutInSeconds);
        mockApiConfiguration.SetupGet(x => x.Host).Returns(new Uri(wireMockServer.Url!).Host);
        mockApiConfiguration.SetupGet(x => x.BaseUrl).Returns(wireMockServer.Url);
        mockApiConfiguration.SetupGet(x => x.UseSsl).Returns(false);
        mockApiConfiguration.SetupGet(x => x.HttpClientTimeout).Returns(tenSeconds);
        mockApiConfiguration.SetupGet(x => x.HttpClientRecoveryTimeout).Returns(tenSeconds);
        mockApiConfiguration.SetupGet(x => x.HttpClientFastFailingTimeout).Returns(tenSeconds);
        mockApiConfiguration.SetupGet(x => x.MaxConnectionsPerServer).Returns(MaxConnections);

        var cache = new Mock<IUofCacheConfiguration>();
        var auth = new Mock<UofClientAuthentication.IPrivateKeyJwt>();
        auth.SetupGet(x => x.ClientId).Returns("client-id");
        auth.SetupGet(x => x.SigningKeyId).Returns("signing-key-id");
        auth.SetupGet(x => x.PrivateKey).Returns(new RsaSecurityKey(RSA.Create()));
        auth.SetupGet(x => x.Host).Returns(new Uri(wireMockServer.Url!).Host);
        auth.SetupGet(x => x.Port).Returns(wireMockServer.Port);
        auth.SetupGet(x => x.UseSsl).Returns(false);

        mockConfiguration.SetupGet(x => x.Api).Returns(mockApiConfiguration.Object);
        mockConfiguration.SetupGet(x => x.Cache).Returns(cache.Object);
        mockConfiguration.SetupGet(x => x.Authentication).Returns(auth.Object);

        return mockConfiguration.Object;
    }

    private static Mock<IFusionCacheProvider> CreateFusionCacheProviderReturningFusionCache()
    {
        var fusionCacheProviderMock = new Mock<IFusionCacheProvider>();
        fusionCacheProviderMock.Setup(cacheProvider => cacheProvider.GetCache(It.IsAny<string>()))
                               .Returns(new FusionCache(new FusionCacheOptions()));
        return fusionCacheProviderMock;
    }
}
