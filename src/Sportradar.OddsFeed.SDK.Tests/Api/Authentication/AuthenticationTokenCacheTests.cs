// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Api.Internal.Authentication;
using Sportradar.OddsFeed.SDK.Api.Internal.Handlers;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;
using ZiggyCreatures.Caching.Fusion;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Authentication;

public class AuthenticationTokenCacheTests
{
    private readonly ITestOutputHelper _outputHelper;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private Mock<IHttpClientFactory> _httpClientFactoryMock;
    private AuthenticationTokenCache _authenticationTokenCache;
    private XunitLoggerFactory _loggerFactory;

    public AuthenticationTokenCacheTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        SetupInfrastructureWithLogging(_outputHelper, LogLevel.Trace);
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

    private void SetupHttpClientMockWithAuthorizedToken(string token = "any-response-token", int delayInMs = 10)
    {
        SetupHttpClientMockWithResponse(CreateValidTokenResponse(token), delayInMs);
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

    private static AuthenticationTokenApiResponse CreateValidTokenResponse(string accessToken)
    {
        return new AuthenticationTokenApiResponse
        {
            AccessToken = accessToken,
            ExpiresIn = 3600,
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

    private static IFusionCacheProvider GetFusionCacheProvider()
    {
        var fusionCacheMock = new Mock<IFusionCacheProvider>();
        fusionCacheMock.Setup(f => f.GetCache(It.IsAny<string>())).Returns(new FusionCache(new FusionCacheOptions()));
        return fusionCacheMock.Object;
    }
}
