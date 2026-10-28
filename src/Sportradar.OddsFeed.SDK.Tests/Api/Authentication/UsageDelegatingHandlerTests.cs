// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Authentication;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Authentication;

public class UsageDelegatingHandlerTests
{
    [Fact]
    public void TokenProviderConstructorWhenConfigurationIsNullThenThrowsArgumentNullException()
    {
        var exception = Should.Throw<ArgumentNullException>(() => new UsageAuthenticationTokenProvider(null, new Mock<IAuthenticationTokenCache>().Object));

        exception.ParamName.ShouldBe("uofConfiguration");
    }

    [Fact]
    public void TokenProviderConstructorWhenTokenCacheIsNullThenThrowsArgumentNullException()
    {
        var config = GetConfigurationWithAuthentication();

        var exception = Should.Throw<ArgumentNullException>(() => new UsageAuthenticationTokenProvider(config, null));

        exception.ParamName.ShouldBe("tokenCache");
    }

    [Fact]
    public void ConstructorWhenTokenCacheIsNullThenThrowsArgumentNullException()
    {
        var exception = Should.Throw<ArgumentNullException>(() => new UsageDelegatingHandler(null));

        exception.ParamName.ShouldBe("usageTokenProvider");
    }

    [Fact]
    public async Task SendAsyncWhenOnlyAccessTokenAvailableThenAddsXAccessTokenHeader()
    {
        var config = GetConfigurationWithoutAuthentication();
        var tokenCache = GetTokenCacheReturningValidApiToken();
        var usageDelegatingHandler = GetUsageDelegatingHandler(config, tokenCache);
        var httpMessageInvoker = new HttpMessageInvoker(usageDelegatingHandler);
        var httpRequestMessage = GetAnyHttpRequestMessage();

        await httpMessageInvoker.SendAsync(httpRequestMessage, CancellationToken.None);

        httpRequestMessage.Headers.ShouldNotBeNull();
        httpRequestMessage.Headers.Authorization.ShouldBeNull();
        httpRequestMessage.Headers.ShouldContain(c => c.Key.Equals("x-access-token", StringComparison.Ordinal));
        var keyValuePairs = httpRequestMessage.Headers.Where(h => h.Key.Equals("x-access-token", StringComparison.Ordinal)).ToList();
        keyValuePairs.Count.ShouldBe(1);
        keyValuePairs.First().Value.ShouldNotBeNull();
        keyValuePairs.First().Value.Count().ShouldBe(1);
        keyValuePairs.First().Value.First().ShouldBe(config.AccessToken);
    }

    [Fact]
    public async Task SendAsyncWhenClientAuthConfiguredAndTokenAvailableThenAddsXAccessTokenHeader()
    {
        var config = GetConfigurationWithAuthentication();
        var tokenCache = GetTokenCacheReturningValidApiToken();
        var usageDelegatingHandler = GetUsageDelegatingHandler(config, tokenCache);
        var httpMessageInvoker = new HttpMessageInvoker(usageDelegatingHandler);
        var httpRequestMessage = GetAnyHttpRequestMessage();

        await httpMessageInvoker.SendAsync(httpRequestMessage, CancellationToken.None);

        httpRequestMessage.Headers.ShouldNotBeNull();
        httpRequestMessage.Headers.Authorization.ShouldBeNull();
        httpRequestMessage.Headers.ShouldContain(c => c.Key.Equals("x-access-token", StringComparison.Ordinal));
        var keyValuePairs = httpRequestMessage.Headers.Where(h => h.Key.Equals("x-access-token", StringComparison.Ordinal)).ToList();
        keyValuePairs.Count.ShouldBe(1);
        keyValuePairs.First().Value.ShouldNotBeNull();
        keyValuePairs.First().Value.Count().ShouldBe(1);
        keyValuePairs.First().Value.First().ShouldBe("valid-jwt-token-for-api");
    }

    [Fact]
    public async Task SendAsyncWhenClientAuthConfiguredAndTokenIsNullThenThrowsCommunicationException()
    {
        var config = GetConfigurationWithAuthentication();
        var tokenCache = GetTokenCacheReturningNullApiToken();
        var usageDelegatingHandler = GetUsageDelegatingHandler(config, tokenCache);
        var httpMessageInvoker = new HttpMessageInvoker(usageDelegatingHandler);
        var httpRequestMessage = GetAnyHttpRequestMessage();

        var exception = await Should.ThrowAsync<CommunicationException>(() => httpMessageInvoker.SendAsync(httpRequestMessage, CancellationToken.None));

        exception.Message.ShouldBe("Token is not available. Unable to authenticate the request.");
        exception.ResponseCode.ShouldBe(HttpStatusCode.Unauthorized);
        exception.Url.ShouldBe("http://localhost/");
    }

    [Fact]
    public async Task SendAsyncWhenClientAuthConfiguredAndTokenIsNullAndNoAddressInRequestMessageThenThrowsCommunicationException()
    {
        var config = GetConfigurationWithAuthentication();
        var tokenCache = GetTokenCacheReturningNullApiToken();
        var usageDelegatingHandler = GetUsageDelegatingHandler(config, tokenCache);
        var httpMessageInvoker = new HttpMessageInvoker(usageDelegatingHandler);
        var httpRequestMessage = GetHttpRequestMessageWithoutUri();

        var exception = await Should.ThrowAsync<CommunicationException>(() => httpMessageInvoker.SendAsync(httpRequestMessage, CancellationToken.None));

        exception.Message.ShouldBe("Token is not available. Unable to authenticate the request.");
        exception.ResponseCode.ShouldBe(HttpStatusCode.Unauthorized);
        exception.Url.ShouldBeNull();
    }

    [Fact]
    public async Task SendAsyncWhenClientAuthConfiguredAndTokenIsEmptyThenThrowsCommunicationException()
    {
        var config = GetConfigurationWithAuthentication();
        var tokenCache = GetTokenCacheReturningEmptyApiToken();
        var usageDelegatingHandler = GetUsageDelegatingHandler(config, tokenCache);
        var httpMessageInvoker = new HttpMessageInvoker(usageDelegatingHandler);
        var httpRequestMessage = GetAnyHttpRequestMessage();

        var exception = await Should.ThrowAsync<CommunicationException>(() => httpMessageInvoker.SendAsync(httpRequestMessage, CancellationToken.None));

        exception.Message.ShouldBe("Token is not available. Unable to authenticate the request.");
        exception.ResponseCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendAsyncWhenClientAuthNotConfiguredAndAccessTokenAvailableThenAddsAccessTokenHeader()
    {
        var config = GetConfigurationWithoutAuthentication();
        var tokenCache = GetTokenCacheReturningValidApiToken();
        var usageDelegatingHandler = GetUsageDelegatingHandler(config, tokenCache);
        var httpMessageInvoker = new HttpMessageInvoker(usageDelegatingHandler);
        var httpRequestMessage = GetAnyHttpRequestMessage();

        await httpMessageInvoker.SendAsync(httpRequestMessage, CancellationToken.None);

        httpRequestMessage.Headers.Contains(UofSdkBootstrap.HttpClientDefaultRequestHeaderForAccessToken).ShouldBeTrue();
        var headerValues = httpRequestMessage.Headers.GetValues(UofSdkBootstrap.HttpClientDefaultRequestHeaderForAccessToken);
        headerValues.ShouldContain(config.AccessToken);
    }

    [Fact]
    public async Task SendAsyncWhenClientAuthNotConfiguredAndAccessTokenIsNullThenThrowsCommunicationException()
    {
        var config = GetConfigurationWithoutAuthenticationAndNoAccessToken();
        var tokenCache = GetTokenCacheReturningValidApiToken();
        var usageDelegatingHandler = GetUsageDelegatingHandler(config, tokenCache);
        var httpMessageInvoker = new HttpMessageInvoker(usageDelegatingHandler);
        var httpRequestMessage = GetAnyHttpRequestMessage();

        var exception = await Should.ThrowAsync<CommunicationException>(() => httpMessageInvoker.SendAsync(httpRequestMessage, CancellationToken.None));

        exception.Message.ShouldBe("Token is not available. Unable to authenticate the request.");
        exception.ResponseCode.ShouldBe(HttpStatusCode.Unauthorized);
        exception.Url.ShouldBe("http://localhost/");
    }

    [Fact]
    public async Task SendAsyncWhenClientAuthNotConfiguredAndAccessTokenIsNullAndNoRequestUriThenThrowsCommunicationException()
    {
        var config = GetConfigurationWithoutAuthenticationAndNoAccessToken();
        var tokenCache = GetTokenCacheReturningValidApiToken();
        var usageDelegatingHandler = GetUsageDelegatingHandler(config, tokenCache);
        var httpMessageInvoker = new HttpMessageInvoker(usageDelegatingHandler);
        var httpRequestMessage = GetHttpRequestMessageWithoutUri();

        var exception = await Should.ThrowAsync<CommunicationException>(() => httpMessageInvoker.SendAsync(httpRequestMessage, CancellationToken.None));

        exception.Message.ShouldBe("Token is not available. Unable to authenticate the request.");
        exception.ResponseCode.ShouldBe(HttpStatusCode.Unauthorized);
        exception.Url.ShouldBeNull();
    }

    private static IUofConfiguration GetConfigurationWithoutAuthentication()
    {
        var config = TestConfiguration.GetConfig();
        return config;
    }

    private static IUofConfiguration GetConfigurationWithAuthentication()
    {
        var config = TestConfiguration.GetConfigWithCiam();
        return config;
    }

    private static IUofConfiguration GetConfigurationWithoutAuthenticationAndNoAccessToken()
    {
        // Create a mock configuration without authentication and without an access token
        var configMock = new Mock<IUofConfiguration>();
        configMock.Setup(c => c.Authentication).Returns((UofClientAuthentication.IPrivateKeyJwt)null);
        configMock.Setup(c => c.AccessToken).Returns((string)null);
        return configMock.Object;
    }

    private static IAuthenticationTokenCache GetTokenCacheReturningValidApiToken()
    {
        var authTokenCacheMock = new Mock<IAuthenticationTokenCache>();
        authTokenCacheMock.Setup(c => c.GetTokenForApi()).ReturnsAsync("valid-jwt-token-for-api");
        return authTokenCacheMock.Object;
    }

    private static IAuthenticationTokenCache GetTokenCacheReturningNullApiToken()
    {
        var authTokenCacheMock = new Mock<IAuthenticationTokenCache>();
        authTokenCacheMock.Setup(c => c.GetTokenForApi()).ReturnsAsync((string)null);
        return authTokenCacheMock.Object;
    }

    private static IAuthenticationTokenCache GetTokenCacheReturningEmptyApiToken()
    {
        var authTokenCacheMock = new Mock<IAuthenticationTokenCache>();
        authTokenCacheMock.Setup(c => c.GetTokenForApi()).ReturnsAsync(string.Empty);
        return authTokenCacheMock.Object;
    }

    private static HttpRequestMessage GetAnyHttpRequestMessage()
    {
        return new HttpRequestMessage(HttpMethod.Get, "http://localhost");
    }

    private static HttpRequestMessage GetHttpRequestMessageWithoutUri()
    {
        return new HttpRequestMessage(HttpMethod.Get, (Uri)null);
    }

    private static UsageDelegatingHandler GetUsageDelegatingHandler(IUofConfiguration config, IAuthenticationTokenCache tokenCache)
    {
        var usageTokenProvider = new UsageAuthenticationTokenProvider(config, tokenCache);
        return new UsageDelegatingHandler(usageTokenProvider)
        {
            InnerHandler = new TestHandler()
        };
    }

    private class TestHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
