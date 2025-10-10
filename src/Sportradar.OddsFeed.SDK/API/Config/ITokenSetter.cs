// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Api.Config
{
    /// <summary>
    /// Defines a contract implemented by classes taking care of the 1st step when building configuration - setting the token
    /// </summary>
    public interface ITokenSetter
    {
        /// <summary>
        /// Sets the access token used to access feed resources (AMQP broker, Sports API, ...)
        /// </summary>
        /// <param name="accessToken">The access token used to access feed resources</param>
        /// <returns>The <see cref="IEnvironmentSelector"/> instance allowing the selection of target environment</returns>
        IEnvironmentSelector SetAccessToken(string accessToken);

        /// <summary>
        /// Sets the authentication configuration used to access feed resources.
        /// </summary>
        /// <param name="privateKeyJwtData">The authentication configuration containing credentials.</param>
        /// <returns>
        /// The <see cref="ITokenSetter"/> instance allowing chaining of calls.
        /// </returns>
        ITokenSetter SetClientAuthentication(UofClientAuthentication.IPrivateKeyJwtData privateKeyJwtData);

        /// <summary>
        /// Sets the access token used to access feed resources (AMQP broker, Sports API, ...) to value read from configuration file
        /// </summary>
        /// <returns>The <see cref="IEnvironmentSelector"/> instance allowing the selection of target environment</returns>
        IEnvironmentSelector SetAccessTokenFromConfigFile();

        /// <summary>
        /// Gets the configuration properties from configuration file. Any values already set by methods on the current instance are overridden. Builds and returns a <see cref="IUofConfiguration"/> instance.
        /// </summary>
        /// <returns>The constructed <see cref="IUofConfiguration"/> instance</returns>
        IUofConfiguration BuildFromConfigFile();
    }
}
