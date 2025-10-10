// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Authentication;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Authentication;

public class AuthenticationTokenExtensionsTests
{
    [Fact]
    public void IsValidWhenAuthenticationTokenIsValidThenReturnsTrue()
    {
        var token = GetValidToken();

        var result = token.IsValid();

        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValidWhenAuthenticationTokenHasEmptyAccessTokenThenReturnsFalse()
    {
        var token = new AuthenticationToken(CreateTokenResponse(""))
        {
            ExpiresAt = DateTime.Now.AddMinutes(10)
        };

        var result = token.IsValid();

        token.AccessToken.ShouldBeEmpty();
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValidWhenAuthenticationTokenHasNullAccessTokenThenReturnsFalse()
    {
        var token = new AuthenticationToken(CreateTokenResponse(null))
        {
            ExpiresAt = DateTime.Now.AddMinutes(10)
        };

        var result = token.IsValid();

        token.AccessToken.ShouldBeNull();
        result.ShouldBeFalse();
    }

    [Fact]
    public void ShouldBeFetchedWhenRefetchStartAtIsPastThenReturnsTrue()
    {
        var token = GetValidToken();
        token.RefetchStartAt = DateTime.Now.AddMinutes(-1);

        var result = token.ShouldBeFetched();

        result.ShouldBeTrue();
    }

    [Fact]
    public void ShouldBeFetchedWhenRefetchStartAtIsFutureThenReturnsFalse()
    {
        TimeProviderAccessor.SetTimeProvider(new RealTimeProvider());
        var token = GetValidToken();
        token.RefetchStartAt = DateTime.Now.AddMinutes(1);

        var result = token.ShouldBeFetched();

        result.ShouldBeFalse();
    }

    [Fact]
    public void ShouldBeFetchedWhenRefetchStartAtIsNullThenReturnsFalse()
    {
        var token = GetValidToken();
        token.RefetchStartAt = null;

        var result = token.ShouldBeFetched();

        result.ShouldBeFalse();
    }

    [Fact]
    public void ShouldBeFetchedWhenRefetchStartAtIsNowThenReturnsFalse()
    {
        var fakeTimeProvider = new FakeTimeProvider(DateTime.Now);
        TimeProviderAccessor.SetTimeProvider(fakeTimeProvider);
        var token = GetValidToken();
        token.RefetchStartAt = TimeProviderAccessor.Current.Now;

        var result = token.ShouldBeFetched();

        result.ShouldBeFalse();
        TimeProviderAccessor.SetTimeProvider(new RealTimeProvider());
    }

    [Fact]
    public void IsValidWhenAuthenticationTokenApiResponseIsValidThenReturnsTrue()
    {
        var response = CreateTokenResponse("token");

        var result = response.IsValid();

        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValidWhenAuthenticationTokenApiResponseHasNoAccessTokenThenReturnsFalse()
    {
        var response = CreateTokenResponse("");

        var result = response.IsValid();

        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValidWhenAuthenticationTokenApiResponseExpiresInIsZeroThenReturnsFalse()
    {
        var response = CreateTokenResponse("token");
        response.ExpiresIn = 0;

        var result = response.IsValid();

        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValidWhenAuthenticationTokenApiResponseExpiresInIsNegativeThenReturnsFalse()
    {
        var response = CreateTokenResponse("token");
        response.ExpiresIn = -1;

        var result = response.IsValid();

        result.ShouldBeFalse();
    }

    [Fact]
    public void GetAudienceForLocalTokenWhenPrivateKeyJwtIsNullThenReturnsNull()
    {
        var result = ((UofClientAuthentication.IPrivateKeyJwt)null).GetAudienceForLocalToken();

        result.ShouldBeNull();
    }

    [Fact]
    public void GetAudienceForLocalTokenWhenUseSslIsTrueThenReturnsHttpsUrl()
    {
        var jwt = GetPrivateKeyJwt();
        jwt.SetUseSsl(true);
        jwt.SetHost("localhost");

        var result = jwt.GetAudienceForLocalToken();

        result.ShouldBe("https://localhost/");
    }

    [Fact]
    public void GetAudienceForLocalTokenWhenUseSslIsFalseThenReturnsHttpUrl()
    {
        var jwt = GetPrivateKeyJwt();
        jwt.SetUseSsl(false);
        jwt.SetHost("localhost");

        var result = jwt.GetAudienceForLocalToken();

        result.ShouldBe("http://localhost/");
    }

    [Fact]
    public void GetAudienceForLocalTokenWhenHostIsNullThenThrow()
    {
        var jwt = GetPrivateKeyJwt();
        jwt.SetUseSsl(false);

        jwt.Host.ShouldBeNull();
        var exception = Should.Throw<ArgumentException>(() => jwt.GetAudienceForLocalToken());

        exception.Message.ShouldContain("Host is not configured");
    }

    [Theory]
    [InlineData(80, "http://localhost/")]
    [InlineData(443, "http://localhost/")]
    [InlineData(8080, "http://localhost:8080/")]
    public void GetAudienceForLocalTokenWhenPortIsSetThenReturnsValidAudience(int port, string expectedAudience)
    {
        var jwt = GetPrivateKeyJwt();
        jwt.SetUseSsl(false);
        jwt.SetHost("localhost");
        jwt.SetPort(port);

        var result = jwt.GetAudienceForLocalToken();

        result.ShouldBe(expectedAudience);
    }

    [Fact]
    public void GetAudienceForLocalTokenWhenPortIsZeroThenReturnsEmptyToken()
    {
        var jwt = GetPrivateKeyJwt();
        jwt.SetUseSsl(false);
        jwt.SetHost("localhost");

        var result = jwt.GetAudienceForLocalToken();

        result.ShouldBe("http://localhost/");
    }

    [Fact]
    public void IsValidWhenAuthenticationTokenHasNullAccessTokenAndNoExpiresAtThenReturnsFalse()
    {
        var token = new AuthenticationToken(CreateTokenResponse(null))
        {
            ExpiresAt = null
        };

        var result = token.IsValid();

        result.ShouldBeFalse();
    }

    [Fact]
    public void ShouldBeFetchedWhenRefetchStartAtIsNullAndCurrentTimeIsSetThenReturnsFalse()
    {
        var fakeTimeProvider = new FakeTimeProvider(DateTime.Now.AddHours(1));
        TimeProviderAccessor.SetTimeProvider(fakeTimeProvider);
        var token = GetValidToken();
        token.RefetchStartAt = null;

        var result = token.ShouldBeFetched();

        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValidWhenAuthenticationTokenHasEmptyAccessTokenAndValidExpiresAtThenReturnsFalse()
    {
        var fakeTimeProvider = new FakeTimeProvider(DateTime.Now);
        TimeProviderAccessor.SetTimeProvider(fakeTimeProvider);

        var token = new AuthenticationToken(CreateTokenResponse(""))
        {
            ExpiresAt = TimeProviderAccessor.Current.Now.AddMinutes(10)
        };

        var result = token.IsValid();

        result.ShouldBeFalse();
        TimeProviderAccessor.SetTimeProvider(new RealTimeProvider());
    }

    [Fact]
    public void ShouldBeFetchedWhenRefetchStartAtEqualsCurrentTimeThenReturnsFalse()
    {
        var currentTime = DateTime.Now;
        var fakeTimeProvider = new FakeTimeProvider(currentTime);
        TimeProviderAccessor.SetTimeProvider(fakeTimeProvider);

        var token = GetValidToken();
        token.RefetchStartAt = currentTime;

        var result = token.ShouldBeFetched();

        result.ShouldBeFalse();
        TimeProviderAccessor.SetTimeProvider(new RealTimeProvider());
    }

    private static AuthenticationToken GetValidToken()
    {
        return new AuthenticationToken(CreateTokenResponse("any-token"));
    }

    private static AuthenticationTokenApiResponse CreateTokenResponse(string accessToken, int expiresIn = 3600, string tokenType = "Bearer")
    {
        return new AuthenticationTokenApiResponse
        {
            AccessToken = accessToken,
            ExpiresIn = expiresIn,
            TokenType = tokenType
        };
    }

    private static PrivateKeyJwt GetPrivateKeyJwt()
    {
        var testPrivateKey = new RsaSecurityKey(RSA.Create(2056));
        return new PrivateKeyJwt("signing-key", "client-id", testPrivateKey);
    }
}
