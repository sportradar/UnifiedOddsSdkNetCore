// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Authentication;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Authentication;
using WireMock.Server;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Authentication;

public class AuthenticationTokenCacheUnitTests : IAsyncLifetime
{
    private const string TestClientId = "test-client-id";
    private const string SigningKeyId = "signing-key-id-A";
    private const string CommonIAmToken = "test-common-iam-token";
    private const int TokenExpiresInSeconds = 30;

    private static readonly AsymmetricSecurityKey TestPrivateKey = new RsaSecurityKey(RSA.Create(2048));

    private readonly ITestOutputHelper _outputHelper;
    private WireMockServer _wireMockServer;
    private Uri _wiremockUri;

    public AuthenticationTokenCacheUnitTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    public Task InitializeAsync()
    {
        _wireMockServer = WireMockServer.Start();
        _wiremockUri = new Uri(_wireMockServer.Url!);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _wireMockServer?.Stop();
        _wireMockServer?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetTokenForApiWhenAssertionUsesDifferentSigningKeyIdThanRegisteredThenReturnsNull()
    {
        var registeredPrivateKeyJwt = CreatePrivateKeyJwt(SigningKeyId);
        const string incorrectSigningKeyId = SigningKeyId + "-incorrect-signing-key-id-part";
        var requestingPrivateKeyJwt = CreatePrivateKeyJwt(incorrectSigningKeyId);

        CommonIamTokenEndpoint.StubReturningUnauthorizedWhenAssertionSigningKeyIdDoesNotMatch(
            _wireMockServer,
            registeredPrivateKeyJwt);

        var authTokenCache = TokenCacheBuilder.StubbedOutDependencies()
                                              .WithXunitLogger(_outputHelper)
                                              .WithFusionCache()
                                              .WithAuthenticationHost(_wiremockUri)
                                              .WithJsonWebTokenFactoryUsingClientAuthentication(requestingPrivateKeyJwt)
                                              .Build();

        var token = await authTokenCache.GetTokenForApi();

        token.ShouldBeNull();
        _wireMockServer.LogEntries.ShouldHaveSingleItem();
        _wireMockServer.LogEntries[0].ResponseMessage.StatusCode.ShouldBe(401);
    }

    [Fact]
    public async Task GetTokenForApiWhenAssertionUsesRegisteredSigningKeyIdThenReturnsAccessToken()
    {
        var privateKeyJwt = CreatePrivateKeyJwt(SigningKeyId);

        CommonIamTokenEndpoint.StubReturningAccessTokenWhenAssertionBuiltWithPrivateKey(
            _wireMockServer,
            privateKeyJwt,
            CommonIAmToken,
            TokenExpiresInSeconds);

        var authTokenCache = TokenCacheBuilder.StubbedOutDependencies()
                                              .WithXunitLogger(_outputHelper)
                                              .WithFusionCache()
                                              .WithAuthenticationHost(_wiremockUri)
                                              .WithJsonWebTokenFactoryUsingClientAuthentication(privateKeyJwt)
                                              .Build();

        var token = await authTokenCache.GetTokenForApi();

        token.ShouldBe(CommonIAmToken);
        _wireMockServer.LogEntries.ShouldHaveSingleItem();
        _wireMockServer.LogEntries[0].ResponseMessage.StatusCode.ShouldBe(200);
    }

    private static PrivateKeyJwt CreatePrivateKeyJwt(string signingKeyId)
    {
        return (PrivateKeyJwt)UofClientAuthentication.PrivateKeyJwt()
                                                     .SetSigningKeyId(signingKeyId)
                                                     .SetClientId(TestClientId)
                                                     .SetPrivateKey(TestPrivateKey)
                                                     .Build();
    }
}
