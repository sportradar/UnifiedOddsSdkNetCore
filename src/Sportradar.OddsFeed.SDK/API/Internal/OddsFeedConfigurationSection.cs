/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Microsoft.Extensions.Configuration;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using System;
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Represents Odds Feed SDK <see cref="ConfigurationSection"/> read from app.config file
    /// </summary>
    internal class OddsFeedConfigurationSection : IOddsFeedConfigurationSection
    {
        /// <summary>
        /// The name of the section element in the app.config file
        /// </summary>
        private const string SectionName = "oddsFeedSection";

        /// <summary>
        /// Gets the access token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets a value specifying maximum allowed feed inactivity window
        /// </summary>
        public int InactivitySeconds { get; set; } = 20;

        /// <summary>
        /// Gets the URL of the messaging broker
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets the name of the virtual host configured on the messaging broker
        /// </summary>
        public string VirtualHost { get; set; }

        /// <summary>
        /// Gets the exchange to which queues are bound on the messaging broker
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// Gets the port used to connect to the messaging broker
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets the username used to connect to the messaging broker
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets the password used to connect to the messaging broker
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets the URL of the API host
        /// </summary>
        public string ApiHost { get; set; }

        /// <summary>
        /// Gets a value indicating whether a secure connection to the messaging broker should be used
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public bool UseSSL { get; set; } = true;

        /// <summary>
        /// Gets a value indicating whether a secure connection to the Sports API should be used
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public bool UseApiSSL { get; set; } = true;

        /// <summary>
        /// Gets the comma delimited string of all wanted languages
        /// </summary>
        /// <example>https://msdn.microsoft.com/en-us/goglobal/bb896001.aspx</example>
        public string SupportedLanguages { get; set; }

        /// <summary>
        /// Gets the 2-letter ISO string of default language
        /// </summary>
        /// <example>https://msdn.microsoft.com/en-us/goglobal/bb896001.aspx</example>
        public string DefaultLanguage { get; set; }

        /// <summary>
        /// Is statistics collecting enabled
        /// </summary>
        public bool StatisticsEnabled { get; set; }

        /// <summary>
        /// Gets the timeout for automatically collecting statistics
        /// </summary>
        public int StatisticsTimeout { get; set; } = 1800;

        /// <summary>
        /// Gets the limit of records for automatically writing statistics
        /// </summary>
        public int StatisticsRecordLimit { get; set; } = 1000000;

        /// <summary>
        /// Gets the file path to the configuration file for the log4net repository used by the SDK
        /// </summary>
        public string SdkLogConfigPath { get; set; }

        /// <summary>
        /// Gets a value indicating whether the unified feed integration environment should be used
        /// </summary>
        public bool UseIntegrationEnvironment { get; set; }

        /// <summary>
        /// Gets a <see cref="Common.ExceptionHandlingStrategy"/> enum member specifying how to handle exceptions thrown to outside callers
        /// </summary>
        public ExceptionHandlingStrategy ExceptionHandlingStrategy { get; set; } = ExceptionHandlingStrategy.CATCH;

        /// <summary>
        /// Gets the comma delimited list of ids of disabled producers
        /// </summary>
        public string DisabledProducers { get; set; }

        /// <summary>
        /// Gets the timeout for recovery to finish
        /// </summary>
        public int MaxRecoveryTime { get; set; } = SdkInfo.MaxRecoveryExecutionInSeconds;

        /// <summary>
        /// Gets the minimal interval between recovery requests initiated by alive messages (seconds)
        /// </summary>
        public int MinIntervalBetweenRecoveryRequests { get; set; } = SdkInfo.DefaultIntervalBetweenRecoveryRequests;

        /// <summary>
        /// Gets the node id
        /// </summary>
        public int NodeId { get; set; }

        /// <summary>
        /// Gets the indication whether the after age should be adjusted before executing recovery request
        /// </summary>
        public bool AdjustAfterAge { get; set; }

        /// <summary>
        /// Gets a value specifying timeout set for HTTP responses
        /// </summary>
        public int HttpClientTimeout { get; set; } = SdkInfo.DefaultHttpClientTimeout;

        /// <summary>
        /// Gets a value specifying timeout set for recovery HTTP responses
        /// </summary>
        public int RecoveryHttpClientTimeout { get; set; } = SdkInfo.DefaultHttpClientTimeout;

        /// <summary>
        /// Provider api endpoints
        /// </summary>
        public Dictionary<string, Endpoint> Endpoints { get; set; }

        /// <summary>
        /// Retrieves the <see cref="OddsFeedConfigurationSection"/> from the app.config file
        /// </summary>
        /// <returns>The <see cref="OddsFeedConfigurationSection"/> instance loaded from config file</returns>
        /// <exception cref="InvalidOperationException">The configuration could not be loaded or the configuration does not contain the requested section</exception>
        internal static OddsFeedConfigurationSection GetSection(IConfiguration config)
        {
            if (config == null)
            {
                throw new InvalidOperationException("Could not load exe configuration");
            }

            var section = config.GetSection(SectionName).Get<OddsFeedConfigurationSection>();

            if (section == null)
            {
                throw new InvalidOperationException($"Could not retrieve section {SectionName} from exe configuration");
            }

            return section;
        }
    }
}