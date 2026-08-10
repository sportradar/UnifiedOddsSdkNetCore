// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Common.Enums;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    /// <summary>
    /// Class EnvironmentManager
    /// </summary>
    internal static class EnvironmentManager
    {
        /// <summary>
        /// The default MQ host port (using TLS)
        /// </summary>
        public const int DefaultMqHostPort = 5671;
        public const int DefaultApiSslPort = 443;

        /// <summary>
        /// Gets the list of all possible environment settings (Custom is not listed, as user should manually put MQ and API host)
        /// </summary>
        /// <value>The list of environment settings.</value>
        public static List<EnvironmentSetting> EnvironmentSettings { get; }

        private const string UsageHostForProduction = "https://usage.uofsdk.betradar.com";
        private const string UsageHostForIntegration = "https://usage-int.uofsdk.betradar.com";

        private const string AuthenticationHostForProduction = "auth.sportradar.com";
        private const string AuthenticationHostForIntegration = "stg-auth.sportradar.com";

        static EnvironmentManager()
        {
            EnvironmentSettings = new List<EnvironmentSetting>
                                      {
                                          new EnvironmentSetting(SdkEnvironment.Production, "mq.betradar.com", "api.betradar.com", true, new List<SdkEnvironment> { SdkEnvironment.Integration }),
                                          new EnvironmentSetting(SdkEnvironment.Integration, "stgmq.betradar.com", "stgapi.betradar.com", true, new List<SdkEnvironment> { SdkEnvironment.Production }),
                                          new EnvironmentSetting(SdkEnvironment.ReplayWithIntegrationCredentials, "replaymq.betradar.com", "stgapi.betradar.com", true),
                                          new EnvironmentSetting(SdkEnvironment.ReplayWithProductionCredentials, "replaymq.betradar.com", "api.betradar.com", true),
                                          new EnvironmentSetting(SdkEnvironment.GlobalProduction, "global.mq.betradar.com", "global.api.betradar.com", true, new List<SdkEnvironment> { SdkEnvironment.Integration, SdkEnvironment.Production }),
                                          new EnvironmentSetting(SdkEnvironment.GlobalIntegration, "global.stgmq.betradar.com", "global.stgapi.betradar.com", true, new List<SdkEnvironment> { SdkEnvironment.Integration, SdkEnvironment.Production }),
                                      };
        }

        /// <summary>
        /// Gets the MQ and API settings for specified <see cref="SdkEnvironment"/>
        /// </summary>
        /// <param name="environment">The environment</param>
        /// <returns>The MQ and API settings for specified <see cref="SdkEnvironment"/></returns>
        public static EnvironmentSetting GetSetting(SdkEnvironment environment)
        {
            var setting = EnvironmentSettings.Find(f => f.Environment.Equals(environment));
            return setting;
        }

        /// <summary>
        /// Gets the MQ host for specified <see cref="SdkEnvironment"/>
        /// </summary>
        /// <param name="environment">The environment</param>
        /// <returns>The MQ host for specified <see cref="SdkEnvironment"/></returns>
        public static string GetMqHost(SdkEnvironment environment)
        {
            var setting = EnvironmentSettings.Find(f => f.Environment.Equals(environment));
            if (setting != null)
            {
                return setting.MqHost;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the API host for specified <see cref="SdkEnvironment"/>
        /// </summary>
        /// <param name="environment">The environment</param>
        /// <returns>The API host for specified <see cref="SdkEnvironment"/></returns>
        public static string GetApiHost(SdkEnvironment environment)
        {
            var setting = EnvironmentSettings.Find(f => f.Environment.Equals(environment));
            if (setting != null)
            {
                return setting.ApiHost;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the API host for specified <see cref="SdkEnvironment"/>
        /// </summary>
        /// <param name="environment">The environment</param>
        /// <returns>The API host for specified <see cref="SdkEnvironment"/></returns>
        public static string GetUsageHost(SdkEnvironment environment)
        {
            return environment == SdkEnvironment.Production
                   || environment == SdkEnvironment.GlobalProduction
                       ? UsageHostForProduction
                       : UsageHostForIntegration;
        }

        /// <summary>
        /// Gets the Authentication API host for specified <see cref="SdkEnvironment"/>
        /// </summary>
        /// <param name="environment">The environment</param>
        /// <returns>The Authentication API host for specified <see cref="SdkEnvironment"/></returns>
        public static string GetAuthenticationHost(SdkEnvironment environment)
        {
            switch (environment)
            {
                case SdkEnvironment.Production:
                case SdkEnvironment.GlobalProduction:
                case SdkEnvironment.ReplayWithProductionCredentials:
                    return AuthenticationHostForProduction;

                case SdkEnvironment.Integration:
                case SdkEnvironment.GlobalIntegration:
                case SdkEnvironment.ReplayWithIntegrationCredentials:
                    return AuthenticationHostForIntegration;

                case SdkEnvironment.Custom:
                default:
                    throw new ArgumentOutOfRangeException(
                                                          nameof(environment),
                                                          environment,
                                                          "Unsupported SDK environment value."
                                                         );
            }
        }
    }
}
