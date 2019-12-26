/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Common;
// ReSharper disable InconsistentNaming

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    public interface IOddsFeedConfigurationSection
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
        /// Gets the comma delimited string of all wanted languages
        /// </summary>
        /// <example>https://msdn.microsoft.com/en-us/goglobal/bb896001.aspx</example>
        string SupportedLanguages { get; }

        /// <summary>
        /// Gets the 2-letter ISO string of default language
        /// </summary>
        /// <example>https://msdn.microsoft.com/en-us/goglobal/bb896001.aspx</example>
        string DefaultLanguage { get; }

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
        /// Gets the node id
        /// </summary>
        /// <remarks>MTS customer must set this value! Use only positive numbers; negative are reserved for internal use.</remarks>
        int NodeId { get; }

        /// <summary>
        /// Gets the indication whether the after age should be adjusted before executing recovery request
        /// </summary>
        /// <value><c>true</c> if [adjust after age]; otherwise, <c>false</c></value>
        bool AdjustAfterAge { get; }
    }
}