// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Text.Json;
using Sportradar.OddsFeed.SDK.Api.Internal.Authentication;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Tests.Common.Helpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Authentication;

/// <summary>
/// WireMock stubs for the Common IAM <c>/oauth/token</c> endpoint used by <see cref="AuthenticationClient"/>.
/// </summary>
internal static class CommonIamTokenEndpoint
{
    private const string TokenPath = "/oauth/token";

    public static void StubReturningAccessTokenWhenAssertionBuiltWithPrivateKey(WireMockServer wireMockServer,
        PrivateKeyJwt privateKeyJwt,
        string commonIAmToken,
        int expiresInSeconds)
    {
        var tokenResponse = new AuthenticationTokenApiResponse
        {
            AccessToken = commonIAmToken,
            ExpiresIn = expiresInSeconds,
            TokenType = "Bearer"
        };

        wireMockServer.Given(Request.Create()
                                    .UsingPost()
                                    .WithPath(TokenPath)
                                    .WithBody(body => AssertionWasBuiltWithJwtFactoryUsingPrivateKey(body, privateKeyJwt)))
                      .RespondWith(Response.Create()
                                           .WithStatusCode(200)
                                           .WithHeader("Content-Type", "application/json")
                                           .WithBody(JsonSerializer.Serialize(tokenResponse)));
    }

    public static void StubReturningUnauthorizedWhenAssertionSigningKeyIdDoesNotMatch(WireMockServer wireMockServer,
        PrivateKeyJwt registeredPrivateKeyJwt)
    {
        wireMockServer.Given(Request.Create()
                                    .UsingPost()
                                    .WithPath(TokenPath)
                                    .WithBody(body => AssertionMatchesClientButNotSigningKeyId(body, registeredPrivateKeyJwt)))
                      .RespondWith(Response.Create()
                                           .WithStatusCode(401)
                                           .WithHeader("Content-Type", "application/json")
                                           .WithBody("{\"error\":\"invalid_client\",\"error_description\":\"Unauthorized\"}"));
    }

    private static bool AssertionWasBuiltWithJwtFactoryUsingPrivateKey(string commonIAmBody, PrivateKeyJwt privateKeyJwt)
    {
        var assertionJwt = CommonIAmRequestBodyHelper.ExtractClientAssertionFrom(commonIAmBody);
        if (string.IsNullOrEmpty(assertionJwt))
        {
            return false;
        }

        try
        {
            var issuer = CommonIAmJwtHelper.GetIssuer(assertionJwt);
            var subject = CommonIAmJwtHelper.GetSubject(assertionJwt);
            if (issuer != privateKeyJwt.ClientId || subject != privateKeyJwt.ClientId)
            {
                return false;
            }

            var kid = CommonIAmJwtHelper.GetSigningKeyId(assertionJwt);
            return kid == privateKeyJwt.SigningKeyId;
        }
        catch
        {
            return false;
        }
    }

    private static bool AssertionMatchesClientButNotSigningKeyId(string commonIAmBody, PrivateKeyJwt registeredPrivateKeyJwt)
    {
        var assertionJwt = CommonIAmRequestBodyHelper.ExtractClientAssertionFrom(commonIAmBody);
        if (string.IsNullOrEmpty(assertionJwt))
        {
            return false;
        }

        try
        {
            var issuer = CommonIAmJwtHelper.GetIssuer(assertionJwt);
            var subject = CommonIAmJwtHelper.GetSubject(assertionJwt);
            if (issuer != registeredPrivateKeyJwt.ClientId || subject != registeredPrivateKeyJwt.ClientId)
            {
                return false;
            }

            var kid = CommonIAmJwtHelper.GetSigningKeyId(assertionJwt);
            return kid != null && kid != registeredPrivateKeyJwt.SigningKeyId;
        }
        catch
        {
            return false;
        }
    }
}
