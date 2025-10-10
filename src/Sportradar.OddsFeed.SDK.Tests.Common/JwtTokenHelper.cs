// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public static class JwtTestHelper
{
    private static readonly RsaSecurityKey SecurityKey = new(RSA.Create(2056));

    private const string Issuer = "test-issuer";
    private const string Audience = "test-audience";

    public static string CreateJwtThatExpiresIn(TimeSpan expiresIn)
    {
        var now = DateTime.UtcNow;

        List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Sub, "test-user"),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            ];

        var signingCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
                                         issuer: Issuer,
                                         audience: Audience,
                                         claims: claims,
                                         notBefore: now,
                                         expires: now.Add(expiresIn),
                                         signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static bool IsJwtExpired(string bearerToken)
    {
        if (string.IsNullOrWhiteSpace(bearerToken))
        {
            return true;
        }

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearerToken);

        var expClaim = jwt.Claims.FirstOrDefault(c => c.Type is JwtRegisteredClaimNames.Exp)?.Value;
        if (!long.TryParse(expClaim, out var expUnix))
        {
            return true;
        }

        var expUtc = DateTimeOffset.FromUnixTimeSeconds(expUnix);
        var nowUtc = DateTimeOffset.UtcNow;

        return expUtc <= nowUtc;
    }
}
