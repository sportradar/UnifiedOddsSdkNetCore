// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Authentication;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;

internal static class TokenCacheBuilderExtensions
{
    /// <summary>
    /// Uses a real FusionCache instance. Entry duration is set dynamically from the
    /// token <c>expires_in</c> by <see cref="Api.Internal.Authentication.AuthenticationTokenCache"/>.
    /// </summary>
    public static TokenCacheBuilder WithFusionCache(this TokenCacheBuilder builder)
    {
        return builder.ConfigureFusionCache();
    }

    public static TokenCacheBuilder WithJsonWebTokenFactoryUsingClientAuthentication(this TokenCacheBuilder builder, PrivateKeyJwt privateKeyJwt)
    {
        return builder.ConfigureJsonWebTokenFactoryUsingClientAuthentication(privateKeyJwt);
    }

    /// <summary>
    /// Real <see cref="System.Net.Http.HttpClient"/> whose BaseAddress is the given auth-server URL
    /// (provided by the test, typically a WireMock localhost URI).
    /// </summary>
    public static TokenCacheBuilder WithAuthenticationHost(this TokenCacheBuilder builder, Uri authenticationServerBaseAddress)
    {
        return builder.ConfigureHttpClientFor(authenticationServerBaseAddress);
    }

    public static TokenCacheBuilder WithXunitLogger(this TokenCacheBuilder builder, ITestOutputHelper outputHelper)
    {
        return builder.WithLoggerFactory(new XunitLoggerFactory(outputHelper));
    }
}
