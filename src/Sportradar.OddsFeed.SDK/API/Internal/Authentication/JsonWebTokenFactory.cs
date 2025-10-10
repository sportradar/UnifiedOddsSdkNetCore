// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Sportradar.OddsFeed.SDK.Api.Config;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Authentication
{
    internal class JsonWebTokenFactory : IJsonWebTokenFactory
    {
        private const int DefaultTokenExpiryInSec = 60;

        private readonly UofClientAuthentication.IPrivateKeyJwt _privateKeyJwt;
        private readonly string _audienceForLocalToken;

        public JsonWebTokenFactory(IUofConfiguration uofConfiguration)
        {
            if (uofConfiguration == null)
            {
                throw new ArgumentNullException(nameof(uofConfiguration));
            }
            _privateKeyJwt = uofConfiguration.Authentication;
            _audienceForLocalToken = uofConfiguration.Authentication.GetAudienceForLocalToken();
        }

        public string CreateJsonWebToken()
        {
            var jwt = CreateJwtSecurityToken();

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        private JwtSecurityToken CreateJwtSecurityToken()
        {
            var issuedAt = DateTimeOffset.UtcNow;
            var expiresAt = issuedAt.AddSeconds(DefaultTokenExpiryInSec);
            var issuedAtUnixSeconds = issuedAt.ToUnixTimeSeconds();

            var signingCredentials = new SigningCredentials(_privateKeyJwt.PrivateKey, SecurityAlgorithms.RsaSha256);

            var claims = new[]
                             {
                                 new Claim(JwtRegisteredClaimNames.Sub, _privateKeyJwt.ClientId),
                                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                 new Claim(JwtRegisteredClaimNames.Iat, issuedAtUnixSeconds.ToString(), ClaimValueTypes.Integer64)
                             };

            return new JwtSecurityToken(_privateKeyJwt.ClientId,
                                        _audienceForLocalToken, // based on configuration always full authentication server url
                                        claims,
                                        issuedAt.UtcDateTime,
                                        expiresAt.UtcDateTime,
                                        signingCredentials);
        }
    }
}
