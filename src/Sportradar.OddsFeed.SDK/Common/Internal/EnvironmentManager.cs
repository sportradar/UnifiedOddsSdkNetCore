using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// Class EnvironmentManager
    /// </summary>
    internal static class EnvironmentManager
    {
        /// <summary>
        /// The default MQ host port
        /// </summary>
        public const int DefaultMqHostPort = 5671;

        /// <summary>
        /// Gets the list of all possible environment settings (Custom is not listed, as user should manually put MQ and API host)
        /// </summary>
        /// <value>The list of environment settings.</value>
        public static List<EnvironmentSetting> EnvironmentSettings { get; }

        static EnvironmentManager()
        {
            var basicRetryList = new List<SdkEnvironment>
                            {
                                SdkEnvironment.Integration,
                                SdkEnvironment.Production
                            };
            EnvironmentSettings = new List<EnvironmentSetting>
                  {
                      new EnvironmentSetting(SdkEnvironment.Production, "mq.betradar.com", "api.betradar.com", true, new List<SdkEnvironment> {SdkEnvironment.Integration}),
                      new EnvironmentSetting(SdkEnvironment.Integration, "stgmq.betradar.com", "stgapi.betradar.com", true, new List<SdkEnvironment> {SdkEnvironment.Production}),
                      new EnvironmentSetting(SdkEnvironment.Replay, "replaymq.betradar.com", "stgapi.betradar.com", true, basicRetryList),
                      new EnvironmentSetting(SdkEnvironment.GlobalProduction, "global.mq.betradar.com", "global.api.betradar.com", true, basicRetryList),
                      new EnvironmentSetting(SdkEnvironment.GlobalIntegration, "global.stgmq.betradar.com", "global.stgapi.betradar.com", true, basicRetryList),
                      new EnvironmentSetting(SdkEnvironment.ProxySingapore, "mq.ap-southeast-1.betradar.com", "api.ap-southeast-1.betradar.com", true, basicRetryList),
                      new EnvironmentSetting(SdkEnvironment.ProxyTokyo, "mq.ap-northeast-1.betradar.com", "api.ap-northeast-1.betradar.com", true, basicRetryList)
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
    }
}
