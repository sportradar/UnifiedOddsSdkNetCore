// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Authentication;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using ZiggyCreatures.Caching.Fusion;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Authentication;

internal sealed class TokenCacheBuilder
{
    private const string DefaultHttpClientName = "AuthHttpClient";

    private IFusionCacheProvider _fusionCacheProvider;
    private IAuthenticationClient _authenticationClient;
    private IJsonWebTokenFactory _jsonWebTokenFactory;
    private ILoggerFactory _loggerFactory;
    private ILogger<AuthenticationTokenCache> _logger;
    private PrivateKeyJwt _privateKeyJwt;
    private Uri _authenticationServerBaseAddress;

    public static TokenCacheBuilder StubbedOutDependencies()
    {
        return new TokenCacheBuilder()
               .WithFusionCacheProvider(CreateUnconfiguredFusionCacheProvider())
               .WithAuthenticationClient(CreateUnconfiguredAuthenticationClient())
               .WithJsonWebTokenFactory(CreateUnconfiguredJsonWebTokenFactory())
               .WithLoggerFactory(NullLoggerFactory.Instance);
    }

    public TokenCacheBuilder WithFusionCacheProvider(IFusionCacheProvider fusionCacheProvider)
    {
        _fusionCacheProvider = fusionCacheProvider;
        return this;
    }

    public TokenCacheBuilder ConfigureFusionCache()
    {
        _fusionCacheProvider = CreateFusionCacheProvider();
        return this;
    }

    public TokenCacheBuilder WithAuthenticationClient(IAuthenticationClient authenticationClient)
    {
        _authenticationClient = authenticationClient;
        return this;
    }

    public TokenCacheBuilder WithJsonWebTokenFactory(IJsonWebTokenFactory jsonWebTokenFactory)
    {
        _jsonWebTokenFactory = jsonWebTokenFactory;
        return this;
    }

    public TokenCacheBuilder WithLoggerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        _logger = _loggerFactory.CreateLogger<AuthenticationTokenCache>();
        return this;
    }

    public TokenCacheBuilder WithLogger(ILogger<AuthenticationTokenCache> logger)
    {
        _logger = logger;
        return this;
    }

    public TokenCacheBuilder ConfigureJsonWebTokenFactoryUsingClientAuthentication(PrivateKeyJwt privateKeyJwt)
    {
        _privateKeyJwt = privateKeyJwt ?? throw new ArgumentNullException(nameof(privateKeyJwt));
        AlignPrivateKeyJwtHostWithAuthenticationServer();

        var configuration = new Mock<IUofConfiguration>();
        configuration.SetupGet(x => x.Authentication).Returns(_privateKeyJwt);
        _jsonWebTokenFactory = new JsonWebTokenFactory(configuration.Object);
        return this;
    }

    /// <summary>
    /// Builds a real <see cref="AuthenticationClient"/> whose <see cref="HttpClient"/> BaseAddress is
    /// <paramref name="authenticationServerBaseAddress"/> (e.g. a locally running WireMock URL provided by the test).
    /// </summary>
    public TokenCacheBuilder ConfigureHttpClientFor(Uri authenticationServerBaseAddress)
    {
        _authenticationServerBaseAddress = authenticationServerBaseAddress
                                           ?? throw new ArgumentNullException(nameof(authenticationServerBaseAddress));
        if (!_authenticationServerBaseAddress.IsAbsoluteUri)
        {
            throw new ArgumentException("Authentication server base address must be absolute.", nameof(authenticationServerBaseAddress));
        }

        AlignPrivateKeyJwtHostWithAuthenticationServer();

        var baseAddress = EnsureTrailingSlash(_authenticationServerBaseAddress);
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
                         .Returns(() => new HttpClient { BaseAddress = baseAddress });

        _authenticationClient = new AuthenticationClient(httpClientFactory.Object, DefaultHttpClientName, _loggerFactory);
        return this;
    }

    public AuthenticationTokenCache Build()
    {
        return new AuthenticationTokenCache(_fusionCacheProvider, _authenticationClient, _jsonWebTokenFactory, _logger);
    }

    private void AlignPrivateKeyJwtHostWithAuthenticationServer()
    {
        if (_privateKeyJwt == null || _authenticationServerBaseAddress == null)
        {
            return;
        }

        _privateKeyJwt.SetHost(_authenticationServerBaseAddress.Host);
        _privateKeyJwt.SetPort(_authenticationServerBaseAddress.Port);
        _privateKeyJwt.SetUseSsl(_authenticationServerBaseAddress.Scheme == Uri.UriSchemeHttps);
    }

    private static IFusionCacheProvider CreateFusionCacheProvider()
    {
        var fusionCache = new FusionCache(new FusionCacheOptions());
        var provider = new Mock<IFusionCacheProvider>();
        provider.Setup(x => x.GetCache(It.IsAny<string>())).Returns(fusionCache);
        return provider.Object;
    }

    private static IFusionCacheProvider CreateUnconfiguredFusionCacheProvider()
    {
        var provider = new Mock<IFusionCacheProvider>();
        provider.Setup(x => x.GetCache(It.IsAny<string>()))
                .Throws(new InvalidOperationException("FusionCache was not configured on TokenCacheBuilder. Call WithFusionCache()."));
        return provider.Object;
    }

    private static IJsonWebTokenFactory CreateUnconfiguredJsonWebTokenFactory()
    {
        var factory = new Mock<IJsonWebTokenFactory>();
        factory.Setup(x => x.CreateJsonWebToken())
               .Throws(new InvalidOperationException("JsonWebTokenFactory was not configured on TokenCacheBuilder. Call WithJsonWebTokenFactoryUsingClientAuthentication(...)."));
        return factory.Object;
    }

    private static IAuthenticationClient CreateUnconfiguredAuthenticationClient()
    {
        var client = new Mock<IAuthenticationClient>();
        client.Setup(x => x.RequestAccessTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>()))
              .ThrowsAsync(new InvalidOperationException("AuthenticationClient was not configured on TokenCacheBuilder. Call WithAuthenticationHttpClientPointingTo(...)."));
        return client.Object;
    }

    private static Uri EnsureTrailingSlash(Uri baseAddress)
    {
        var absolute = baseAddress.AbsoluteUri;
        if (!absolute.EndsWith('/'))
        {
            absolute += "/";
        }

        return new Uri(absolute);
    }
}
