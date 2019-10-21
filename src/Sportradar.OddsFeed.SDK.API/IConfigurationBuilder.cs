/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.API
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
        IEnvironmentSelectorV1 SetAccessToken(string accessToken);

        /// <summary>
        /// Sets the access token used to access feed resources (AMQP broker, Sports API, ...) to value read from configuration file
        /// </summary>
        /// <returns>The <see cref="IEnvironmentSelector"/> instance allowing the selection of target environment</returns>
        IEnvironmentSelectorV1 SetAccessTokenFromConfigFile();
    }

    /// <summary>
    /// Defines a contract implemented by classes taking care of the 2nd step when building configuration - selecting the environment.
    /// </summary>
    public interface IEnvironmentSelector
    {
        /// <summary>
        /// Returns a <see cref="IConfigurationBuilder"/> with properties set to values needed to access integration environment
        /// </summary>
        /// <returns>A <see cref="IConfigurationBuilder"/> with properties set to values needed to access integration environment</returns>
        [Obsolete("Use IEnvironmentSelectorV1.SelectIntegration()")]
        IConfigurationBuilder SelectStaging();

        /// <summary>
        /// Returns a <see cref="IConfigurationBuilder"/> with properties set to values needed to access production environment
        /// </summary>
        /// <returns>A <see cref="IConfigurationBuilder"/> with properties set to values needed to access production environment</returns>
        IConfigurationBuilder SelectProduction();

        /// <summary>
        /// Returns a <see cref="IReplayConfigurationBuilder"/> with properties set to values needed to access replay server
        /// </summary>
        /// <returns>A <see cref="IReplayConfigurationBuilder"/> with properties set to values needed to access replay server</returns>
        IReplayConfigurationBuilder SelectReplay();

        /// <summary>
        /// Returns a <see cref="ICustomConfigurationBuilder"/> allowing the properties to be set to custom values (useful for testing with non-standard AMQP)
        /// </summary>
        /// <returns>A <see cref="ICustomConfigurationBuilder"/> with properties set to values needed to access replay server</returns>
        ICustomConfigurationBuilder SelectCustom();
    }

    /// <summary>
    /// Defines a contract implemented by classes taking care of the 2nd step when building configuration - selecting the environment.
    /// </summary>
    public interface IEnvironmentSelectorV1 : IEnvironmentSelector
    {
        /// <summary>
        /// Returns a <see cref="IConfigurationBuilder"/> with properties set to values needed to access integration environment
        /// </summary>
        /// <returns>A <see cref="IConfigurationBuilder"/> with properties set to values needed to access integration environment</returns>
        IConfigurationBuilder SelectIntegration();
    }

    /// <summary>
    /// A base contract custom and general configuration builders
    /// </summary>
    /// <typeparam name="T">The type of the builder extending the interface</typeparam>
    public interface IConfigurationBuilderBase<out T>
    {
        /// <summary>
        /// Sets the general configuration properties to values read from configuration file. Only value which can be set
        /// through <see cref="IConfigurationBuilderBase{T}"/> methods are set. Any values already set by methods on the current instance are overridden
        /// </summary>
        /// <returns>A <see cref="IConfigurationBuilderBase{T}"/> derived instance used to set general configuration properties</returns>
        T LoadFromConfigFile();

        /// <summary>
        /// Sets the languages in which translatable data is available
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{T}"/> specifying languages in which translatable data should be available</param>
        /// <returns>A <see cref="IConfigurationBuilderBase{T}"/> derived instance used to set general configuration properties</returns>
        T SetSupportedLanguages(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Sets the default language in which translatable data is available
        /// </summary>
        /// <param name="culture">A default language in which translatable data should be available</param>
        /// <returns>A <see cref="IConfigurationBuilderBase{T}"/> derived instance used to set general configuration properties</returns>
        T SetDefaultLanguage(CultureInfo culture);

        /// <summary>
        /// Sets the value specifying how exceptions thrown in the SDK are handled.
        /// </summary>
        /// <param name="strategy">A <see cref="ExceptionHandlingStrategy"/> enum specifying how exceptions thrown in the SDK are handled</param>
        /// <returns>A <see cref="IConfigurationBuilderBase{T}"/> derived instance used to set general configuration properties</returns>
        T SetExceptionHandlingStrategy(ExceptionHandlingStrategy strategy);

        /// <summary>
        /// Sets the node id used to separate between SDK instances associated with the same account
        /// </summary>
        /// <param name="nodeId">The node id to be set</param>
        /// <remarks>MTS customer must set this value! Use only positive numbers; negative are reserved for internal use.</remarks>
        /// <returns>A <see cref="IConfigurationBuilderBase{T}"/> derived instance used to set general configuration properties</returns>
        T SetNodeId(int nodeId);

        /// <summary>
        /// Specifies the producers which should be disabled (i.e. no recovery, ...)
        /// </summary>
        /// <param name="producerIds">The list of producer ids specifying the producers which should be disabled</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> derived instance used to set general configuration properties</returns>
        T SetDisabledProducers(IEnumerable<int> producerIds);

        /// <summary>
        /// Builds and returns a <see cref="IOddsFeedConfiguration"/> instance
        /// </summary>
        /// <returns>The constructed <see cref="IOddsFeedConfiguration"/> instance</returns>
        IOddsFeedConfiguration Build();
    }

    /// <summary>
    /// Defines a contract implemented by classes used to set recovery related configuration properties
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRecoveryConfigurationBuilder<out T> : IConfigurationBuilderBase<T>
    {
        /// <summary>
        /// Sets the max time window between two consecutive alive messages before the associated producer is marked as down
        /// </summary>
        /// <param name="inactivitySeconds">the max time window between two consecutive alive messages</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> derived instance used to set general configuration properties</returns>
        T SetInactivitySeconds(int inactivitySeconds);

        /// <summary>
        /// Sets the maximum time in seconds in which recovery must be completed (minimum 900 seconds)
        /// </summary>
        /// <param name="maxRecoveryTimeInSeconds">Maximum recovery time in seconds</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> instance used to set general configuration properties</returns>
        T SetMaxRecoveryTime(int maxRecoveryTimeInSeconds);

        /// <summary>
        /// Sets the value indicating whether the after age should be adjusted before executing recovery request
        /// </summary>
        /// <param name="adjustAfterAge">True if age should be adjusted; False otherwise</param>
        /// <returns>The <see cref="IRecoveryConfigurationBuilder{T}"/> instance used to set additional values</returns>
        T SetAdjustAfterAge(bool adjustAfterAge);
    }

    /// <summary>
    /// Defines a contract implemented by classes used to set general configuration properties
    /// </summary>
    /// <remarks>
    /// Types associated with <see cref="IConfigurationBuilder"/> represent a re-factored approach to building SDK configuration and
    /// therefore make <see cref="IConfigurationBuilder"/> related instances obsolete. The <see cref="IConfigurationBuilder"/>
    /// and related instances cannot be removed in order not to introduce braking changes.
    /// </remarks>
    public interface IConfigurationBuilder : IRecoveryConfigurationBuilder<IConfigurationBuilder>
    {

    }

    /// <summary>
    /// Defines a contract implemented by classes used to set replay configuration properties
    /// </summary>
    public interface IReplayConfigurationBuilder : IConfigurationBuilderBase<IReplayConfigurationBuilder>
    {

    }

    /// <summary>
    /// Defines a contract implemented by classes used to set general and custom configuration properties
    /// </summary>
    public interface ICustomConfigurationBuilder : IRecoveryConfigurationBuilder<ICustomConfigurationBuilder>
    {
        /// <summary>
        /// Set the host name of the Sports API server
        /// </summary>
        /// <param name="apiHost">The host name of the Sports API server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder"/> instance used to set custom config values</returns>
        ICustomConfigurationBuilder SetApiHost(string apiHost);

        /// <summary>
        /// Sets the host name of the AMQP server
        /// </summary>
        /// <param name="host">The host name of the AMQP server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder"/> instance used to set custom config values</returns>
        ICustomConfigurationBuilder SetMessagingHost(string host);

        /// <summary>
        /// Sets the virtual host name of the AMQP server
        /// </summary>
        /// <param name="virtualHost">The virtual host name of the AMQP server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder"/> instance used to set custom config values</returns>
        ICustomConfigurationBuilder SetVirtualHost(string virtualHost);

        /// <summary>
        /// Sets the port used to connect to the AMQP server
        /// </summary>
        /// <param name="port">The port used to connect to the AMQP server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder"/> instance used to set custom config values</returns>
        ICustomConfigurationBuilder SetMessagingPort(int port);

        /// <summary>
        /// Sets the username used to authenticate with the messaging server
        /// </summary>
        /// <param name="username">The username used to authenticate with the messaging server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder"/> instance used to set custom config values</returns>
        ICustomConfigurationBuilder SetMessagingUsername(string username);

        /// <summary>
        /// Sets the password used to authenticate with the messaging server
        /// </summary>
        /// <param name="password">The password used to authenticate with the messaging server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder"/> instance used to set custom config values</returns>
        ICustomConfigurationBuilder SetMessagingPassword(string password);

        /// <summary>
        /// Sets the value specifying whether SSL should be used to communicate with Sports API
        /// </summary>
        /// <param name="useApiSsl">The value specifying whether SSL should be used to communicate with Sports API</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder"/> instance used to set custom config values</returns>
        ICustomConfigurationBuilder UseApiSsl(bool useApiSsl);

        /// <summary>
        /// Sets the value specifying whether SSL should be used to communicate with the messaging server
        /// </summary>
        /// <param name="useMessagingSsl">The value specifying whether SSL should be used to communicate with the messaging server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder"/> instance used to set custom config values</returns>
        ICustomConfigurationBuilder UseMessagingSsl(bool useMessagingSsl);
    }
}
