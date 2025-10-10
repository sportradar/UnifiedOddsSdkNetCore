// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Microsoft.IdentityModel.Tokens;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;

namespace Sportradar.OddsFeed.SDK.Api.Config
{
    /// <summary>
    /// Static factory and contracts for UOF client authentication.
    /// </summary>
    public static class UofClientAuthentication
    {
        /// <summary>
        /// Returns a builder for <see cref="IPrivateKeyJwt" />
        /// </summary>
        public static IPrivateKeyJwtBuilder PrivateKeyJwt()
        {
            return new PrivateKeyJwt.Builder();
        }

        /// <summary>
        /// Authentication configuration for the UOF SDK using the Private Key JWT method.
        /// This corresponds to OAuth 2.0 Client Credentials with a JWT assertion signed by a private key.
        /// </summary>
        public interface IPrivateKeyJwtData
        {
            /// <summary>
            /// The key ID (KID) used to sign the JWT.
            /// </summary>
            string SigningKeyId { get; }

            /// <summary>
            /// The client identifier used for authentication (also used in JWT "iss" and "sub" claims).
            /// </summary>
            string ClientId { get; }

            /// <summary>
            /// The private key used to sign the JWT.
            /// </summary>
            AsymmetricSecurityKey PrivateKey { get; }
        }

        /// <summary>
        /// Authentication configuration for the UOF SDK using the Private Key JWT method.
        /// This corresponds to OAuth 2.0 Client Credentials with a JWT assertion signed by a private key.
        /// </summary>
        public interface IPrivateKeyJwt : IPrivateKeyJwtData
        {
            /// <summary>
            /// The host name of the authentication server.
            /// </summary>
            string Host { get; }

            /// <summary>
            /// The port number of the authentication server.
            /// </summary>
            int Port { get; }

            /// <summary>
            /// Indicates whether SSL (HTTPS) is used to communicate with the authentication server.
            /// </summary>
            bool UseSsl { get; }
        }

        /// <summary>
        /// Builder for creating <see cref="IPrivateKeyJwt" /> instances.
        /// </summary>
        public interface IPrivateKeyJwtBuilder
        {
            /// <summary>
            /// Sets the signing key identifier (KID).
            /// </summary>
            /// <param name="signingKeyId">The signing key ID.</param>
            /// <returns>The same builder instance.</returns>
            IPrivateKeyJwtBuilder SetSigningKeyId(string signingKeyId);

            /// <summary>
            /// Sets the client identifier.
            /// </summary>
            /// <param name="clientId">The client ID.</param>
            /// <returns>The same builder instance.</returns>
            IPrivateKeyJwtBuilder SetClientId(string clientId);

            /// <summary>
            /// Sets the private key used to sign the JWT.
            /// </summary>
            /// <param name="privateKey">The private key.</param>
            /// <returns>The same builder instance.</returns>
            IPrivateKeyJwtBuilder SetPrivateKey(AsymmetricSecurityKey privateKey);

            /// <summary>
            /// Builds the configured <see cref="IPrivateKeyJwt" /> instance.
            /// </summary>
            IPrivateKeyJwtData Build();
        }
    }
}
