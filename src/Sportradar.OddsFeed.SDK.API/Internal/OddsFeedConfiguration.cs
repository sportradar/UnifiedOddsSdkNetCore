/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Represents SDK configuration
    /// </summary>
    internal class OddsFeedConfiguration : IOddsFeedConfiguration
    {
        private readonly ILog _executionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(OddsFeedConfiguration));

        /// <summary>
        /// Gets the access token used when accessing feed's REST interface
        /// </summary>
        public string AccessToken { get; }

        /// <summary>
        /// Gets the maximum allowed timeout in seconds, between consecutive AMQP messages associated with the same producer.
        /// If this value is exceeded, the producer is considered to be down
        /// </summary>
        public int InactivitySeconds { get; }

        /// <summary>
        /// Gets a <see cref="CultureInfo" /> specifying default locale to which translatable values will be translated
        /// </summary>
        /// <value>The default locale</value>
        public CultureInfo DefaultLocale { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{CultureInfo}"/> specifying locales (languages) to which translatable values will be translated
        /// </summary>
        public IEnumerable<CultureInfo> Locales { get; }

        /// <summary>
        /// Gets the comma delimited list of ids of disabled producers (default: none)
        /// </summary>
        /// <value>The list of ids of disabled producers</value>
        public IEnumerable<int> DisabledProducers { get; }

        /// <summary>
        /// Gets the maximum recovery time
        /// </summary>
        /// <value>The maximum recovery time</value>
        public int MaxRecoveryTime { get; }

        /// <summary>
        /// Gets the node identifier
        /// </summary>
        /// <remarks>MTS customer must set this value! Use only positive numbers; negative are reserved for internal use.</remarks>
        /// <value>The node identifier</value>
        public int NodeId { get; }

        /// <summary>
        /// Gets the <see cref="SdkEnvironment"/> value specifying the environment to which to connect.
        /// </summary>
        public SdkEnvironment Environment { get; }

        /// <summary>
        /// Gets the exception handling strategy
        /// </summary>
        /// <value>The exception handling strategy</value>
        public ExceptionHandlingStrategy ExceptionHandlingStrategy { get; }

        /// <summary>
        /// Gets a value specifying the host name of the AQMP broker
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// Gets the port used for connecting to the AQMP broker
        /// </summary>
        /// <value>The port</value>
        public int Port { get; }

        /// <summary>
        /// Gets the user name for connecting to the AQMP broker
        /// </summary>
        /// <value>The username</value>
        public string Username { get; }

        /// <summary>
        /// Gets the password for connecting to the AQMP broker
        /// </summary>
        /// <value>The password</value>
        public string Password { get; }

        /// <summary>
        /// Gets a value specifying the virtual host of the AQMP broker
        /// </summary>
        /// <value>The virtual host</value>
        public string VirtualHost { get; set; }

        /// <summary>
        /// Gets a value indicating whether a secure connection to the messaging server should be used
        /// </summary>
        public bool UseSsl { get; }

        /// <summary>
        /// Gets a host name of the Sports API
        /// </summary>
        public string ApiHost { get; }

        /// <summary>
        /// Gets a value indicating whether the connection to Sports API should use SSL
        /// </summary>
        public bool UseApiSsl { get; }

        /// <summary>
        /// Gets a value indicating whether the after age should be enforced before executing recovery request
        /// </summary>
        /// <value><c>true</c> if [enforce after age]; otherwise, <c>false</c></value>
        public bool AdjustAfterAge { get; }

        /// <summary>
        /// Gets the <see cref="IOddsFeedConfigurationSection"/> used to obtain 'hidden' properties
        /// </summary>
        /// <value>The <see cref="IOddsFeedConfigurationSection"/> used to obtain 'hidden' properties</value>
        internal IOddsFeedConfigurationSection Section { get; }

        public OddsFeedConfiguration(
            string accessToken,
            int inactivitySeconds,
            List<CultureInfo> requiredLanguages,
            List<int> disabledProducers,
            string apiHost,
            string host,
            string virtualHost,
            bool useSsl,
            int maxRecoveryTimeInSeconds,
            bool useIntegrationEnvironment,
            int nodeId,
            ExceptionHandlingStrategy exceptionHandlingStrategy,
            bool adjustAfterAge,
            IOddsFeedConfigurationSection section){

            Contract.Requires(!string.IsNullOrEmpty(accessToken));
            Contract.Requires(inactivitySeconds >= SdkInfo.MinInactivitySeconds && inactivitySeconds <= SdkInfo.MaxInactivitySeconds);
            Contract.Requires(!string.IsNullOrEmpty(apiHost));
            Contract.Requires(!string.IsNullOrEmpty(host));
            Contract.Requires(requiredLanguages != null);

            AccessToken = accessToken;
            InactivitySeconds = inactivitySeconds;
            Locales = new List<CultureInfo>(requiredLanguages);
            DefaultLocale = requiredLanguages.First();
            DisabledProducers = disabledProducers != null && disabledProducers.Any()
                ? new List<int>(disabledProducers)
                : null;
            // ApiHost & UseApiSsl is set below
            Host = host;
            VirtualHost = virtualHost;
            UseSsl = useSsl;
            MaxRecoveryTime = maxRecoveryTimeInSeconds;
            NodeId = nodeId;
            if (nodeId < 0)
            {
                _executionLog.Warn($"Setting nodeId to {nodeId}. Use only positive numbers; negative are reserved for internal use.");
            }
            ExceptionHandlingStrategy = exceptionHandlingStrategy;

            //appropriately set properties introduced with new builder:
            Environment = useIntegrationEnvironment ? SdkEnvironment.Integration : SdkEnvironment.Production;
            if (apiHost.ToLower().StartsWith("http://"))
            {
                ApiHost = apiHost.Substring(7);  //remove leading http://
                UseApiSsl = false;
            }
            else if (apiHost.ToLower().StartsWith("https://"))
            {
                ApiHost = apiHost.Substring(8); //remove leading https://
                UseApiSsl = true;
            }
            else
            {
                ApiHost = apiHost;
                UseApiSsl = true;
            }
            Port = useSsl ? SdkInfo.DefaultHostPort : 5672;
            Username = accessToken;
            Password = null;
            AdjustAfterAge = adjustAfterAge;
            Section = section;
        }

        public OddsFeedConfiguration(
            string accessToken,
            SdkEnvironment environment,
            CultureInfo defaultCulture,
            List<CultureInfo> wantedCultures,
            string host,
            string virtualHost,
            int port,
            string username,
            string password,
            string apiHost,
            bool useSsl,
            bool useApiSsl,
            int inactivitySeconds,
            int maxRecoveryExecutionInSeconds,
            int nodeId,
            List<int> disabledProducers,
            ExceptionHandlingStrategy exceptionHandlingStrategy,
            bool adjustAfterAge,
            IOddsFeedConfigurationSection section)
        {
            Contract.Requires(!string.IsNullOrEmpty(accessToken));
            Contract.Requires(defaultCulture != null);
            Contract.Requires(inactivitySeconds >= SdkInfo.MinInactivitySeconds && inactivitySeconds <= SdkInfo.MaxInactivitySeconds);

            AccessToken = accessToken;
            Environment = environment;
            DefaultLocale = defaultCulture;
            var locales = new List<CultureInfo>();
            if (wantedCultures != null && wantedCultures.Any())
            {
                locales.AddRange(wantedCultures);
            }
            if (locales.Contains(defaultCulture))
            {
                locales.Remove(defaultCulture);
            }
            locales.Insert(0, defaultCulture);
            Locales = new List<CultureInfo>(locales.Distinct());
            Host = host;
            VirtualHost = virtualHost;
            Port = port;
            Username = string.IsNullOrEmpty(username) ? accessToken : username;
            Password = password;
            UseSsl = useSsl;
            ApiHost = apiHost;
            UseApiSsl = useApiSsl;
            InactivitySeconds = inactivitySeconds;
            MaxRecoveryTime = maxRecoveryExecutionInSeconds;
            NodeId = nodeId;
            if (nodeId < 0)
            {
                _executionLog.Warn($"Setting nodeId to {nodeId}. Use only positive numbers; negative are reserved for internal use.");
            }
            DisabledProducers = disabledProducers != null && disabledProducers.Any()
                ? new List<int>(disabledProducers)
                : null;
            ExceptionHandlingStrategy = exceptionHandlingStrategy;
            AdjustAfterAge = adjustAfterAge;
            Section = section;
        }

        /// <summary>
        /// Defined field invariants needed by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!string.IsNullOrEmpty(AccessToken));
            Contract.Invariant(DefaultLocale != null);
            Contract.Invariant(Locales != null && Locales.Any());
            Contract.Invariant(InactivitySeconds >= SdkInfo.MinInactivitySeconds && InactivitySeconds <= SdkInfo.MaxInactivitySeconds);
            Contract.Invariant(MaxRecoveryTime >= SdkInfo.MinRecoveryExecutionInSeconds);
        }
    }
}
