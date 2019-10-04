/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Configuration;
using System.Diagnostics.Contracts;
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
        /// Gets the comma delimited string of all wanted languages
        /// </summary>
        /// <example>https://msdn.microsoft.com/en-us/goglobal/bb896001.aspx</example>
        [ConfigurationProperty("supportedLanguages", IsRequired = false, DefaultValue = null)]
        public string SupportedLanguages => (string) base["supportedLanguages"];

        /// <summary>
        /// Gets the 2-letter ISO string of default language
        /// </summary>
        /// <example>https://msdn.microsoft.com/en-us/goglobal/bb896001.aspx</example>
        [ConfigurationProperty("defaultLanguage", IsRequired = false, DefaultValue = null)]
        public string DefaultLanguage => (string) base["defaultLanguage"];

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
        [ConfigurationProperty("useStagingEnvironment", IsRequired = false, DefaultValue = false)]
        [Obsolete("Use configuration property useIntegrationEnvironment")]
        public bool UseStagingEnvironment => (bool) base["useStagingEnvironment"];

        /// <summary>
        /// Gets a value indicating whether the unified feed integration environment should be used
        /// </summary>
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
        /// Gets the timeout for recovery to finish
        /// </summary>
        [ConfigurationProperty("maxRecoveryTime", IsRequired = false, DefaultValue = SdkInfo.MaxRecoveryExecutionInSeconds)]
        [IntegerValidator(MinValue = SdkInfo.MinRecoveryExecutionInSeconds, MaxValue = SdkInfo.MaxRecoveryExecutionInSeconds, ExcludeRange = false)]
        public int MaxRecoveryTime => (int) base["maxRecoveryTime"];

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

        ///// <summary>
        ///// Gets a <see cref="SdkEnvironment"/> enum member specifying the environment sdk connects to
        ///// </summary>
        //[ConfigurationProperty("environment", IsRequired = false, DefaultValue = SdkEnvironment.Integration)]
        //public SdkEnvironment Environment => (SdkEnvironment)base["environment"];

        /// <summary>
        /// Retrieves the <see cref="OddsFeedConfigurationSection"/> from the app.config file
        /// </summary>
        /// <returns>The <see cref="OddsFeedConfigurationSection"/> instance loaded from config file</returns>
        /// <exception cref="InvalidOperationException">The configuration could not be loaded or the configuration does not contain the requested section</exception>
        /// <exception cref="ConfigurationErrorsException">The section in the configuration file is not valid</exception>
        internal static OddsFeedConfigurationSection GetSection()
        {
            Contract.Ensures(Contract.Result<OddsFeedConfigurationSection>() != null);

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config == null)
            {
                throw new InvalidOperationException("Could not load exe configuration");
            }

            var section = (OddsFeedConfigurationSection)config.GetSection(SectionName);
            if (section == null)
            {
                throw new InvalidOperationException($"Could not retrieve section {SectionName} from exe configuration");
            }
            return section;
        }
    }
}
