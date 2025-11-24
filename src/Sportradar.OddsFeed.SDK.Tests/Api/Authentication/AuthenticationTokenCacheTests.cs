// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Moq.Protected;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Api.Internal.Authentication;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Handlers;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;
using Xunit.Abstractions;
using ZiggyCreatures.Caching.Fusion;
using FakeTimeProvider = Microsoft.Extensions.Time.Testing.FakeTimeProvider;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Authentication;

public class AuthenticationTokenCacheTests : IAsyncLifetime
{
    private const int MaxConnectionsPerServer = 100;
    private static readonly TimeSpan FastFailingTimeout = TimeSpan.FromSeconds(10);
    private readonly ITestOutputHelper _outputHelper;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private Mock<IHttpClientFactory> _httpClientFactoryMock;
    private AuthenticationTokenCache _authenticationTokenCache;
    private XunitLoggerFactory _loggerFactory;
    private WireMockServer _wireMockServer;
    private Uri _authenticationServerUri;

    public AuthenticationTokenCacheTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        SetupInfrastructureWithLogging(_outputHelper, LogLevel.Trace);
    }

    public Task InitializeAsync()
    {
        _wireMockServer = WireMockServer.Start();
        _authenticationServerUri = new Uri(_wireMockServer.Url!);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _wireMockServer?.Stop();
        _wireMockServer?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetTokenForApiWhenNoTokenCachedThenReturnsNewToken()
    {
        SetupHttpClientMockWithAuthorizedToken();

        var token = await _authenticationTokenCache.GetTokenForApi();

        token.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetTokenForApiWhenNoTokenCachedThenRequiresSingleApiCall()
    {
        SetupHttpClientMockWithAuthorizedToken();

        _ = await _authenticationTokenCache.GetTokenForApi();

        VerifyApiCallWasMade(Times.Once());
    }

    [Fact]
    public async Task GetTokenForApiWhenNoTokenAndReturnsUnauthorizedThenLog()
    {
        SetupHttpClientMockWithUnauthorizedResponse();

        var token = await _authenticationTokenCache.GetTokenForApi();

        token.ShouldBeNull();

        VerifyApiCallWasMade(Times.Once());
    }

    [Fact]
    public async Task GetTokenForApiWhenReturnsInvalidJsonThenThrowJsonException()
    {
        _httpMessageHandlerMock.Protected()
                               .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                               .ReturnsAsync(new HttpResponseMessage
                               {
                                   StatusCode = HttpStatusCode.OK,
                                   Content = new StringContent("{\"error\":\"invalid_client\",\"error_description\":\"Unauthorized\"")
                               });

        var token = await _authenticationTokenCache.GetTokenForApi();

        token.ShouldBeNull();

        VerifyApiCallWasMade(Times.Once());
    }

    [Fact]
    public async Task GetTokenForApiWhenCallingSecondTimeThenReturnsFirstToken()
    {
        SetupHttpClientMockWithAuthorizedToken(Guid.NewGuid().ToString());

        var token1 = await _authenticationTokenCache.GetTokenForApi();
        var token2 = await _authenticationTokenCache.GetTokenForApi();

        token1.ShouldNotBeNullOrEmpty();
        token2.ShouldNotBeNullOrEmpty();
        token1.ShouldBe(token2);

        VerifyApiCallWasMade(Times.Once());
    }

    [Fact]
    public async Task GetTokenForFeedWhenNoTokenCachedThenReturnsNewToken()
    {
        SetupHttpClientMockWithAuthorizedToken();

        var token = await _authenticationTokenCache.GetTokenForFeed();

        token.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetTokenForFeedWhenNoTokenCachedThenRequiresSingleApiCall()
    {
        SetupHttpClientMockWithAuthorizedToken();

        _ = await _authenticationTokenCache.GetTokenForFeed();

        VerifyApiCallWasMade(Times.Once());
    }

    [Fact]
    public async Task GetTokenForFeedWhenCallingSecondTimeThenReturnsFirstToken()
    {
        SetupHttpClientMockWithAuthorizedToken(Guid.NewGuid().ToString());

        var token1 = await _authenticationTokenCache.GetTokenForFeed();
        var token2 = await _authenticationTokenCache.GetTokenForFeed();

        token1.ShouldNotBeNullOrEmpty();
        token2.ShouldNotBeNullOrEmpty();
        token1.ShouldBe(token2);

        VerifyApiCallWasMade(Times.Once());
    }

    [Fact]
    public async Task GetTokenForFeedWhenNoTokenAndReturnsUnauthorizedThenLog()
    {
        SetupHttpClientMockWithUnauthorizedResponse();

        var token = await _authenticationTokenCache.GetTokenForFeed();

        token.ShouldBeNull();

        VerifyApiCallWasMade(Times.Once());
    }

    [Fact]
    public void JsonWebTokenFactoryWhenWithoutUofConfigThenThrows()
    {
        Should.Throw<ArgumentNullException>(() => new JsonWebTokenFactory(null));
    }

    [Fact]
    public async Task GetTokenWhenResponseIncludeExpiredTokenThenIgnore()
    {
        var tokenResponse = new AuthenticationTokenApiResponse
        {
            AccessToken = "accessToken",
            ExpiresIn = 0,
            TokenType = "Bearer"
        };
        SetupHttpClientMockWithResponse(tokenResponse);

        var token = await _authenticationTokenCache.GetTokenForApi();

        token.ShouldBeNull();

        VerifyApiCallWasMade(Times.Once());
    }

    [Fact]
    public async Task GetTokenForApiWithValidResponseWhenLoggingSetToDebugThenDebugLogIsRecorded()
    {
        SetupInfrastructureWithLogging(_outputHelper, LogLevel.Debug);
        SetupHttpClientMockWithAuthorizedToken();

        _ = await _authenticationTokenCache.GetTokenForApi();

        var logger = _loggerFactory.GetRegisteredLogger(typeof(RestTraffic).FullName);
        logger.ShouldNotBeNull();

        logger.Messages.ShouldBeOfSize(2);
        var initMessage = logger.Messages.First(f => f.Contains("initiated"));
        var resultMessage = logger.Messages.First(f => f.Contains("took"));

        initMessage.ShouldNotBeNull();
        initMessage.ShouldContain("RestTraffic");
        initMessage.ShouldContain("Information");
        initMessage.ShouldNotContain("TraceId:");
        initMessage.ShouldContain("POST:");
        resultMessage.ShouldNotBeNull();
        resultMessage.ShouldContain("RestTraffic");
        resultMessage.ShouldContain("Debug");
        resultMessage.ShouldContain("TraceId:");
        resultMessage.ShouldContain("POST:");
        resultMessage.ShouldNotContain("TraceId:  POST:");
        resultMessage.ShouldContain(" took ");
        resultMessage.ShouldContain("Response:");
        resultMessage.ShouldContain("AccessToken");
        resultMessage.ShouldContain("ExpiresIn");
        resultMessage.ShouldContain("TokenType");
    }

    [Fact]
    public async Task GetTokenForApiWithValidResponseWhenLoggingSetToDebugThenOnlyInfoLogIsRecorded()
    {
        SetupInfrastructureWithLogging(_outputHelper, LogLevel.Information);
        SetupHttpClientMockWithAuthorizedToken();

        _ = await _authenticationTokenCache.GetTokenForApi();

        var logger = _loggerFactory.GetRegisteredLogger(typeof(RestTraffic).FullName);
        logger.ShouldNotBeNull();

        logger.Messages.ShouldBeOfSize(2);
        var initMessage = logger.Messages.First(f => f.Contains("initiated"));
        var resultMessage = logger.Messages.First(f => f.Contains("took"));

        initMessage.ShouldNotBeNull();
        initMessage.ShouldContain("RestTraffic");
        initMessage.ShouldContain("Information");
        initMessage.ShouldNotContain("TraceId:");
        initMessage.ShouldContain("POST:");
        resultMessage.ShouldNotBeNull();
        resultMessage.ShouldContain("RestTraffic");
        resultMessage.ShouldContain("Information");
        resultMessage.ShouldContain("TraceId:");
        resultMessage.ShouldContain("POST:");
        resultMessage.ShouldNotContain("TraceId:  POST:");
        resultMessage.ShouldContain(" took ");
        resultMessage.ShouldNotContain("Response:");
        resultMessage.ShouldNotContain("AccessToken");
        resultMessage.ShouldNotContain("ExpiresIn");
        resultMessage.ShouldNotContain("TokenType");
    }

    [Fact]
    public async Task GetTokenForApiWithUnauthorizedResponseWhenLoggingSetToDebugThenErrorLogIsRecorded()
    {
        SetupInfrastructureWithLogging(_outputHelper, LogLevel.Debug);
        SetupHttpClientMockWithUnauthorizedResponse();

        _ = await _authenticationTokenCache.GetTokenForApi();

        var logger = _loggerFactory.GetRegisteredLogger(typeof(RestTraffic).FullName);
        logger.ShouldNotBeNull();

        logger.Messages.ShouldBeOfSize(2);
        var initMessage = logger.Messages.First(f => f.Contains("initiated"));
        var resultMessage = logger.Messages.First(f => f.Contains("took"));

        initMessage.ShouldNotBeNull();
        initMessage.ShouldContain("RestTraffic");
        initMessage.ShouldContain("Information");
        initMessage.ShouldNotBe("TraceId:");
        initMessage.ShouldContain("POST:");
        resultMessage.ShouldNotBeNull();
        resultMessage.ShouldContain("RestTraffic");
        resultMessage.ShouldContain("Error");
        resultMessage.ShouldContain("TraceId:");
        resultMessage.ShouldContain("POST:");
        resultMessage.ShouldNotContain("TraceId:  POST:");
        resultMessage.ShouldContain(" took ");
        resultMessage.ShouldNotContain("Response:");
        resultMessage.ShouldNotContain("AccessToken");
        resultMessage.ShouldNotContain("ExpiresIn");
        resultMessage.ShouldNotContain("TokenType");
    }

    [Fact]
    public async Task GetTokenForApiWithValidResponseWhenNoDecoratorThenNoTraceId()
    {
        SetupInfrastructureWithoutDecorator(_outputHelper, LogLevel.Debug);
        SetupHttpClientMockWithAuthorizedToken();

        _ = await _authenticationTokenCache.GetTokenForApi();

        var logger = _loggerFactory.GetRegisteredLogger(typeof(RestTraffic).FullName);
        logger.ShouldNotBeNull();

        logger.Messages.ShouldBeOfSize(2);
        var initMessage = logger.Messages.First(f => f.Contains("initiated"));
        var resultMessage = logger.Messages.First(f => f.Contains("took"));

        initMessage.ShouldNotBeNull();
        initMessage.ShouldContain("RestTraffic");
        initMessage.ShouldContain("Information");
        initMessage.ShouldNotContain("TraceId:");
        initMessage.ShouldContain("POST:");
        resultMessage.ShouldNotBeNull();
        resultMessage.ShouldContain("RestTraffic");
        resultMessage.ShouldContain("Debug");
        resultMessage.ShouldContain("TraceId:  POST:");
        resultMessage.ShouldContain(" took ");
        resultMessage.ShouldContain("Response:");
        resultMessage.ShouldContain("AccessToken");
        resultMessage.ShouldContain("ExpiresIn");
        resultMessage.ShouldContain("TokenType");
    }

    [Fact]
    public async Task GetTokenForApiWhenMultipleConcurrentRequestsThenOnlyOneApiCallIsMade()
    {
        SetupInfrastructureWithLogging(_outputHelper, LogLevel.Debug);
        SetupHttpClientMockWithAuthorizedToken(delayInMs: 500);

        var tokens = new string[10];

        var tasks = Enumerable.Range(0, 10)
                              .Select(async i => tokens[i] = await _authenticationTokenCache.GetTokenForApi())
                              .ToArray();
        await Task.WhenAll(tasks);

        tokens.All(t => t == tokens[0]).ShouldBeTrue();
        VerifyApiCallWasMade(Times.Once());
    }

    [Fact]
    public async Task GetTokenForFeedWhenMultipleConcurrentRequestsThenOnlyOneApiCallIsMade()
    {
        SetupInfrastructureWithLogging(_outputHelper, LogLevel.Debug);
        SetupHttpClientMockWithAuthorizedToken(delayInMs: 500);

        var tokens = new string[10];

        var tasks = Enumerable.Range(0, 10)
                              .Select(async i => tokens[i] = await _authenticationTokenCache.GetTokenForFeed())
                              .ToArray();
        await Task.WhenAll(tasks);

        tokens.All(t => t == tokens[0]).ShouldBeTrue();
        VerifyApiCallWasMade(Times.Once());
    }

    [Fact]
    public async Task GetTokenForBothWhenMultipleConcurrentRequestsThenOnlyOneApiCallPerAudienceIsMade()
    {
        SetupInfrastructureWithLogging(_outputHelper, LogLevel.Debug);
        SetupHttpClientMockWithAuthorizedToken(delayInMs: 500);

        var tokensForApi = new string[10];
        var tokensForFeed = new string[10];

        var tasks = Enumerable.Range(0, 10)
                              .Select(async i => tokensForApi[i] = await _authenticationTokenCache.GetTokenForApi())
                              .ToArray();
        tasks = tasks.Concat(Enumerable.Range(0, 10)
                                       .Select(async i => tokensForFeed[i] = await _authenticationTokenCache.GetTokenForFeed()))
                     .ToArray();
        await Task.WhenAll(tasks);

        tokensForApi.All(t => t == tokensForApi[0]).ShouldBeTrue();
        tokensForFeed.All(t => t == tokensForFeed[0]).ShouldBeTrue();
        VerifyApiCallWasMade(Times.Exactly(2));
    }

    [Fact]
    public async Task GetTokenForApiWhenCiamEndpointFailsThenNextCallReturnsNullDespiteCiamRecovering()
    {
        const string validToken = "valid-access-token";
        StubAuthenticationWithResponse500ThenWithResponseReturning(validToken);

        var configuration = GetValidAuthenticationConfigurationFor(_authenticationServerUri, FastFailingTimeout, MaxConnectionsPerServer);
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddUofSdkServices(configuration);

        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var authenticationTokenCache = scope.ServiceProvider.GetRequiredService<IAuthenticationTokenCache>();

        var firstToken = await authenticationTokenCache.GetTokenForApi();
        var secondToken = await authenticationTokenCache.GetTokenForApi();

        firstToken.ShouldBeNull("First call should return null when CIAM fails");
        secondToken.ShouldBeNull("Second call should return null due to circuit breaker, despite CIAM recovering");

        var requests = _wireMockServer.LogEntries;
        requests.Count.ShouldBe(1, "CIAM endpoint should be called only once due to circuit breaker");
    }

    [Fact]
    public async Task GetTokenForApiWhenCiamRecoversAfterFailureThenCircuitClosesAfterBreakDuration()
    {
        const string validToken = "recovered-access-token";
        StubAuthenticationWithResponse500ThenWithResponseReturning(validToken);

        var fakeTimeProvider = new FakeTimeProvider();
        var configuration = GetValidAuthenticationConfigurationFor(_authenticationServerUri, FastFailingTimeout, MaxConnectionsPerServer);
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddUofSdkServices(configuration);
        serviceCollection.AddSingleton<TimeProvider>(fakeTimeProvider);

        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var authenticationTokenCache = scope.ServiceProvider.GetRequiredService<IAuthenticationTokenCache>();

        var firstToken = await authenticationTokenCache.GetTokenForApi();
        fakeTimeProvider.Advance(TimeSpan.FromSeconds(5));
        var secondToken = await authenticationTokenCache.GetTokenForApi();

        firstToken.ShouldBeNull("First call should return null when CIAM fails");
        secondToken.ShouldBe(validToken, "Second call should return the valid token from recovered CIAM");

        var requests = _wireMockServer.LogEntries;
        requests.Count.ShouldBeGreaterThanOrEqualTo(2, "CIAM endpoint should be called two times");
    }

    [Fact]
    public async Task GetTokenForApiWhenCiamKeepsFailingThenRequestsTokenEvery5Seconds()
    {
        StubAuthenticationWithResponse500(_wireMockServer);

        var fakeTimeProvider = new FakeTimeProvider();
        var configuration = GetValidAuthenticationConfigurationFor(_authenticationServerUri, FastFailingTimeout, MaxConnectionsPerServer);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddUofSdkServices(configuration);
        serviceCollection.AddSingleton<TimeProvider>(fakeTimeProvider);

        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var authenticationTokenCache = scope.ServiceProvider.GetRequiredService<IAuthenticationTokenCache>();

        const int requestsCount = 10;
        var tokens = await RequestAuthenticationTokenEvery5Seconds(authenticationTokenCache, fakeTimeProvider, requestsCount);

        tokens.ShouldAllBe(token => token == null, "All token requests should return null when CIAM keeps failing");

        var requests = _wireMockServer.LogEntries;
        requests.ShouldBeOfSize(requestsCount);
    }

    [Fact]
    public async Task GetTokenForApiWhenCiamKeepsFailingFor10TimesThenCircuitShouldOpenFor5MinutesThenClose()
    {
        const string recoveredToken = "recovered-token-after-5-minutes";
        var validTokenResponse = CreateAuthenticationApiResponseFor(recoveredToken);

        StubAuthenticationWithResponse500(_wireMockServer);

        var fakeTimeProvider = new FakeTimeProvider();
        var configuration = GetValidAuthenticationConfigurationFor(_authenticationServerUri, FastFailingTimeout, MaxConnectionsPerServer);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddUofSdkServices(configuration);
        serviceCollection.AddSingleton<TimeProvider>(fakeTimeProvider);

        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var authenticationTokenCache = scope.ServiceProvider.GetRequiredService<IAuthenticationTokenCache>();

        const int requestsCount = 10;
        await RequestAuthenticationTokenEvery5Seconds(authenticationTokenCache, fakeTimeProvider, requestsCount);

        fakeTimeProvider.Advance(TimeSpan.FromMinutes(5));

        StubAuthenticationWithValidResponse(_wireMockServer, validTokenResponse);

        var recoveredTokenResult = await authenticationTokenCache.GetTokenForApi();

        recoveredTokenResult.ShouldBe(recoveredToken, "Should receive the valid token from recovered CIAM endpoint");
        _wireMockServer.LogEntries.Count.ShouldBe(requestsCount + 1, "One new CIAM was sent after the circuit breaker open period elapsed");
    }

    [Fact]
    public async Task GetTokenForApiWhenCiamKeepsFailingFor10TimesThenCircuitShouldOpenFor5Minutes()
    {
        const string recoveredToken = "recovered-token-after-5-minutes";
        var validTokenResponse = CreateAuthenticationApiResponseFor(recoveredToken);

        StubAuthenticationWithResponse500(_wireMockServer);

        var fakeTimeProvider = new FakeTimeProvider();
        var configuration = GetValidAuthenticationConfigurationFor(_authenticationServerUri, FastFailingTimeout, MaxConnectionsPerServer);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddUofSdkServices(configuration);
        serviceCollection.AddSingleton<TimeProvider>(fakeTimeProvider);

        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var authenticationTokenCache = scope.ServiceProvider.GetRequiredService<IAuthenticationTokenCache>();

        const int requestsCount = 10;
        await RequestAuthenticationTokenEvery5Seconds(authenticationTokenCache, fakeTimeProvider, requestsCount);

        fakeTimeProvider.Advance(TimeSpan.FromMinutes(4));

        StubAuthenticationWithValidResponse(_wireMockServer, validTokenResponse);

        var tokenAfter4Minutes = await authenticationTokenCache.GetTokenForApi();

        tokenAfter4Minutes.ShouldBeNull("Should still receive null due to circuit breaker still open");
        _wireMockServer.LogEntries.Count.ShouldBe(requestsCount, "No new CIAM request should be sent while circuit breaker is open");
    }

    [Fact]
    public async Task GetTokenForApiWhenCiamKeepsFailingThenCircuitBreakerOpen10TimesFor5SecondsThenAfter10FailuresOpensFor5Minutes()
    {
        StubAuthenticationWithResponse500(_wireMockServer);

        var fakeTimeProvider = new FakeTimeProvider();
        var configuration = GetValidAuthenticationConfigurationFor(_authenticationServerUri, FastFailingTimeout, MaxConnectionsPerServer);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddUofSdkServices(configuration);
        serviceCollection.AddSingleton<TimeProvider>(fakeTimeProvider);

        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var authenticationTokenCache = scope.ServiceProvider.GetRequiredService<IAuthenticationTokenCache>();

        const int requestsCount = 10;
        await RequestAuthenticationTokenEvery5Seconds(authenticationTokenCache, fakeTimeProvider, requestsCount);

        fakeTimeProvider.Advance(TimeSpan.FromMinutes(5));

        const int requestsCount100 = 100;
        var tokens = await RequestAuthenticationTokenEvery5Minutes(authenticationTokenCache, fakeTimeProvider, requestsCount100);

        tokens.ShouldAllBe(token => token == null, "All token requests should return null when CIAM keeps failing");

        _wireMockServer.LogEntries.ShouldBeOfSize(requestsCount + requestsCount100);
    }

    [Fact]
    public async Task GetTokenForApiWhenCiamFailsFor5TimesThenRecoversAndCircuitClosesWithContinued5SecondIntervals()
    {
        const string recoveredToken = "recovered-token-after-5-failures";

        StubAuthenticationWithResponse500(_wireMockServer);

        var fakeTimeProvider = new FakeTimeProvider();
        var configuration = GetValidAuthenticationConfigurationFor(_authenticationServerUri, FastFailingTimeout, MaxConnectionsPerServer);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddUofSdkServices(configuration);
        serviceCollection.AddSingleton<TimeProvider>(fakeTimeProvider);

        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var authenticationTokenCache = scope.ServiceProvider.GetRequiredService<IAuthenticationTokenCache>();

        const int failedRequestsCount = 5;
        var failedTokens = await RequestAuthenticationTokenEvery5Seconds(authenticationTokenCache, fakeTimeProvider, failedRequestsCount);

        failedTokens.ShouldAllBe(token => token == null, "All first 5 token requests should return null when CIAM is failing");
        fakeTimeProvider.Advance(TimeSpan.FromSeconds(6));

        const int tokenExpiryTimeInSeconds = 1;
        var validTokenResponse = CreateAuthenticationApiResponseFor(recoveredToken, tokenExpiryTimeInSeconds);
        StubAuthenticationWithValidResponse(_wireMockServer, validTokenResponse);

        _wireMockServer.ResetLogEntries();
        var recoveredTokenResult = await authenticationTokenCache.GetTokenForApi();
        recoveredTokenResult.ShouldBe(recoveredToken, "Should receive the valid token after CIAM recovers");

        await Task.Delay(TimeSpan.FromSeconds(tokenExpiryTimeInSeconds + 1));

        var requestsAfterRecovery = _wireMockServer.LogEntries.Count;
        requestsAfterRecovery.ShouldBe(1, "Should have made a request after recovery");

        StubAuthenticationWithResponse500(_wireMockServer);
        _wireMockServer.ResetLogEntries();
        const int additionalRequestsCount = 10;
        failedTokens = await RequestAuthenticationTokenEvery5Seconds(authenticationTokenCache, fakeTimeProvider, additionalRequestsCount);

        failedTokens.ShouldAllBe(token => token == null, "All additional token requests should return null when CIAM is failing again");
        _wireMockServer.LogEntries.Count.ShouldBe(additionalRequestsCount, "Should have made requests for all additional attempts");
    }

    [Fact]
    public async Task GetTokenForApiWhenCiamKeepsFailingAndMultipleThreadsTryToGetNewTokenThenCircuitBreakerOpenOnlyOnce()
    {
        StubAuthenticationWithResponse500(_wireMockServer);

        var fakeTimeProvider = new FakeTimeProvider();
        var configuration = GetValidAuthenticationConfigurationFor(_authenticationServerUri, FastFailingTimeout, MaxConnectionsPerServer);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddUofSdkServices(configuration);
        serviceCollection.AddSingleton<TimeProvider>(fakeTimeProvider);

        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var authenticationTokenCache = scope.ServiceProvider.GetRequiredService<IAuthenticationTokenCache>();

        const int requestsCount = 1000;
        var tasks = Enumerable.Range(0, requestsCount).Select(_ => authenticationTokenCache.GetTokenForApi());
        await Task.WhenAll(tasks);
        fakeTimeProvider.Advance(TimeSpan.FromSeconds(5));
        tasks = Enumerable.Range(0, requestsCount).Select(_ => authenticationTokenCache.GetTokenForApi());
        await Task.WhenAll(tasks);

        _wireMockServer.LogEntries.ShouldBeOfSize(2);
    }

    private void SetupHttpClientMockWithAuthorizedToken(string token = "any-response-token", int delayInMs = 10)
    {
        SetupHttpClientMockWithResponse(CreateAuthenticationApiResponseFor(token), delayInMs);
    }

    private void SetupHttpClientMockWithResponse(AuthenticationTokenApiResponse tokenResponse, int delayInMs = 10)
    {
        _httpMessageHandlerMock.Protected()
                               .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                               .Returns(async (HttpRequestMessage _, CancellationToken token) =>
                                        {
                                            await Task.Delay(delayInMs, token);
                                            return new HttpResponseMessage
                                            {
                                                StatusCode = HttpStatusCode.OK,
                                                Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
                                            };
                                        });
    }

    private void SetupHttpClientMockWithUnauthorizedResponse()
    {
        _httpMessageHandlerMock.Protected()
                               .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                               .ReturnsAsync(new HttpResponseMessage
                               {
                                   StatusCode = HttpStatusCode.Unauthorized,
                                   Content = new StringContent("{\"error\":\"invalid_client\",\"error_description\":\"Unauthorized\"}")
                               });
    }

    private static AuthenticationTokenApiResponse CreateAuthenticationApiResponseFor(string accessToken, int expiryTimeInSeconds = 3600)
    {
        return new AuthenticationTokenApiResponse
        {
            AccessToken = accessToken,
            ExpiresIn = expiryTimeInSeconds,
            TokenType = "Bearer"
        };
    }

    private void VerifyApiCallWasMade(Times times)
    {
        _httpMessageHandlerMock.Protected().Verify("SendAsync", times, ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    private void SetupInfrastructureWithLogging(ITestOutputHelper outputHelper, LogLevel logLevel)
    {
        var uofConfig = TestConfiguration.GetConfigWithCiam();
        _loggerFactory = new XunitLoggerFactory(outputHelper, logLevel);
        var logger = _loggerFactory.CreateLogger<AuthenticationTokenCache>();

        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var requestDecorator = new RequestDecorator();
        var decoratorHandler = new HttpRequestDecoratorHandler(requestDecorator, _httpMessageHandlerMock.Object);

        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                              .Returns(() => new HttpClient(decoratorHandler) { BaseAddress = new Uri("https://localhost/") });

        IAuthenticationClient authenticationClient = new AuthenticationClient(_httpClientFactoryMock.Object, "TestClient", _loggerFactory);
        IJsonWebTokenFactory jsonWebTokenFactory = new JsonWebTokenFactory(uofConfig);

        _authenticationTokenCache = new AuthenticationTokenCache(GetFusionCacheProvider(), authenticationClient, jsonWebTokenFactory, logger);
    }

    private void SetupInfrastructureWithoutDecorator(ITestOutputHelper outputHelper, LogLevel logLevel)
    {
        var uofConfig = TestConfiguration.GetConfigWithCiam();
        _loggerFactory = new XunitLoggerFactory(outputHelper, logLevel);
        var logger = _loggerFactory.CreateLogger<AuthenticationTokenCache>();

        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                              .Returns(() => new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("https://localhost/") });

        IAuthenticationClient authenticationClient = new AuthenticationClient(_httpClientFactoryMock.Object, "TestClient", _loggerFactory);
        IJsonWebTokenFactory jsonWebTokenFactory = new JsonWebTokenFactory(uofConfig);

        _authenticationTokenCache = new AuthenticationTokenCache(GetFusionCacheProvider(), authenticationClient, jsonWebTokenFactory, logger);
    }

    private void StubAuthenticationWithResponse500ThenWithResponseReturning(string validToken)
    {
        const string scenarioName = "CircuitBreaker";
        var validTokenResponse = CreateAuthenticationApiResponseFor(validToken);
        _wireMockServer.Given(Request.Create()
                                     .UsingPost()
                                     .WithPath("/oauth/token"))
                       .InScenario(scenarioName)
                       .WillSetStateTo("CiamRecovered")
                       .RespondWith(Response.Create()
                                            .WithStatusCode(500));

        _wireMockServer.Given(Request.Create()
                                     .UsingPost()
                                     .WithPath("/oauth/token"))
                       .InScenario(scenarioName)
                       .WhenStateIs("CiamRecovered")
                       .RespondWith(Response.Create()
                                            .WithStatusCode(200)
                                            .WithHeader("Content-Type", "application/json")
                                            .WithBody(JsonSerializer.Serialize(validTokenResponse)));
    }

    private static IFusionCacheProvider GetFusionCacheProvider()
    {
        var fusionCacheMock = new Mock<IFusionCacheProvider>();
        fusionCacheMock.Setup(f => f.GetCache(It.IsAny<string>())).Returns(new FusionCache(new FusionCacheOptions()));
        return fusionCacheMock.Object;
    }

    private static IUofConfiguration GetValidAuthenticationConfigurationFor(Uri authenticationServerUri, TimeSpan fastFailingTimeout, int maxConnectionsPerServer)
    {
        var mockConfiguration = new Mock<IUofConfiguration>();
        var mockApiConfiguration = new Mock<IUofApiConfiguration>();

        mockApiConfiguration.SetupGet(x => x.HttpClientFastFailingTimeout).Returns(fastFailingTimeout);
        mockApiConfiguration.SetupGet(x => x.MaxConnectionsPerServer).Returns(maxConnectionsPerServer);

        var cache = new Mock<IUofCacheConfiguration>();

        var mockAuth = new Mock<UofClientAuthentication.IPrivateKeyJwt>();
        mockAuth.Setup(x => x.ClientId).Returns("test-client-id");
        mockAuth.Setup(x => x.SigningKeyId).Returns("test-signing-key-id");
        mockAuth.Setup(x => x.PrivateKey).Returns(new RsaSecurityKey(RSA.Create()));
        mockAuth.Setup(x => x.Host).Returns(authenticationServerUri.Host);
        mockAuth.Setup(x => x.Port).Returns(authenticationServerUri.Port);
        mockAuth.Setup(x => x.UseSsl).Returns(false);

        mockConfiguration.SetupGet(x => x.Api).Returns(mockApiConfiguration.Object);
        mockConfiguration.SetupGet(x => x.Cache).Returns(cache.Object);
        mockConfiguration.SetupGet(x => x.Authentication).Returns(mockAuth.Object);

        return mockConfiguration.Object;
    }

    private static void StubAuthenticationWithValidResponse(WireMockServer wireMockServer, AuthenticationTokenApiResponse validTokenResponse)
    {
        wireMockServer.Given(Request.Create()
                                    .UsingPost()
                                    .WithPath("/oauth/token"))
                      .RespondWith(Response.Create()
                                           .WithStatusCode(200)
                                           .WithHeader("Content-Type", "application/json")
                                           .WithBody(JsonSerializer.Serialize(validTokenResponse)));
    }

    private static void StubAuthenticationWithResponse500(WireMockServer wireMockServer)
    {
        wireMockServer.Given(Request.Create()
                                    .UsingPost()
                                    .WithPath("/oauth/token"))
                      .RespondWith(Response.Create()
                                           .WithStatusCode(500)
                                           .WithBody("{\"error\":\"server_error\"}"));
    }

    private static async Task<string[]> RequestAuthenticationTokenEvery5Seconds(IAuthenticationTokenCache authenticationTokenCache,
        FakeTimeProvider fakeTimeProvider,
        int requestsCount)
    {
        var tokens = new string[requestsCount];
        for (var i = 0; i < requestsCount; i++)
        {
            tokens[i] = await authenticationTokenCache.GetTokenForApi();
            if (i < requestsCount - 1)
            {
                fakeTimeProvider.Advance(TimeSpan.FromSeconds(5));
            }
        }

        return tokens;
    }

    private static async Task<string[]> RequestAuthenticationTokenEvery5Minutes(IAuthenticationTokenCache authenticationTokenCache,
        FakeTimeProvider fakeTimeProvider,
        int requestsCount)
    {
        var tokens = new string[requestsCount];
        for (var i = 0; i < requestsCount; i++)
        {
            tokens[i] = await authenticationTokenCache.GetTokenForApi();
            if (i < requestsCount - 1)
            {
                fakeTimeProvider.Advance(TimeSpan.FromMinutes(5));
            }
        }

        return tokens;
    }
}
