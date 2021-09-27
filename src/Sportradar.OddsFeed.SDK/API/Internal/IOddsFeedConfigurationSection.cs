/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using Sportradar.OddsFeed.SDK.Common;
// ReSharper disable InconsistentNaming

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    internal interface IOddsFeedConfigurationSection
    {
        /// <summary>
        /// Gets the access token
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// Gets a value specifying maximum allowed feed inactivity window
        /// </summary>
        int InactivitySeconds { get; }

        /// <summary>
        /// Gets the URL of the messaging broker
        /// </summary>
        string Host { get; }

        /// <summary>
        /// Gets the name of the virtual host configured on the messaging broker
        /// </summary>
        string VirtualHost { get; }

        /// <summary>
        /// Gets the port used to connect to the messaging broker
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets the username used to connect to the messaging broker
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Gets the password used to connect to the messaging broker
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Gets the URL of the API host
        /// </summary>
        string ApiHost { get; }

        /// <summary>
        /// Gets a value indicating whether a secure connection to the messaging broker should be used
        /// </summary>
        bool UseSSL { get; }

        /// <summary>
        /// Gets a value indicating whether a secure connection to the Sports API should be used
        /// </summary>
        bool UseApiSSL { get; }

        /// <summary>
        /// Gets the 2-letter ISO string of default language
        /// </summary>
        /// <example>https://msdn.microsoft.com/en-us/goglobal/bb896001.aspx</example>
        string DefaultLanguage { get; }

        /// <summary>
        /// Gets the comma delimited string of all wanted languages
        /// </summary>
        /// <example>https://msdn.microsoft.com/en-us/goglobal/bb896001.aspx</example>
        string SupportedLanguages { get; }

        /// <summary>
        /// Is statistics collecting enabled
        /// </summary>
        bool StatisticsEnabled { get; }

        /// <summary>
        /// Gets the timeout for automatically collecting statistics
        /// </summary>
        int StatisticsTimeout { get; }

        /// <summary>
        /// Gets the limit of records for automatically writing statistics
        /// </summary>
        int StatisticsRecordLimit { get; }

        /// <summary>
        /// Gets the file path to the configuration file for the log4net repository used by the SDK
        /// </summary>
        string SdkLogConfigPath { get; }

        /// <summary>
        /// Gets a value indicating whether the unified feed integration environment should be used
        /// </summary>
        [Obsolete("Deprecated in favor of UfEnvironment property - it provides more options then just Integration or Production")]
        bool UseIntegrationEnvironment { get; }

        /// <summary>
        /// Gets a <see cref="Common.ExceptionHandlingStrategy"/> enum member specifying how to handle exceptions thrown to outside callers
        /// </summary>
        ExceptionHandlingStrategy ExceptionHandlingStrategy { get; }

        /// <summary>
        /// Gets the comma delimited list of ids of disabled producers
        /// </summary>
        string DisabledProducers { get; }

        /// <summary>
        /// Gets the timeout for recovery to finish
        /// </summary>
        int MaxRecoveryTime { get; }

        /// <summary>
        /// Gets the minimal interval between recovery requests initiated by alive messages (seconds)
        /// </summary>
        int MinIntervalBetweenRecoveryRequests { get; }

        /// <summary>
        /// Gets the node id
        /// </summary>
        /// <remarks>MTS customer must set this value! Use only positive numbers; negative are reserved for internal use.</remarks>
        int NodeId { get; }

        /// <summary>
        /// Gets the indication whether the after age should be adjusted before executing recovery request
        /// </summary>
        /// <value><c>true</c> if [adjust after age]; otherwise, <c>false</c></value>
        bool AdjustAfterAge { get; }

        /// <summary>
        /// Gets a value specifying timeout set for HTTP responses
        /// </summary>
        int HttpClientTimeout { get; }

        /// <summary>
        /// Gets a value specifying timeout set for recovery HTTP responses
        /// </summary>
        int RecoveryHttpClientTimeout { get; }

        /// <summary>
        /// Gets a value indicating to which unified feed environment sdk should connect
        /// </summary>
        /// <remarks>Dependent on the other configuration, it may set MQ and API host address and port</remarks>
        SdkEnvironment? UfEnvironment { get; }
    }
}