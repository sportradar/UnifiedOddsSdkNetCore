// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
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

public class AuthenticationDelegatingHandlerTests
{
    [Fact]
    public void ConstructorWhenTokenCacheIsNullThenThrowsArgumentNullException()
    {
        var config = GetConfigurationWithAuthentication();

        var exception = Should.Throw<ArgumentNullException>(() => new AuthenticationDelegatingHandler(config, null));

        exception.ParamName.ShouldBe("tokenCache");
    }

    [Fact]
    public async Task SendAsyncWhenClientAuthConfiguredAndTokenAvailableThenAddsAuthorizationHeader()
    {
        var config = GetConfigurationWithAuthentication();
        var tokenCache = GetTokenCacheReturningValidApiToken();
        var authenticationDelegatingHandler = GetAuthenticationDelegatingHandler(config, tokenCache);
        var httpMessageInvoker = new HttpMessageInvoker(authenticationDelegatingHandler);
        var httpRequestMessage = GetAnyHttpRequestMessage();

        await httpMessageInvoker.SendAsync(httpRequestMessage, CancellationToken.None);

        httpRequestMessage.Headers.Authorization.ShouldNotBeNull();
        httpRequestMessage.Headers.Authorization.Scheme.ShouldBe("Bearer");
        httpRequestMessage.Headers.Authorization.Parameter.ShouldBe("valid-jwt-token-for-api");
    }

    [Fact]
    public async Task SendAsyncWhenClientAuthConfiguredAndTokenIsNullThenThrowsCommunicationException()
    {
        var config = GetConfigurationWithAuthentication();
        var tokenCache = GetTokenCacheReturningNullApiToken();
        var authenticationDelegatingHandler = GetAuthenticationDelegatingHandler(config, tokenCache);
        var httpMessageInvoker = new HttpMessageInvoker(authenticationDelegatingHandler);
        var httpRequestMessage = GetAnyHttpRequestMessage();

        var exception = await Should.ThrowAsync<CommunicationException>(() => httpMessageInvoker.SendAsync(httpRequestMessage, CancellationToken.None));

        exception.Message.ShouldBe("Authentication token is not available. Unable to authenticate the request.");
        exception.ResponseCode.ShouldBe(HttpStatusCode.Unauthorized);
        exception.Url.ShouldBe("http://localhost/");
    }

    [Fact]
    public async Task SendAsyncWhenClientAuthConfiguredAndTokenIsNullAndNoAddressInRequestMessageThenThrowsCommunicationException()
    {
        var config = GetConfigurationWithAuthentication();
        var tokenCache = GetTokenCacheReturningNullApiToken();
        var authenticationDelegatingHandler = GetAuthenticationDelegatingHandler(config, tokenCache);
        var httpMessageInvoker = new HttpMessageInvoker(authenticationDelegatingHandler);
        var httpRequestMessage = GetHttpRequestMessageWithoutUri();

        var exception = await Should.ThrowAsync<CommunicationException>(() => httpMessageInvoker.SendAsync(httpRequestMessage, CancellationToken.None));

        exception.Message.ShouldBe("Authentication token is not available. Unable to authenticate the request.");
        exception.ResponseCode.ShouldBe(HttpStatusCode.Unauthorized);
        exception.Url.ShouldBeNull();
    }

    [Fact]
    public async Task SendAsyncWhenClientAuthConfiguredAndTokenIsEmptyThenThrowsCommunicationException()
    {
        var config = GetConfigurationWithAuthentication();
        var tokenCache = GetTokenCacheReturningEmptyApiToken();
        var authenticationDelegatingHandler = GetAuthenticationDelegatingHandler(config, tokenCache);
        var httpMessageInvoker = new HttpMessageInvoker(authenticationDelegatingHandler);
        var httpRequestMessage = GetAnyHttpRequestMessage();

        var exception = await Should.ThrowAsync<CommunicationException>(() => httpMessageInvoker.SendAsync(httpRequestMessage, CancellationToken.None));

        exception.Message.ShouldBe("Authentication token is not available. Unable to authenticate the request.");
        exception.ResponseCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendAsyncWhenClientAuthNotConfiguredAndAccessTokenAvailableThenAddsAccessTokenHeader()
    {
        var config = GetConfigurationWithoutAuthentication();
        var tokenCache = GetTokenCacheReturningValidApiToken();
        var authenticationDelegatingHandler = GetAuthenticationDelegatingHandler(config, tokenCache);
        var httpMessageInvoker = new HttpMessageInvoker(authenticationDelegatingHandler);
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
        var authenticationDelegatingHandler = GetAuthenticationDelegatingHandler(config, tokenCache);
        var httpMessageInvoker = new HttpMessageInvoker(authenticationDelegatingHandler);
        var httpRequestMessage = GetAnyHttpRequestMessage();

        var exception = await Should.ThrowAsync<CommunicationException>(() => httpMessageInvoker.SendAsync(httpRequestMessage, CancellationToken.None));

        exception.Message.ShouldBe("Access token is not available. Unable to authenticate the request.");
        exception.ResponseCode.ShouldBe(HttpStatusCode.Unauthorized);
        exception.Url.ShouldBe("http://localhost/");
    }

    [Fact]
    public async Task SendAsyncWhenClientAuthNotConfiguredAndAccessTokenIsNullAndNoRequestUriThenThrowsCommunicationException()
    {
        var config = GetConfigurationWithoutAuthenticationAndNoAccessToken();
        var tokenCache = GetTokenCacheReturningValidApiToken();
        var authenticationDelegatingHandler = GetAuthenticationDelegatingHandler(config, tokenCache);
        var httpMessageInvoker = new HttpMessageInvoker(authenticationDelegatingHandler);
        var httpRequestMessage = GetHttpRequestMessageWithoutUri();

        var exception = await Should.ThrowAsync<CommunicationException>(() => httpMessageInvoker.SendAsync(httpRequestMessage, CancellationToken.None));

        exception.Message.ShouldBe("Access token is not available. Unable to authenticate the request.");
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
        // Create a mock configuration without authentication and without access token
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

    private static AuthenticationDelegatingHandler GetAuthenticationDelegatingHandler(IUofConfiguration config, IAuthenticationTokenCache tokenCache)
    {
        return new AuthenticationDelegatingHandler(config, tokenCache)
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
