// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Helpers;

internal static class CommonIAmJwtHelper
{
    public static string GetOnlyAudience(string jwtToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwtToken);
        var audiences = token.Audiences.ToArray();
        if (audiences.Length == 1)
        {
            return audiences[0];
        }

        throw new ArgumentException("JWT token does not contain exactly one audience", nameof(jwtToken));
    }

    public static string GetIssuer(string jwtToken)
    {
        return new JwtSecurityTokenHandler().ReadJwtToken(jwtToken).Issuer;
    }

    public static string GetSubject(string jwtToken)
    {
        return new JwtSecurityTokenHandler().ReadJwtToken(jwtToken).Subject;
    }

    public static string GetSigningKeyId(string jwtToken)
    {
        return new JwtSecurityTokenHandler().ReadJwtToken(jwtToken).Header.Kid;
    }
}
