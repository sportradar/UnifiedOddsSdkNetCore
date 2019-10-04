/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using Sportradar.OddsFeed.SDK.API.Contracts;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines method used to set access token property on the configuration builder
    /// </summary>
    [ContractClass(typeof(ConfigurationAccessTokenSetterContract))]
    [Obsolete("Use ITokenSetter")]
    public interface IConfigurationAccessTokenSetter
    {
        /// <summary>
        /// Sets the access token used for the authentication.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns>The <see cref="IConfigurationInactivitySecondsSetter"/> instance used to set additional values.</returns>
        /// <exception cref="System.ArgumentException">Value cannot be a null reference or empty string</exception>
        IConfigurationInactivitySecondsSetter SetAccessToken(string accessToken);
    }

    /// <summary>
    /// Defines method used to set inactivity seconds property on the configuration builder
    /// </summary>
    [ContractClass(typeof(ConfigurationInactivitySecondsSetterContract))]
    [Obsolete]
    public interface IConfigurationInactivitySecondsSetter
    {
        /// <summary>
        /// Sets the max time window between two messages before the producer is market as "down"
        /// </summary>
        /// <param name="inactivitySeconds">The inactivity seconds Value must be between 20 and 180.</param>
        /// <returns>The <see cref="IOddsFeedConfigurationBuilder"/> instance used to set additional values.</returns>
        /// <exception cref="ArgumentOutOfRangeException">inactivitySeconds &lt; 20 or inactivitySeconds &gt; 180</exception>
        IOddsFeedConfigurationBuilder SetInactivitySeconds(int inactivitySeconds);
    }

    /// <summary>
    /// Defines methods used to set optional properties on the configuration builder
    /// </summary>
    [ContractClass(typeof(OddsFeedConfigurationBuilderContract))]
    [Obsolete("Use IConfigurationBuilder")]
    public interface IOddsFeedConfigurationBuilder
    {
        /// <summary>
        /// Adds a provided <see cref="CultureInfo"/> into the list of default locales
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> representing the locale.</param>
        /// <returns>The <see cref="IOddsFeedConfigurationBuilder"/> instance used to set additional values.</returns>
        /// <exception cref="ArgumentNullException">A <code>culture</code> is a null reference</exception>
        IOddsFeedConfigurationBuilder AddLocale(CultureInfo culture);

        /// <summary>
        /// Removes the specified <see cref="CultureInfo"/> from the list of default locales
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> representing the locale to be removed.</param>
        /// <returns>The <see cref="IOddsFeedConfigurationBuilder"/> instance used to set additional values.</returns>
        /// <exception cref="ArgumentNullException">The <code>culture</code> is a null reference</exception>
        IOddsFeedConfigurationBuilder RemoveLocale(CultureInfo culture);

        /// <summary>
        /// Sets the URL of the API host
        /// </summary>
        /// <param name="apiHost">The URL of the API host.</param>
        /// <returns>The <see cref="IOddsFeedConfigurationBuilder"/> instance used to set additional values.</returns>
        IOddsFeedConfigurationBuilder SetApiHost(string apiHost);

        /// <summary>
        /// Sets the URL of the messaging host
        /// </summary>
        /// <param name="host">The URL of the messaging host.</param>
        /// <returns>The <see cref="IOddsFeedConfigurationBuilder"/> instance used to set additional values.</returns>
        IOddsFeedConfigurationBuilder SetHost(string host);

        /// <summary>
        /// Sets the name of the virtual host configured on the messaging server.
        /// </summary>
        /// <param name="virtualHost">The name virtual host.</param>
        /// <returns>The <see cref="IOddsFeedConfigurationBuilder"/> instance used to set additional values.</returns>
        IOddsFeedConfigurationBuilder SetVirtualHost(string virtualHost);

        /// <summary>
        /// Sets the value indicating whether a secure connection to the message broker should be used
        /// </summary>
        /// <param name="useSsl">True if secure connection should be used; False otherwise.</param>
        /// <returns>The <see cref="IOddsFeedConfigurationBuilder"/> instance used to set additional values.</returns>
        IOddsFeedConfigurationBuilder SetUseSsl(bool useSsl);

        /// <summary>
        /// Sets the maximum time in seconds in which recovery must be completed (minimum 900 seconds)
        /// </summary>
        /// <param name="maxRecoveryTime">Maximum recovery time</param>
        /// <returns>The <see cref="IOddsFeedConfigurationBuilder"/> instance used to set additional values.</returns>
        IOddsFeedConfigurationBuilder SetMaxRecoveryTime(int maxRecoveryTime);

        /// <summary>
        /// Sets a value indicating whether the SDK should connect to the integration environment
        /// </summary>
        /// <param name="useStagingEnvironment"></param>
        /// <returns>The <see cref="IOddsFeedConfigurationBuilder"/> instance used to set additional values.</returns>
        [Obsolete("Use SetUseIntegrationEnvironment(bool useIntegrationEnvironment)")]
        IOddsFeedConfigurationBuilder SetUseStagingEnvironment(bool useStagingEnvironment);

        /// <summary>
        /// Sets a value indicating whether the SDK should connect to the integration environment
        /// </summary>
        /// <param name="useIntegrationEnvironment"></param>
        /// <returns>The <see cref="IOddsFeedConfigurationBuilder"/> instance used to set additional values.</returns>
        IOddsFeedConfigurationBuilder SetUseIntegrationEnvironment(bool useIntegrationEnvironment);

        /// <summary>
        /// Sets the node id for this instance of the sdk
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <returns>The <see cref="IOddsFeedConfigurationBuilder"/> instance used to set additional values.</returns>
        IOddsFeedConfigurationBuilder SetNodeId(int nodeId);

        /// <summary>
        /// Builds and returns a <see cref="IOddsFeedConfiguration"/> instance
        /// </summary>
        /// <returns>The constructed <see cref="IOddsFeedConfiguration"/> instance.</returns>
        IOddsFeedConfiguration Build();
    }
}