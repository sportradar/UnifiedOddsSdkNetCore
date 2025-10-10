// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.IdentityModel.Tokens;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    /// <summary>
    /// Concrete implementation of Private Key JWT authentication configuration.
    /// </summary>
    internal sealed class PrivateKeyJwt : UofClientAuthentication.IPrivateKeyJwt
    {
        /// <summary>
        /// Internal constructor; use the builder to create instances.
        /// </summary>
        internal PrivateKeyJwt(string signingKeyId, string clientId, AsymmetricSecurityKey privateKey)
        {
            SigningKeyId = signingKeyId;
            ClientId = clientId;
            PrivateKey = privateKey;
        }

        /// <inheritdoc />
        public string SigningKeyId { get; }

        /// <inheritdoc />
        public string ClientId { get; }

        /// <inheritdoc />
        public AsymmetricSecurityKey PrivateKey { get; }

        /// <inheritdoc />
        public string Host { get; private set; }

        /// <inheritdoc />
        public int Port { get; private set; }

        /// <inheritdoc />
        public bool UseSsl { get; private set; }

        /// <summary>
        /// Sets the authentication server host.
        /// Intended to be called by your custom configuration builder.
        /// </summary>
        /// <param name="host">The host name.</param>
        internal void SetHost(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("Host can not be null or empty.", nameof(host));
            }

            Host = host;
        }

        /// <summary>
        /// Sets the authentication server port.
        /// Intended to be called by your custom configuration builder.
        /// </summary>
        /// <param name="port">Port in range 1..65535.</param>
        internal void SetPort(int port)
        {
            if (port <= 0 || port > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535.");
            }

            Port = port;
        }

        /// <summary>
        /// Sets whether SSL (HTTPS) should be used.
        /// Intended to be called by your custom configuration builder.
        /// </summary>
        /// <param name="useSsl">True to use HTTPS.</param>
        internal void SetUseSsl(bool useSsl)
        {
            UseSsl = useSsl;
        }

        /// <summary>
        /// Overrides <see cref="object.ToString()"/> to provide a sanitized string representation (sensitive values masked).
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Authentication{{ SigningKeyId={SdkInfo.ClearSensitiveData(SigningKeyId)}, ClientId={SdkInfo.ClearSensitiveData(ClientId)} }}";
        }

        /// <summary>
        /// Builder for <see cref="PrivateKeyJwt" /> instances.
        /// </summary>
        public sealed class Builder : UofClientAuthentication.IPrivateKeyJwtBuilder
        {
            private string _clientId;
            private AsymmetricSecurityKey _privateKey;
            private string _signingKeyId;

            /// <inheritdoc />
            public UofClientAuthentication.IPrivateKeyJwtBuilder SetSigningKeyId(string signingKeyId)
            {
                if (string.IsNullOrWhiteSpace(signingKeyId))
                {
                    throw new ArgumentException("Signing Key Id must not be null or whitespace.", nameof(signingKeyId));
                }

                _signingKeyId = signingKeyId;
                return this;
            }

            /// <inheritdoc />
            public UofClientAuthentication.IPrivateKeyJwtBuilder SetClientId(string clientId)
            {
                if (string.IsNullOrWhiteSpace(clientId))
                {
                    throw new ArgumentException("Client Id must not be null or whitespace.", nameof(clientId));
                }

                _clientId = clientId;
                return this;
            }

            /// <inheritdoc />
            public UofClientAuthentication.IPrivateKeyJwtBuilder SetPrivateKey(AsymmetricSecurityKey privateKey)
            {
                if (privateKey == null)
                {
                    throw new ArgumentException("Private key cannot be null", nameof(privateKey));
                }

                if (!(privateKey is RsaSecurityKey))
                {
                    throw new ArgumentException("The provided private key does not support RSA algorithms. Only RSA keys are supported.", nameof(privateKey));
                }

                _privateKey = privateKey;
                return this;
            }

            /// <inheritdoc />
            [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Easy to read")]
            public UofClientAuthentication.IPrivateKeyJwtData Build()
            {
                if (string.IsNullOrWhiteSpace(_signingKeyId))
                {
                    throw new InvalidOperationException("Signing key ID has not been set.");
                }

                if (string.IsNullOrWhiteSpace(_clientId))
                {
                    throw new InvalidOperationException("Client ID has not been set.");
                }

                return _privateKey == null
                           ? throw new InvalidOperationException("Private Key has not been set.")
                           : new PrivateKeyJwt(_signingKeyId, _clientId, _privateKey);
            }
        }
    }
}
