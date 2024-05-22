// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Configuration;
using Sportradar.OddsFeed.SDK.Common.Enums;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    /// <summary>
    /// Represents Odds Feed SDK <see cref="ConfigurationSection"/> read from app.config file
    /// </summary>
    internal class UofConfigurationSection : ConfigurationSection, IUofConfigurationSection
    {
        /// <summary>
        /// The name of the section element in the app.config file
        /// </summary>
        internal const string SectionName = "uofSdkSection";

        /// <summary>
        /// Gets the access token
        /// </summary>
        [ConfigurationProperty("accessToken", IsRequired = true)]
        public string AccessToken => (string)base["accessToken"];

        /// <summary>
        /// Gets the 2-letter ISO string of default language
        /// </summary>
        /// <example>https://msdn.microsoft.com/en-us/goglobal/bb896001.aspx</example>
        [ConfigurationProperty("defaultLanguage", IsRequired = false, DefaultValue = null)]
        public string DefaultLanguage => (string)base["defaultLanguage"];

        /// <summary>
        /// Gets the comma delimited string of all wanted languages
        /// </summary>
        /// <example>https://msdn.microsoft.com/en-us/goglobal/bb896001.aspx</example>
        [ConfigurationProperty("desiredLanguages", IsRequired = false, DefaultValue = null)]
        public string Languages => (string)base["desiredLanguages"];

        /// <summary>
        /// Gets the URL of the messaging broker
        /// </summary>
        [ConfigurationProperty("host", IsRequired = false, DefaultValue = null)]
        public string RabbitHost => (string)base["host"];

        /// <summary>
        /// Gets the port used to connect to the messaging broker
        /// </summary>
        [ConfigurationProperty("port", IsRequired = false, DefaultValue = 0)]
        public int RabbitPort => (int)base["port"];

        /// <summary>
        /// Gets the name of the virtual host configured on the messaging broker
        /// </summary>
        [ConfigurationProperty("virtualHost", IsRequired = false, DefaultValue = null)]
        public string RabbitVirtualHost => (string)base["virtualHost"];

        /// <summary>
        /// Gets a value indicating whether a secure connection to the messaging broker should be used
        /// </summary>
        [ConfigurationProperty("useSsl", IsRequired = false, DefaultValue = true)]
        public bool RabbitUseSsl => (bool)base["useSsl"];

        /// <summary>
        /// Gets the username used to connect to the messaging broker
        /// </summary>
        [ConfigurationProperty("username", IsRequired = false, DefaultValue = null)]
        public string RabbitUsername => (string)base["username"];

        /// <summary>
        /// Gets the password used to connect to the messaging broker
        /// </summary>
        [ConfigurationProperty("password", IsRequired = false, DefaultValue = null)]
        public string RabbitPassword => (string)base["password"];

        /// <summary>
        /// Gets the URL of the API host
        /// </summary>
        [ConfigurationProperty("apiHost", IsRequired = false, DefaultValue = null)]
        public string ApiHost => (string)base["apiHost"];

        /// <summary>
        /// Gets a value indicating whether a secure connection to the Sports API should be used
        /// </summary>
        [ConfigurationProperty("apiUseSsl", IsRequired = false, DefaultValue = true)]
        public bool ApiUseSsl => (bool)base["apiUseSsl"];

        /// <summary>
        /// Gets a <see cref="ExceptionHandlingStrategy"/> enum member specifying how to handle exceptions thrown to outside callers
        /// </summary>
        [ConfigurationProperty("exceptionHandlingStrategy", IsRequired = false, DefaultValue = ExceptionHandlingStrategy.Catch)]
        public ExceptionHandlingStrategy ExceptionHandlingStrategy => (ExceptionHandlingStrategy)base["exceptionHandlingStrategy"];

        /// <summary>
        /// Gets the comma delimited list of ids of disabled producers
        /// </summary>
        [ConfigurationProperty("disabledProducers", IsRequired = false)]
        public string DisabledProducers => (string)base["disabledProducers"];

        /// <summary>
        /// Gets the node id
        /// </summary>
        [ConfigurationProperty("nodeId", IsRequired = false, DefaultValue = 0)]
        public int NodeId => (int)base["nodeId"];

        /// <summary>
        /// Gets a value indicating to which unified feed environment sdk should connect
        /// </summary>
        /// <remarks>Dependent on the other configuration, it may set MQ and API host address and port</remarks>
        [ConfigurationProperty("environment", IsRequired = false, DefaultValue = SdkEnvironment.Integration)]
        public SdkEnvironment Environment => (SdkEnvironment)base["environment"];

        /// <summary>
        /// Retrieves the <see cref="UofConfigurationSection"/> from the app.config file
        /// </summary>
        /// <returns>The <see cref="UofConfigurationSection"/> instance loaded from config file</returns>
        /// <exception cref="InvalidOperationException">The configuration could not be loaded or the configuration does not contain the requested section</exception>
        /// <exception cref="ConfigurationErrorsException">The section in the configuration file is not valid</exception>
        internal static UofConfigurationSection GetSection()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config == null)
            {
                throw new InvalidOperationException("Could not load exe configuration");
            }

            var section = (UofConfigurationSection)config.GetSection(SectionName);
            if (section == null)
            {
                throw new InvalidOperationException($"Could not retrieve section {SectionName} from exe configuration");
            }
            return section;
        }
    }
}
