/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Configuration;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Represents Odds Feed SDK <see cref="ConfigurationSection"/> read from app.config file
    /// </summary>
    internal class OddsFeedConfigurationSection : ConfigurationSection, IOddsFeedConfigurationSection
    {
        /// <summary>
        /// The name of the section element in the app.config file
        /// </summary>
        private const string SectionName = "oddsFeedSection";

        /// <summary>
        /// Gets the access token
        /// </summary>
        [ConfigurationProperty("accessToken", IsRequired = true)]
        public string AccessToken => (string) base["accessToken"];

        /// <summary>
        /// Gets a value specifying maximum allowed feed inactivity window
        /// </summary>
        [ConfigurationProperty("inactivitySeconds", IsRequired = false, DefaultValue=20)]
        [IntegerValidator(MinValue = SdkInfo.MinInactivitySeconds, MaxValue = SdkInfo.MaxInactivitySeconds, ExcludeRange = false)]
        public int InactivitySeconds => (int) base["inactivitySeconds"];

        /// <summary>
        /// Gets the URL of the messaging broker
        /// </summary>
        [ConfigurationProperty("host", IsRequired = false, DefaultValue = null)]
        public string Host => (string) base["host"];

        /// <summary>
        /// Gets the name of the virtual host configured on the messaging broker
        /// </summary>
        [ConfigurationProperty("virtualHost", IsRequired=false, DefaultValue = null)]
        public string VirtualHost => (string) base["virtualHost"];

        /// <summary>
        /// Gets the port used to connect to the messaging broker
        /// </summary>
        [ConfigurationProperty("port", IsRequired = false, DefaultValue = 0)]
        public int Port => (int) base["port"];

        /// <summary>
        /// Gets the username used to connect to the messaging broker
        /// </summary>
        [ConfigurationProperty("username", IsRequired = false, DefaultValue = null)]
        public string Username => (string) base["username"];

        /// <summary>
        /// Gets the password used to connect to the messaging broker
        /// </summary>
        [ConfigurationProperty("password", IsRequired = false, DefaultValue = null)]
        public string Password => (string) base["password"];

        /// <summary>
        /// Gets the URL of the API host
        /// </summary>
        [ConfigurationProperty("apiHost", IsRequired = false, DefaultValue = null)]
        public string ApiHost => (string) base["apiHost"];

        /// <summary>
        /// Gets a value indicating whether a secure connection to the messaging broker should be used
        /// </summary>
        [ConfigurationProperty("useSSL", IsRequired = false, DefaultValue = true)]
        // ReSharper disable once InconsistentNaming
        public bool UseSSL => (bool) base["useSSL"];

        /// <summary>
        /// Gets a value indicating whether a secure connection to the Sports API should be used
        /// </summary>
        [ConfigurationProperty("useApiSSL", IsRequired = false, DefaultValue = true)]
        // ReSharper disable once InconsistentNaming
        public bool UseApiSSL => (bool) base["useApiSSL"];

        /// <summary>
        /// Gets the 2-letter ISO string of default language
        /// </summary>
        /// <example>https://msdn.microsoft.com/en-us/goglobal/bb896001.aspx</example>
        [ConfigurationProperty("defaultLanguage", IsRequired = false, DefaultValue = null)]
        public string DefaultLanguage => (string) base["defaultLanguage"];

        /// <summary>
        /// Gets the comma delimited string of all wanted languages
        /// </summary>
        /// <example>https://msdn.microsoft.com/en-us/goglobal/bb896001.aspx</example>
        [ConfigurationProperty("supportedLanguages", IsRequired = false, DefaultValue = null)]
        public string SupportedLanguages => (string) base["supportedLanguages"];

        /// <summary>
        /// Is statistics collecting enabled
        /// </summary>
        [ConfigurationProperty("statisticsEnabled", IsRequired = false, DefaultValue = false)]
        public bool StatisticsEnabled => (bool) base["statisticsEnabled"];

        /// <summary>
        /// Gets the timeout for automatically collecting statistics
        /// </summary>
        [ConfigurationProperty("statisticsTimeout", IsRequired = false, DefaultValue = 1800)]
        public int StatisticsTimeout => (int) base["statisticsTimeout"];

        /// <summary>
        /// Gets the limit of records for automatically writing statistics
        /// </summary>
        [ConfigurationProperty("statisticsMaxRecord", IsRequired = false, DefaultValue = 1000000)]
        public int StatisticsRecordLimit => (int) base["statisticsMaxRecord"];

        /// <summary>
        /// Gets the file path to the configuration file for the log4net repository used by the SDK
        /// </summary>
        [ConfigurationProperty("sdkLogConfigPath", IsRequired = false)]
        public string SdkLogConfigPath => (string) base["sdkLogConfigPath"];

        /// <summary>
        /// Gets a value indicating whether the unified feed integration environment should be used
        /// </summary>
        /// <!-- September 2021 -->
        [Obsolete("Deprecated in favor of ufEnvironment attribute - it provides more options then just Integration or Production")]
        [ConfigurationProperty("useIntegrationEnvironment", IsRequired = false, DefaultValue = false)]
        public bool UseIntegrationEnvironment => (bool) base["useIntegrationEnvironment"];

        /// <summary>
        /// Gets a <see cref="Common.ExceptionHandlingStrategy"/> enum member specifying how to handle exceptions thrown to outside callers
        /// </summary>
        [ConfigurationProperty("exceptionHandlingStrategy", IsRequired = false, DefaultValue = ExceptionHandlingStrategy.CATCH)]
        public ExceptionHandlingStrategy ExceptionHandlingStrategy => (ExceptionHandlingStrategy) base["exceptionHandlingStrategy"];

        /// <summary>
        /// Gets the comma delimited list of ids of disabled producers
        /// </summary>
        [ConfigurationProperty("disabledProducers", IsRequired = false)]
        public string DisabledProducers => (string) base["disabledProducers"];

        /// <summary>
        /// Gets the timeout for recovery to finish (in seconds)
        /// </summary>
        [ConfigurationProperty("maxRecoveryTime", IsRequired = false, DefaultValue = SdkInfo.MaxRecoveryExecutionInSeconds)]
        [IntegerValidator(MinValue = SdkInfo.MinRecoveryExecutionInSeconds, MaxValue = SdkInfo.MaxRecoveryExecutionInSeconds, ExcludeRange = false)]
        public int MaxRecoveryTime => (int) base["maxRecoveryTime"];

        /// <summary>
        /// Gets the minimal interval between recovery requests initiated by alive messages (seconds)
        /// </summary>
        [ConfigurationProperty("minIntervalBetweenRecoveryRequests", IsRequired = false, DefaultValue = SdkInfo.DefaultIntervalBetweenRecoveryRequests)]
        [IntegerValidator(MinValue = SdkInfo.MinIntervalBetweenRecoveryRequests, MaxValue = SdkInfo.MaxIntervalBetweenRecoveryRequests, ExcludeRange = false)]
        public int MinIntervalBetweenRecoveryRequests => (int)base["minIntervalBetweenRecoveryRequests"];

        /// <summary>
        /// Gets the node id
        /// </summary>
        [ConfigurationProperty("nodeId", IsRequired = false, DefaultValue = 0)]
        public int NodeId => (int) base["nodeId"];

        /// <summary>
        /// Gets the indication whether the after age should be adjusted before executing recovery request
        /// </summary>
        [ConfigurationProperty("adjustAfterAge", IsRequired = false, DefaultValue = false)]
        public bool AdjustAfterAge => (bool) base["adjustAfterAge"];

        /// <summary>
        /// Gets a value specifying timeout set for HTTP responses
        /// </summary>
        [ConfigurationProperty("httpClientTimeout", IsRequired = false, DefaultValue = SdkInfo.DefaultHttpClientTimeout)]
        [IntegerValidator(MinValue = SdkInfo.MinHttpClientTimeout, MaxValue = SdkInfo.MaxHttpClientTimeout, ExcludeRange = false)]
        public int HttpClientTimeout => (int)base["httpClientTimeout"];

        /// <summary>
        /// Gets a value specifying timeout set for recovery HTTP responses
        /// </summary>
        [ConfigurationProperty("recoveryHttpClientTimeout", IsRequired = false, DefaultValue = SdkInfo.DefaultHttpClientTimeout)]
        [IntegerValidator(MinValue = SdkInfo.MinHttpClientTimeout, MaxValue = SdkInfo.MaxHttpClientTimeout, ExcludeRange = false)]
        public int RecoveryHttpClientTimeout => (int)base["recoveryHttpClientTimeout"];

        /// <summary>
        /// Gets a value indicating to which unified feed environment sdk should connect
        /// </summary>
        /// <remarks>Dependent on the other configuration, it may set MQ and API host address and port</remarks>
        [ConfigurationProperty("ufEnvironment", IsRequired = false, DefaultValue = null)]
        public SdkEnvironment? UfEnvironment => (SdkEnvironment?) base["ufEnvironment"];

        /// <summary>
        /// Retrieves the <see cref="OddsFeedConfigurationSection"/> from the app.config file
        /// </summary>
        /// <returns>The <see cref="OddsFeedConfigurationSection"/> instance loaded from config file</returns>
        /// <exception cref="InvalidOperationException">The configuration could not be loaded or the configuration does not contain the requested section</exception>
        /// <exception cref="ConfigurationErrorsException">The section in the configuration file is not valid</exception>
        internal static OddsFeedConfigurationSection GetSection()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config == null)
            {
                throw new InvalidOperationException("Could not load exe configuration");
            }

            var section = (OddsFeedConfigurationSection) config.GetSection(SectionName);
            if (section == null)
            {
                throw new InvalidOperationException($"Could not retrieve section {SectionName} from exe configuration");
            }
            return section;
        }
    }
}
