/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using Sportradar.OddsFeed.SDK.API.Contracts;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines a contract implemented by classes representing odds feed configuration / settings
    /// </summary>
    [ContractClass(typeof(OddsFeedConfigurationContract))]
    public interface IOddsFeedConfiguration {

        /// <summary>
        /// Gets the access token used when accessing feed's REST interface
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// Gets the maximum allowed timeout in seconds, between consecutive AMQP messages associated with the same producer.
        /// If this value is exceeded, the producer is considered to be down
        /// </summary>
        int InactivitySeconds { get; }

        /// <summary>
        /// Gets a <see cref="CultureInfo"/> specifying default locale to which translatable values will be translated
        /// </summary>
        CultureInfo DefaultLocale { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{CultureInfo}"/> specifying locales (languages) to which translatable values will be translated
        /// </summary>
        IEnumerable<CultureInfo> Locales { get; }

        /// <summary>
        /// Gets the comma delimited list of ids of disabled producers (default: none)
        /// </summary>
        /// <remarks></remarks>
        /// <value>The list of ids of disabled producers</value>
        IEnumerable<int> DisabledProducers { get; }

        /// <summary>
        /// Gets the maximum recovery time
        /// </summary>
        /// <value>The maximum recovery time</value>
        int MaxRecoveryTime { get; }

        /// <summary>
        /// Gets the node identifier
        /// </summary>
        /// <remarks>MTS customer must set this value! Use only positive numbers; negative are reserved for internal use.</remarks>
        int NodeId { get; }

        /// <summary>
        /// Gets the <see cref="SdkEnvironment"/> value specifying the environment to which to connect.
        /// </summary>
        SdkEnvironment Environment { get; }

        /// <summary>
        /// Gets the exception handling strategy
        /// </summary>
        ExceptionHandlingStrategy ExceptionHandlingStrategy { get; }

        /// <summary>
        /// Gets a value specifying the host name of the AQMP broker
        /// </summary>
        string Host { get; }

        /// <summary>
        /// Gets the port used for connecting to the AQMP broker
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets the user name for connecting to the AQMP broker
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Gets the password for connecting to the AQMP broker
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Gets a value specifying the virtual host of the AQMP broker
        /// </summary>
        string VirtualHost { get; }

        /// <summary>
        /// Gets a value specifying whether the connection to AMQP broker should use SSL encryption
        /// </summary>
        bool UseSsl { get; }

        /// <summary>
        /// Gets a host name of the Sports API
        /// </summary>
        string ApiHost { get; }

        /// <summary>
        /// Gets a value indicating whether the connection to Sports API should use SSL
        /// </summary>
        bool UseApiSsl { get; }

        /// <summary>
        /// Gets a value indicating whether the after age should be adjusted before executing recovery request
        /// </summary>
        /// <value><c>true</c> if [adjust after age]; otherwise, <c>false</c></value>
        bool AdjustAfterAge { get; }
    }
}